using System;
using System.Reflection;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Records inheritance and interface relations. This is what keeps overrides and interface
    /// implementations out of the dead code list: nothing calls them directly, the runtime dispatches
    /// to them through the base or interface member.
    /// </summary>
    internal static class HierarchyScanner
    {
        private const BindingFlags DeclaredMembers = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        /// <summary>Scans the base type, the interfaces and the override chains of one type.</summary>
        /// <param name="type">Type to inspect.</param>
        /// <param name="registry">Registry used to fold accessors onto their property.</param>
        /// <param name="sink">Receiver for the relations.</param>
        public static void ScanType(Type type, MemberRegistry registry, IUsageSink sink)
        {
            if (!KeyFactory.TryForType(type, out TypeKey typeKey))
                return;

            ScanBaseType(type, typeKey, sink);
            ScanInterfaces(type, typeKey, registry, sink);
            ScanOverrides(type, registry, sink);
        }

        private static void ScanBaseType(Type type, TypeKey typeKey, IUsageSink sink)
        {
            if (type.BaseType == null)
                return;

            if (KeyFactory.TryForType(type.BaseType, out TypeKey baseKey))
                sink.AddTypeRelation(typeKey, baseKey);
        }

        private static void ScanInterfaces(Type type, TypeKey typeKey, MemberRegistry registry, IUsageSink sink)
        {
            foreach (Type contract in type.GetInterfaces())
            {
                if (KeyFactory.TryForType(contract, out TypeKey contractKey))
                    sink.AddTypeRelation(typeKey, contractKey);

                MapImplementations(type, contract, registry, sink);
            }
        }

        private static void MapImplementations(Type type, Type contract, MemberRegistry registry, IUsageSink sink)
        {
            if (type.IsInterface)
                return;

            InterfaceMapping mapping;

            try
            {
                mapping = type.GetInterfaceMap(contract);
            }
            catch (Exception)
            {
                // Open generic types and some Unity proxies cannot produce a map. Nothing to record then.
                return;
            }

            for (int index = 0; index < mapping.TargetMethods.Length; index++)
            {
                MethodInfo implementation = mapping.TargetMethods[index];
                MethodInfo contractMethod = mapping.InterfaceMethods[index];

                if (implementation == null || contractMethod == null)
                    continue;

                if (implementation.DeclaringType != type)
                    continue;

                Link(implementation, contractMethod, registry, sink, EUsageKind.InterfaceImplementation);
            }
        }

        private static void ScanOverrides(Type type, MemberRegistry registry, IUsageSink sink)
        {
            foreach (MethodInfo method in type.GetMethods(DeclaredMembers))
            {
                if (!method.IsVirtual)
                    continue;

                MethodInfo baseDefinition = method.GetBaseDefinition();
                if (baseDefinition == null || baseDefinition == method)
                    continue;

                Link(method, baseDefinition, registry, sink, EUsageKind.Override);
            }
        }

        private static void Link(MethodInfo source,
            MethodInfo target,
            MemberRegistry registry,
            IUsageSink sink,
            EUsageKind kind)
        {
            if (!KeyFactory.TryForMember(source, out MemberKey rawSource))
                return;

            if (!KeyFactory.TryForMember(target, out MemberKey rawTarget))
                return;

            MemberKey sourceKey = registry.Resolve(rawSource);
            MemberNodeInfo node = registry.Find(sourceKey);
            if (node == null)
                return;

            node.IsOverride = true;

            if (kind == EUsageKind.InterfaceImplementation)
                CountImplementation(registry, rawTarget);

            sink.AddMemberUsage(sourceKey, rawTarget, kind);
        }

        private static void CountImplementation(MemberRegistry registry, MemberKey contractKey)
        {
            MemberNodeInfo contract = registry.Find(registry.Resolve(contractKey));
            if (contract == null)
                return;

            contract.ImplementationCount++;
        }
    }
}