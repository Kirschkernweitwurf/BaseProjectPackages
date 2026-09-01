using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using Base.UtilityPackage;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Builds the node for one type and for every member it declares. Accessors, backing fields and
    /// event adders never become nodes of their own. They are redirected onto the property or event
    /// they belong to, so the graph shows the code as it was written.
    /// </summary>
    internal static class MemberCollector
    {
        private const BindingFlags DeclaredMembers = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        private const string ParameterSeparator = ", ";
        private const string ReturnSeparator = " : ";

        /// <summary>Member names of each interface, so a widely implemented one is only read once.</summary>
        private static readonly Dictionary<Type, string[]> InterfaceMemberNames = new();

        /// <summary>
        /// Empties the caches held between types. They are keyed on Type, so leaving them in place
        /// would grow the tool's own footprint with every rescan for the life of the domain.
        /// </summary>
        internal static void ResetCaches() => InterfaceMemberNames.Clear();

        /// <summary>Creates the type node and registers every member it declares.</summary>
        /// <param name="type">Type to collect.</param>
        /// <param name="registry">Registry that receives the member nodes and redirects.</param>
        /// <returns>The type node, or null when the type cannot be keyed.</returns>
        internal static TypeNodeInfo Collect(Type type, MemberRegistry registry)
        {
            if (!KeyFactory.TryForType(type, out TypeKey typeKey))
                return null;

            TypeNodeInfo node = BuildTypeNode(type, typeKey);

            // Invoke, BeginInvoke and EndInvoke on a delegate are generated, so there is nothing to show.
            if (node.Kind == ETypeKind.Delegate)
                return node;

            HashSet<int> accessorTokens = new();

            CollectProperties(type, typeKey, registry, node, accessorTokens);
            CollectEvents(type, typeKey, registry, node, accessorTokens);
            CollectFields(type, typeKey, registry, node);
            CollectConstructors(type, typeKey, registry, node);
            CollectMethods(type, typeKey, registry, node, accessorTokens);
            MarkInterfaceMembers(type, node);

            return node;
        }

        /// <summary>
        /// Flags every member whose name is declared on an interface the type implements. An implicit
        /// implementation is an ordinary public method with no override keyword, so nothing else in the
        /// metadata says that lowering its visibility would stop the type compiling.
        /// </summary>
        private static void MarkInterfaceMembers(Type type, TypeNodeInfo node)
        {
            HashSet<string> names = CollectInterfaceMemberNames(type);
            if (names.Count == 0)
                return;

            foreach (MemberNodeInfo member in node.Members)
            {
                if (names.Contains(member.Name))
                    member.ImplementsInterfaceMember = true;
            }
        }

        private static HashSet<string> CollectInterfaceMemberNames(Type type)
        {
            HashSet<string> names = new(StringComparer.Ordinal);

            if (type.IsInterface)
                return names;

            foreach (Type contract in type.GetInterfaces())
                names.UnionWith(ReadInterfaceMemberNames(contract));

            return names;
        }

        /// <summary>
        /// One interface is implemented by many types, so its member names are read once and kept. The
        /// cache holds nothing but strings keyed on types from the current domain, and a domain reload
        /// throws the whole thing away.
        /// </summary>
        private static string[] ReadInterfaceMemberNames(Type contract)
        {
            if (InterfaceMemberNames.TryGetValue(contract, out string[] cached))
                return cached;

            MemberInfo[] members = contract.GetMembers();
            string[] names = new string[members.Length];

            for (int index = 0; index < members.Length; index++)
                names[index] = members[index].Name;

            InterfaceMemberNames[contract] = names;
            return names;
        }

        private static EAccessLevel GetAccessLevel(Type type)
        {
            if (type.IsPublic || type.IsNestedPublic)
                return EAccessLevel.Public;

            if (type.IsNestedPrivate)
                return EAccessLevel.Private;

            if (type.IsNestedFamily)
                return EAccessLevel.Protected;

            return type.IsNestedFamORAssem
                ? EAccessLevel.ProtectedInternal
                : EAccessLevel.Internal;
        }

        private static TypeNodeInfo BuildTypeNode(Type type, TypeKey typeKey)
        {
            TypeNodeInfo node = new(typeKey,
                TypeNameUtility.FormatShortName(type),
                TypeNameUtility.FormatFullName(type),
                string.IsNullOrEmpty(type.Namespace)
                    ? CodebaseGraphData.GlobalNamespaceName
                    : type.Namespace,
                type.Assembly.GetName().Name,
                ResolveTypeKind(type),
                GetAccessLevel(type),
                type.IsAbstract && type.IsSealed,
                type.IsAbstract && !type.IsSealed,
                typeof(Object).IsAssignableFrom(type),
                typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                IsSealed = type.IsSealed,
                IsAttribute = typeof(Attribute).IsAssignableFrom(type),
                IsEditorWindow = UnityEntryPointCatalog.IsEditorWindow(type)
            };

            if (UnityEntryPointCatalog.TryGetEntryPointAttribute(type, out string reason)
                || UnityEntryPointCatalog.IsEngineDriven(type, out reason))
            {
                node.IsEntryPoint = true;
                node.EntryPointReason = reason;
            }

            if (UnityEntryPointCatalog.IsSuppressed(type, out string suppression))
            {
                node.IsExcludedFromFindings = true;
                node.ExclusionReason = suppression;
            }

            if (type.DeclaringType != null && KeyFactory.TryForType(type.DeclaringType, out TypeKey outer))
                node.DeclaringTypeKey = outer;

            if (type.BaseType != null && KeyFactory.TryForType(type.BaseType, out TypeKey baseKey))
                node.BaseTypeKey = baseKey;

            return node;
        }

        private static ETypeKind ResolveTypeKind(Type type)
        {
            if (type.IsInterface)
                return ETypeKind.Interface;

            if (type.IsEnum)
                return ETypeKind.Enum;

            if (typeof(Delegate).IsAssignableFrom(type))
                return ETypeKind.Delegate;

            return type.IsValueType
                ? ETypeKind.Struct
                : ETypeKind.Class;
        }

        private static void CollectProperties(Type type,
            TypeKey typeKey,
            MemberRegistry registry,
            TypeNodeInfo node,
            HashSet<int> accessorTokens)
        {
            foreach (PropertyInfo property in type.GetProperties(DeclaredMembers))
            {
                if (!KeyFactory.TryForMember(property, out MemberKey key))
                    continue;

                MethodInfo getter = property.GetMethod;
                MethodInfo setter = property.SetMethod;
                MethodInfo any = getter ?? setter;
                if (any == null)
                    continue;

                MemberNodeInfo member = new(key,
                    property.Name,
                    $"{property.Name}{ReturnSeparator}{TypeNameUtility.Format(property.PropertyType)}",
                    EMemberKind.Property,
                    GetAccessLevel(any),
                    typeKey,
                    any.IsStatic,
                    setter == null,
                    any.IsVirtual,
                    any.IsAbstract);

                ApplyAttributeEntryPoint(property, member);
                ApplyBackingFieldSerialization(type, property.Name, member);
                ApplySuppression(property, member);
                Add(registry, node, member);

                RedirectAccessor(getter, key, registry, accessorTokens);
                RedirectAccessor(setter, key, registry, accessorTokens);
            }
        }

        private static void CollectEvents(Type type,
            TypeKey typeKey,
            MemberRegistry registry,
            TypeNodeInfo node,
            HashSet<int> accessorTokens)
        {
            foreach (EventInfo declaredEvent in type.GetEvents(DeclaredMembers))
            {
                if (!KeyFactory.TryForMember(declaredEvent, out MemberKey key))
                    continue;

                MethodInfo adder = declaredEvent.AddMethod;
                MethodInfo remover = declaredEvent.RemoveMethod;
                MethodInfo any = adder ?? remover;
                if (any == null)
                    continue;

                MemberNodeInfo member = new(key,
                    declaredEvent.Name,
                    $"{declaredEvent.Name}{ReturnSeparator}{TypeNameUtility.Format(declaredEvent.EventHandlerType)}",
                    EMemberKind.Event,
                    GetAccessLevel(any),
                    typeKey,
                    any.IsStatic,
                    false,
                    any.IsVirtual,
                    any.IsAbstract);

                ApplyAttributeEntryPoint(declaredEvent, member);
                ApplySuppression(declaredEvent, member);
                Add(registry, node, member);

                RedirectAccessor(adder, key, registry, accessorTokens);
                RedirectAccessor(remover, key, registry, accessorTokens);
                RedirectAccessor(declaredEvent.RaiseMethod, key, registry, accessorTokens);
                RedirectEventBackingField(type, declaredEvent.Name, key, registry);
            }
        }

        private static void CollectFields(Type type, TypeKey typeKey, MemberRegistry registry, TypeNodeInfo node)
        {
            foreach (FieldInfo field in type.GetFields(DeclaredMembers))
            {
                // Runtime bookkeeping, such as the value__ field on every enum.
                if (field.IsSpecialName)
                    continue;

                if (!KeyFactory.TryForMember(field, out MemberKey key))
                    continue;

                // An auto property backing field is not something anyone wrote, so it folds into the property.
                if (CompilerGeneratedNameResolver.TryGetBackingPropertyName(field.Name, out string propertyName))
                {
                    if (registry.TryFindByName(typeKey, propertyName, out MemberKey propertyKey))
                        registry.Redirect(key, propertyKey);

                    continue;
                }

                // Field like events already have a node under the event name.
                if (registry.TryFindByName(typeKey, field.Name, out MemberKey existing) && !existing.Equals(key))
                    continue;

                MemberNodeInfo member = new(key,
                    field.Name,
                    $"{field.Name}{ReturnSeparator}{TypeNameUtility.Format(field.FieldType)}",
                    ResolveFieldKind(type, field),
                    GetAccessLevel(field),
                    typeKey,
                    field.IsStatic,
                    field.IsInitOnly || field.IsLiteral,
                    false,
                    false);

                if (UnityEntryPointCatalog.IsSerializedEntryPoint(field, out string reason))
                {
                    member.IsEntryPoint = true;
                    member.EntryPointReason = reason;
                }

                UnityEntryPointCatalog.CollectSerializedAliases(field, member.SerializedAliases);

                ApplyAttributeEntryPoint(field, member);
                ApplySuppression(field, member);
                Add(registry, node, member);
            }
        }

        private static void CollectConstructors(Type type,
            TypeKey typeKey,
            MemberRegistry registry,
            TypeNodeInfo node)
        {
            foreach (ConstructorInfo constructor in type.GetConstructors(DeclaredMembers))
            {
                if (!KeyFactory.TryForMember(constructor, out MemberKey key))
                    continue;

                MemberNodeInfo member = new(key,
                    constructor.Name,
                    BuildSignature(constructor, TypeNameUtility.Format(type)),
                    EMemberKind.Constructor,
                    GetAccessLevel(constructor),
                    typeKey,
                    constructor.IsStatic,
                    false,
                    false,
                    false);

                if (UnityEntryPointCatalog.IsRuntimeConstructor(constructor.IsStatic,
                        node.IsUnityObject,
                        out string reason))
                {
                    member.IsEntryPoint = true;
                    member.EntryPointReason = reason;
                }

                ApplyAttributeEntryPoint(constructor, member);
                ApplySuppression(constructor, member);
                Add(registry, node, member);
            }
        }

        private static void CollectMethods(Type type,
            TypeKey typeKey,
            MemberRegistry registry,
            TypeNodeInfo node,
            HashSet<int> accessorTokens)
        {
            foreach (MethodInfo method in type.GetMethods(DeclaredMembers))
            {
                if (!KeyFactory.TryForMember(method, out MemberKey key))
                    continue;

                if (accessorTokens.Contains(key.Token))
                    continue;

                // Lambdas and local functions are folded onto their owner in a later pass.
                if (CompilerGeneratedNameResolver.IsGeneratedName(method.Name))
                    continue;

                MemberNodeInfo member = new(key,
                    method.Name,
                    BuildSignature(method, method.Name),
                    EMemberKind.Method,
                    GetAccessLevel(method),
                    typeKey,
                    method.IsStatic,
                    false,
                    method.IsVirtual,
                    method.IsAbstract);

                if (UnityEntryPointCatalog.Inspect(method, type, out string reason, out bool isReset))
                {
                    member.IsEntryPoint = true;
                    member.EntryPointReason = reason;
                }

                member.IsStateReset = isReset;
                member.IsAnimationEventSignature = UnityEntryPointCatalog.IsAnimationEventSignature(method);

                ApplySuppression(method, member);
                Add(registry, node, member);
            }
        }

        /// <summary>
        /// An auto property written as [field: SerializeField] carries the attribute on its generated
        /// backing field, so the property itself looks untouched even though Unity writes it.
        /// </summary>
        private static void ApplyBackingFieldSerialization(Type type, string propertyName, MemberNodeInfo node)
        {
            if (node.IsEntryPoint)
                return;

            FieldInfo backing = type.GetField($"<{propertyName}>k__BackingField", DeclaredMembers);
            if (!UnityEntryPointCatalog.IsSerializedEntryPoint(backing, out string reason))
                return;

            node.IsEntryPoint = true;
            node.EntryPointReason = reason;
        }

        private static void RedirectAccessor(MethodInfo accessor,
            MemberKey ownerKey,
            MemberRegistry registry,
            HashSet<int> accessorTokens)
        {
            if (accessor == null)
                return;

            if (!KeyFactory.TryForMember(accessor, out MemberKey key))
                return;

            accessorTokens.Add(key.Token);
            registry.Redirect(key, ownerKey);
        }

        private static void RedirectEventBackingField(Type type,
            string eventName,
            MemberKey ownerKey,
            MemberRegistry registry)
        {
            FieldInfo backing = type.GetField(eventName, DeclaredMembers);
            if (backing == null)
                return;

            if (KeyFactory.TryForMember(backing, out MemberKey key))
                registry.Redirect(key, ownerKey);
        }

        private static EMemberKind ResolveFieldKind(Type type, FieldInfo field)
        {
            if (type.IsEnum && field.IsLiteral)
                return EMemberKind.EnumMember;

            if (field.IsLiteral)
                return EMemberKind.Const;

            return UnityEntryPointCatalog.IsSerialized(field)
                ? EMemberKind.SerializedField
                : EMemberKind.Field;
        }

        private static EAccessLevel GetAccessLevel(FieldInfo field)
        {
            if (field.IsPublic)
                return EAccessLevel.Public;

            if (field.IsFamily)
                return EAccessLevel.Protected;

            if (field.IsFamilyOrAssembly)
                return EAccessLevel.ProtectedInternal;

            return field.IsAssembly
                ? EAccessLevel.Internal
                : EAccessLevel.Private;
        }

        private static EAccessLevel GetAccessLevel(MethodBase method)
        {
            if (method.IsPublic)
                return EAccessLevel.Public;

            if (method.IsFamily)
                return EAccessLevel.Protected;

            if (method.IsFamilyOrAssembly)
                return EAccessLevel.ProtectedInternal;

            return method.IsAssembly
                ? EAccessLevel.Internal
                : EAccessLevel.Private;
        }

        private static void ApplyAttributeEntryPoint(MemberInfo member, MemberNodeInfo node)
        {
            if (node.IsEntryPoint)
                return;

            if (!UnityEntryPointCatalog.TryGetEntryPointAttribute(member, out string reason))
                return;

            node.IsEntryPoint = true;
            node.EntryPointReason = reason;
        }

        /// <summary>
        /// Takes a member the author has marked as deliberately out of scope. The same attribute names
        /// answer for a whole type and for one member of it, so an ignore attribute reads the same way
        /// wherever it is written. The older source line marker sets the flag from its own pass.
        /// </summary>
        private static void ApplySuppression(MemberInfo member, MemberNodeInfo node)
        {
            if (!UnityEntryPointCatalog.IsSuppressed(member, out _))
                return;

            node.IsSuppressed = true;
        }

        private static void Add(MemberRegistry registry, TypeNodeInfo node, MemberNodeInfo member)
        {
            registry.Register(member);
            node.Members.Add(member);
        }

        private static string BuildSignature(MethodBase method, string displayName)
        {
            StringBuilder builder = new(displayName);
            builder.Append('(');

            ParameterInfo[] parameters = method.GetParameters();
            for (int index = 0; index < parameters.Length; index++)
            {
                if (index > 0)
                    builder.Append(ParameterSeparator);

                builder.Append(TypeNameUtility.Format(parameters[index].ParameterType));
            }

            builder.Append(')');

            if (method is MethodInfo info)
            {
                builder.Append(ReturnSeparator);
                builder.Append(TypeNameUtility.Format(info.ReturnType));
            }

            return builder.ToString();
        }
    }
}