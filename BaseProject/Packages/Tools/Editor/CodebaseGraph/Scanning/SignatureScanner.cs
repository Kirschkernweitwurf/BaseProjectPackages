using System;
using System.Reflection;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Links the types that only appear in signatures. A field type, a parameter type, a return type
    /// and a generic argument all live in metadata and never show up as an instruction. So without this
    /// pass an enum used purely as a field type, or a data class only ever passed as an argument, looks
    /// like nothing in the project references it.
    /// </summary>
    internal static class SignatureScanner
    {
        private const BindingFlags DeclaredMembers = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        private const int MaxGenericDepth = 4;

        /// <summary>Links every type named in the signatures of one type's members.</summary>
        /// <param name="type">Type whose signatures should be read.</param>
        /// <param name="sink">Receiver for the relations.</param>
        internal static void ScanType(Type type, IUsageSink sink)
        {
            if (!KeyFactory.TryForType(type, out TypeKey typeKey))
                return;

            foreach (FieldInfo field in type.GetFields(DeclaredMembers))
                Link(field.FieldType, typeKey, sink, 0);

            foreach (PropertyInfo property in type.GetProperties(DeclaredMembers))
                Link(property.PropertyType, typeKey, sink, 0);

            foreach (EventInfo declaredEvent in type.GetEvents(DeclaredMembers))
                Link(declaredEvent.EventHandlerType, typeKey, sink, 0);

            foreach (MethodInfo method in type.GetMethods(DeclaredMembers))
            {
                Link(method.ReturnType, typeKey, sink, 0);
                LinkParameters(method, typeKey, sink);

                if (method.IsGenericMethodDefinition)
                    LinkConstraints(method.GetGenericArguments(), typeKey, sink);
            }

            foreach (ConstructorInfo constructor in type.GetConstructors(DeclaredMembers))
                LinkParameters(constructor, typeKey, sink);

            LinkConstraints(type.GetGenericArguments(), typeKey, sink);
        }

        /// <summary>
        /// Links the constraints on generic parameters. A marker interface used only as a constraint,
        /// which is the entire point of that pattern, appears nowhere else in metadata or in IL.
        /// </summary>
        private static void LinkConstraints(Type[] parameters, TypeKey typeKey, IUsageSink sink)
        {
            foreach (Type parameter in parameters)
            {
                if (!parameter.IsGenericParameter)
                    continue;

                foreach (Type constraint in parameter.GetGenericParameterConstraints())
                    Link(constraint, typeKey, sink, 0);
            }
        }

        private static void LinkParameters(MethodBase method, TypeKey typeKey, IUsageSink sink)
        {
            foreach (ParameterInfo parameter in method.GetParameters())
                Link(parameter.ParameterType, typeKey, sink, 0);
        }

        private static void Link(Type target, TypeKey typeKey, IUsageSink sink, int depth)
        {
            if (target == null || depth > MaxGenericDepth)
                return;

            if (KeyFactory.TryForType(target, out TypeKey targetKey))
                sink.AddTypeRelation(typeKey, targetKey);

            if (!target.IsGenericType)
                return;

            // A List<EAudioType> only mentions the enum through its generic argument.
            foreach (Type argument in target.GetGenericArguments())
                Link(argument, typeKey, sink, depth + 1);
        }
    }
}