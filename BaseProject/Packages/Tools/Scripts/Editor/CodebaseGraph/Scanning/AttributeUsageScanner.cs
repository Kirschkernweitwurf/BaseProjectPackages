using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Records attribute usage. Attributes live in metadata rather than in IL, so without this pass
    /// every attribute class in the project would look like it is never used.
    /// </summary>
    public static class AttributeUsageScanner
    {
        private const BindingFlags DeclaredMembers = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        /// <summary>Scans the attributes on a type and on every member it declares.</summary>
        /// <param name="type">Type to inspect.</param>
        /// <param name="registry">Registry used to fold accessors onto their property.</param>
        /// <param name="sink">Receiver for the usages.</param>
        public static void ScanType(Type type, MemberRegistry registry, IUsageSink sink)
        {
            if (!KeyFactory.TryForType(type, out TypeKey typeKey))
                return;

            foreach (CustomAttributeData attribute in Read(type))
                ReportOnType(attribute, typeKey, sink);

            foreach (MemberInfo member in type.GetMembers(DeclaredMembers))
                ScanMember(member, typeKey, registry, sink);
        }

        private static void ScanMember(MemberInfo member, TypeKey typeKey, MemberRegistry registry, IUsageSink sink)
        {
            IList<CustomAttributeData> attributes = Read(member);
            if (attributes.Count == 0)
                return;

            if (!KeyFactory.TryForMember(member, out MemberKey rawKey))
                return;

            MemberKey sourceKey = registry.Resolve(rawKey);
            if (registry.Find(sourceKey) == null)
            {
                foreach (CustomAttributeData attribute in attributes)
                    ReportOnType(attribute, typeKey, sink);

                return;
            }

            foreach (CustomAttributeData attribute in attributes)
            {
                ReportOnMember(attribute, sourceKey, sink);
                ReportNamedTargets(attribute, typeKey, registry, sourceKey, sink);
            }
        }

        /// <summary>
        /// Resolves method names passed to an attribute as strings. Attributes like OnValueChanged or
        /// ValidateInput invoke a member by name, which no metadata or IL reference records, so without
        /// this the target looks like nothing calls it.
        /// </summary>
        private static void ReportNamedTargets(CustomAttributeData attribute,
            TypeKey typeKey,
            MemberRegistry registry,
            MemberKey sourceKey,
            IUsageSink sink)
        {
            foreach (CustomAttributeTypedArgument argument in attribute.ConstructorArguments)
            {
                if (argument.Value is not string name || name.Length == 0)
                    continue;

                if (!registry.TryFindByName(typeKey, name, out MemberKey targetKey))
                    continue;

                sink.AddMemberUsage(sourceKey, targetKey, EUsageKind.AttributeUsage);
            }
        }

        private static void ReportOnMember(CustomAttributeData attribute, MemberKey sourceKey, IUsageSink sink)
        {
            if (KeyFactory.TryForMember(attribute.Constructor, out MemberKey constructorKey))
                sink.AddMemberUsage(sourceKey, constructorKey, EUsageKind.AttributeUsage);

            if (KeyFactory.TryForType(attribute.AttributeType, out TypeKey attributeKey))
                sink.AddTypeUsage(sourceKey, attributeKey);
        }

        private static void ReportOnType(CustomAttributeData attribute, TypeKey typeKey, IUsageSink sink)
        {
            if (KeyFactory.TryForType(attribute.AttributeType, out TypeKey attributeKey))
                sink.AddTypeRelation(typeKey, attributeKey);
        }

        private static IList<CustomAttributeData> Read(MemberInfo member)
        {
            try
            {
                return member.GetCustomAttributesData();
            }
            catch (Exception)
            {
                // An attribute whose type failed to load simply does not take part in the graph.
                return Array.Empty<CustomAttributeData>();
            }
        }
    }
}
