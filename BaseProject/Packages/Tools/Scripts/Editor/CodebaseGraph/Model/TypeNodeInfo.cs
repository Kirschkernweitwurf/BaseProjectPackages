using System.Collections.Generic;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One type in the graph, with its members and its relations to other types.</summary>
    public sealed class TypeNodeInfo
    {
        /// <summary>Identity of the type.</summary>
        public TypeKey Key { get; }

        /// <summary>Name without namespace, with nested types joined by a dot.</summary>
        public string ShortName { get; }

        /// <summary>Full name including namespace.</summary>
        public string FullName { get; }

        /// <summary>Namespace of the type, or an empty string for the global namespace.</summary>
        public string Namespace { get; }

        /// <summary>Name of the assembly the type lives in.</summary>
        public string AssemblyName { get; }

        /// <summary>Category the type falls into.</summary>
        public ETypeKind Kind { get; }

        /// <summary>Declared visibility.</summary>
        public EAccessLevel Access { get; }

        /// <summary>True for static classes.</summary>
        public bool IsStatic { get; }

        /// <summary>True for abstract types.</summary>
        public bool IsAbstract { get; }

        /// <summary>True when the type derives from a Unity object.</summary>
        public bool IsUnityObject { get; }

        /// <summary>True when the type derives from MonoBehaviour.</summary>
        public bool IsMonoBehaviour { get; }

        /// <summary>True when something outside the code reaches this type, for example an editor attribute.</summary>
        public bool IsEntryPoint { get; set; }

        /// <summary>Reason the type counts as an entry point, used for the tooltip.</summary>
        public string EntryPointReason { get; set; }

        /// <summary>Asset path of the script file, or null when none could be resolved.</summary>
        public string ScriptPath { get; set; }

        /// <summary>True when the type only exists in the editor and never ships in a build.</summary>
        public bool IsEditorOnly { get; set; }

        /// <summary>Stable id used for dismissals, built once so lookups allocate nothing.</summary>
        public string DismissalId { get; set; }

        /// <summary>
        /// True when the type ships in a distributable package. Its public surface exists to be called
        /// from code that is not in this project, so "nothing here uses it" is not a defect.
        /// </summary>
        public bool IsPackageAssembly { get; set; }

        /// <summary>
        /// True when findings on this type are meaningless: generated output, a sample fixture, a test,
        /// or anything carrying a suppression attribute.
        /// </summary>
        public bool IsExcludedFromFindings { get; set; }

        /// <summary>Reason the type is excluded, shown so an exclusion is never silent.</summary>
        public string ExclusionReason { get; set; }

        /// <summary>Type this one is nested inside, or the default when it is top level.</summary>
        public TypeKey DeclaringTypeKey { get; set; }

        /// <summary>Every member declared directly on this type.</summary>
        public List<MemberNodeInfo> Members { get; }

        /// <summary>Types this one depends on, with how many member level usages back that up.</summary>
        public Dictionary<TypeKey, int> Outgoing { get; }

        /// <summary>Types that depend on this one, with how many member level usages back that up.</summary>
        public Dictionary<TypeKey, int> Incoming { get; }

        /// <summary>Number of usages that leave the scanned scope, for example into Unity itself.</summary>
        public int ExternalReferenceCount { get; set; }

        /// <summary>Findings the analyzer reported for this type.</summary>
        public ETypeIssue Issues { get; set; }

        /// <summary>Names of the other types in the same dependency cycle, if any.</summary>
        public List<string> CyclePartners { get; }

        /// <summary>Identifies the cycle this type belongs to, shared by every type in the same loop.</summary>
        public string CycleId { get; set; }

        /// <summary>Number of types that depend on this one.</summary>
        public int FanIn => Incoming.Count;

        /// <summary>Number of types this one depends on.</summary>
        public int FanOut => Outgoing.Count;

        /// <summary>
        /// Ratio of outgoing to total coupling. Zero means nothing depends outward and the type is
        /// stable, one means it depends on everything and nothing depends on it.
        /// </summary>
        public float Instability => FanIn + FanOut == 0
            ? 0f
            : FanOut / (float)(FanIn + FanOut);

        /// <summary>True when the analyzer reported anything on the type itself.</summary>
        public bool HasIssues => Issues != ETypeIssue.None;

        /// <summary>Creates a type node without members or relations yet.</summary>
        /// <param name="key">Identity of the type.</param>
        /// <param name="shortName">Name without namespace.</param>
        /// <param name="fullName">Full name including namespace.</param>
        /// <param name="typeNamespace">Namespace of the type.</param>
        /// <param name="assemblyName">Name of the declaring assembly.</param>
        /// <param name="kind">Category the type falls into.</param>
        /// <param name="access">Declared visibility.</param>
        /// <param name="isStatic">Whether the type is a static class.</param>
        /// <param name="isAbstract">Whether the type is abstract.</param>
        /// <param name="isUnityObject">Whether the type derives from a Unity object.</param>
        /// <param name="isMonoBehaviour">Whether the type derives from MonoBehaviour.</param>
        public TypeNodeInfo(TypeKey key,
            string shortName,
            string fullName,
            string typeNamespace,
            string assemblyName,
            ETypeKind kind,
            EAccessLevel access,
            bool isStatic,
            bool isAbstract,
            bool isUnityObject,
            bool isMonoBehaviour)
        {
            Key = key;
            ShortName = shortName;
            FullName = fullName;
            Namespace = typeNamespace;
            AssemblyName = assemblyName;
            Kind = kind;
            Access = access;
            IsStatic = isStatic;
            IsAbstract = isAbstract;
            IsUnityObject = isUnityObject;
            IsMonoBehaviour = isMonoBehaviour;
            Members = new List<MemberNodeInfo>();
            Outgoing = new Dictionary<TypeKey, int>();
            Incoming = new Dictionary<TypeKey, int>();
            CyclePartners = new List<string>();
        }

        /// <summary>Adds one member level usage to the type level relation.</summary>
        /// <param name="target">Type that is being used.</param>
        public void AddOutgoing(TypeKey target)
        {
            Outgoing.TryGetValue(target, out int count);
            Outgoing[target] = count + 1;
        }

        /// <summary>Records that another type uses this one.</summary>
        /// <param name="source">Type that uses this one.</param>
        public void AddIncoming(TypeKey source)
        {
            Incoming.TryGetValue(source, out int count);
            Incoming[source] = count + 1;
        }
    }
}
