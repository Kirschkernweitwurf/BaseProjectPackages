using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One type in the graph, with its members and its relations to other types.</summary>
    internal sealed class TypeNodeInfo
    {
        /// <summary>Identity of the type.</summary>
        internal TypeKey Key { get; }

        /// <summary>Name without namespace, with nested types joined by a dot.</summary>
        internal string ShortName { get; }

        /// <summary>Full name including namespace.</summary>
        internal string FullName { get; }

        /// <summary>Namespace of the type, or an empty string for the global namespace.</summary>
        internal string Namespace { get; }

        /// <summary>Name of the assembly the type lives in.</summary>
        internal string AssemblyName { get; }

        /// <summary>Category the type falls into.</summary>
        internal ETypeKind Kind { get; }

        /// <summary>Declared visibility.</summary>
        internal EAccessLevel Access { get; }

        /// <summary>True for static classes.</summary>
        internal bool IsStatic { get; }

        /// <summary>True for abstract types.</summary>
        internal bool IsAbstract { get; }

        /// <summary>True when no type can derive from this one.</summary>
        internal bool IsSealed { get; set; }

        /// <summary>True when this type is an attribute, which exists to be applied to other code.</summary>
        internal bool IsAttribute { get; set; }

        /// <summary>True when this type is an editor window, which nothing outside the project opens.</summary>
        internal bool IsEditorWindow { get; set; }

        /// <summary>True when this type shares a namespace with an editor window that owns it.</summary>
        internal bool IsWindowOwned { get; set; }

        /// <summary>Members that are neither a constructor nor a property or event accessor.</summary>
        internal int BehaviourMemberCount { get; set; }

        /// <summary>True when the type derives from a Unity object.</summary>
        internal bool IsUnityObject { get; }

        /// <summary>True when the type derives from MonoBehaviour.</summary>
        internal bool IsMonoBehaviour { get; }

        /// <summary>True when something outside the code reaches this type, for example an editor attribute.</summary>
        internal bool IsEntryPoint { get; set; }

        /// <summary>Reason the type counts as an entry point, used for the tooltip.</summary>
        internal string EntryPointReason { get; set; }

        /// <summary>Asset path of the script file, or null when none could be resolved.</summary>
        internal string ScriptPath { get; set; }

        /// <summary>True when the type only exists in the editor and never ships in a build.</summary>
        internal bool IsEditorOnly { get; set; }

        /// <summary>Stable id used for dismissals, built once so lookups allocate nothing.</summary>
        internal string DismissalId { get; set; }

        /// <summary>
        /// True when the type ships in a distributable package. Its public surface exists to be called
        /// from code that is not in this project, so "nothing here uses it" is not a defect.
        /// </summary>
        internal bool IsPackageAssembly { get; set; }

        /// <summary>
        /// True when findings on this type are meaningless: generated output, a sample fixture, a test,
        /// or anything carrying a suppression attribute.
        /// </summary>
        internal bool IsExcludedFromFindings { get; set; }

        /// <summary>Reason the type is excluded, shown so an exclusion is never silent.</summary>
        internal string ExclusionReason { get; set; }

        /// <summary>Type this one is nested inside, or the default when it is top level.</summary>
        internal TypeKey DeclaringTypeKey { get; set; }

        /// <summary>Type this one derives from, or the default when it derives from nothing scanned.</summary>
        internal TypeKey BaseTypeKey { get; set; }

        /// <summary>Every member declared directly on this type.</summary>
        internal List<MemberNodeInfo> Members { get; }

        /// <summary>Types this one depends on, with how many member level usages back that up.</summary>
        internal Dictionary<TypeKey, int> Outgoing { get; }

        /// <summary>Types that depend on this one, with how many member level usages back that up.</summary>
        internal Dictionary<TypeKey, int> Incoming { get; }

        /// <summary>Number of usages that leave the scanned scope, for example into Unity itself.</summary>
        internal int ExternalReferenceCount { get; set; }

        /// <summary>Findings the analyzer reported for this type.</summary>
        internal ETypeIssue Issues { get; set; }

        /// <summary>Names of the other types in the same dependency cycle, if any.</summary>
        internal List<string> CyclePartners { get; }

        /// <summary>Identifies the cycle this type belongs to, shared by every type in the same loop.</summary>
        internal string CycleId { get; set; }

        /// <summary>The edges that close the loop, written out so the cycle can be checked by reading.</summary>
        internal string CycleDescription { get; set; }

        /// <summary>How many types are tangled together around this loop, which is often far more.</summary>
        internal int CycleComponentSize { get; set; }

        /// <summary>The edge in the loop held together by the fewest usages, offered as a hint.</summary>
        internal string CycleCutHint { get; set; }

        /// <summary>Total size of every compiled member body on this type, in bytes.</summary>
        internal int IlSize { get; set; }

        /// <summary>How many different namespaces this type reaches into.</summary>
        internal int NamespaceReach { get; set; }

        /// <summary>
        /// Share of the members that only hold data: consts, enum members and static readonly fields.
        /// A type made almost entirely of those is a lookup table however it happens to be declared.
        /// </summary>
        internal float DataMemberShare { get; set; }

        /// <summary>
        /// Share of the members that are abstract, counting an interface as wholly abstract. Something
        /// everything depends on is safer when it is abstract, because an abstraction changes rarely.
        /// </summary>
        internal float Abstractness { get; set; }

        /// <summary>Number of types that depend on this one.</summary>
        internal int FanIn => Incoming.Count;

        /// <summary>Number of types this one depends on.</summary>
        internal int FanOut => Outgoing.Count;

        /// <summary>
        /// Ratio of outgoing to total coupling. Zero means nothing depends outward and the type is
        /// stable, one means it depends on everything and nothing depends on it.
        /// </summary>
        internal float Instability => FanIn + FanOut == 0
            ? 0f
            : FanOut / (float)(FanIn + FanOut);

        /// <summary>
        /// Distance from the main sequence, where abstractness and instability sum to one. Zero is
        /// healthy: depended upon and abstract, or concrete and depending on plenty. One is a corner.
        /// </summary>
        internal float MainSequenceDistance => Mathf.Abs(Abstractness + Instability - 1f);

        /// <summary>True when something reported here was not reported by the previous scan.</summary>
        internal bool HasNewFindings { get; set; }

        /// <summary>True when the analyzer reported anything on the type itself.</summary>
        internal bool HasIssues => Issues != ETypeIssue.None;

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
        internal void AddOutgoing(TypeKey target)
        {
            Outgoing.TryGetValue(target, out int count);
            Outgoing[target] = count + 1;
        }

        /// <summary>Records that another type uses this one.</summary>
        /// <param name="source">Type that uses this one.</param>
        internal void AddIncoming(TypeKey source)
        {
            Incoming.TryGetValue(source, out int count);
            Incoming[source] = count + 1;
        }
    }
}