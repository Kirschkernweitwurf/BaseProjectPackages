using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;
using Base.UtilityPackage;
using UnityEditor;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// What the asset pass needs to look things up, and where its answers are written back. Holding it
    /// apart from the walking keeps the YAML reader to reading YAML.
    /// <br/><br/>
    /// Every lookup here fails towards crediting rather than away from it. A serialized field that is
    /// wrongly credited loses a little severity; one that is wrongly not credited is promoted to the top
    /// of the report as something to delete, and that is the more expensive mistake by far.
    /// </summary>
    internal sealed class AssetScanContext
    {
        private const string AnimationEventReason = "Called by an animation event";
        private const string ReferenceTypeReason = "Stored in an asset by SerializeReference";
        private const string UnityEventReason = "Called by a UnityEvent wired in the inspector";

        /// <summary>True when nothing in the graph could be answered for by an asset.</summary>
        internal bool IsEmpty => _anyField.Count == 0 && _anyMethod.Count == 0 && _byFullName.Count == 0;

        private readonly Dictionary<string, TypeNodeInfo> _byFullName = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TypeNodeInfo> _byGuid = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<MemberNodeInfo>> _anyField = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<MemberNodeInfo>> _anyMethod = new(StringComparer.Ordinal);

        private readonly Dictionary<TypeKey, Dictionary<string, List<MemberNodeInfo>>> _candidates = new();

        private CodebaseGraphData _graph;

        /// <summary>Gathers everything the assets might have something to say about.</summary>
        /// <param name="graph">Graph to read from and later annotate.</param>
        /// <returns>The prepared context.</returns>
        internal static AssetScanContext Build(CodebaseGraphData graph)
        {
            AssetScanContext context = new()
            {
                _graph = graph
            };

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                context._byFullName[type.FullName] = type;
                context.CollectCandidates(type);
            }

            return context;
        }

        /// <summary>Finds the type a script guid points at, remembering the answer.</summary>
        /// <param name="guid">Guid read out of an m_Script reference.</param>
        /// <returns>The type, or null when the script cannot be resolved.</returns>
        internal TypeNodeInfo ResolveByGuid(string guid)
        {
            if (_byGuid.TryGetValue(guid, out TypeNodeInfo cached))
                return cached;

            TypeNodeInfo resolved = ReadScriptType(guid);
            _byGuid[guid] = resolved;

            return resolved;
        }

        /// <summary>
        /// Records that one asset carries a value for a serialized field. The key is looked for on the
        /// script's own type and then up its base chain, because a component's block carries everything
        /// it inherited too. And finally by name alone when the chain has nothing, which is what a
        /// nested serializable class or an unresolvable script looks like.
        /// </summary>
        /// <param name="owner">Type the document belongs to, or null when it could not be resolved.</param>
        /// <param name="key">Serialized key read from the document.</param>
        internal void CreditField(TypeNodeInfo owner, string key)
        {
            for (TypeNodeInfo current = owner; current != null; current = ReadBaseType(current))
            {
                if (!_candidates.TryGetValue(current.Key, out Dictionary<string, List<MemberNodeInfo>> byName))
                    continue;

                if (!byName.TryGetValue(key, out List<MemberNodeInfo> members))
                    continue;

                Credit(members);
                _graph.FieldsCreditedByType++;
                return;
            }

            if (!_anyField.TryGetValue(key, out List<MemberNodeInfo> anywhere))
                return;

            Credit(anywhere);

            // Which of the two ways this fell through matters: one is a gap that could be closed, the
            // other is a script that cannot be resolved to a type at all.
            if (owner == null)
                _graph.FieldsCreditedByUnknownScript++;
            else
                _graph.FieldsCreditedByNestedType++;
        }

        /// <summary>Marks a method named by an inspector wired UnityEvent as reachable.</summary>
        /// <param name="typeName">Namespace qualified name of the target type.</param>
        /// <param name="methodName">Method the event calls.</param>
        internal void MarkEventTarget(string typeName, string methodName)
        {
            if (_byFullName.TryGetValue(typeName, out TypeNodeInfo type))
            {
                MarkNamed(type, methodName, UnityEventReason);
                return;
            }

            MarkAnywhere(methodName, UnityEventReason, false);
        }

        /// <summary>
        /// Marks a method named by an animation event as reachable. A clip records only the method name,
        /// never the type, so this can only be matched by name across the project.
        /// </summary>
        /// <param name="methodName">Method the clip calls.</param>
        internal void MarkAnimationEvent(string methodName) => MarkAnywhere(methodName, AnimationEventReason, true);

        /// <summary>Marks a type stored by SerializeReference as reachable.</summary>
        /// <param name="typeName">Namespace qualified name read from the reference entry.</param>
        internal void MarkReferenceType(string typeName)
        {
            if (!_byFullName.TryGetValue(typeName, out TypeNodeInfo type))
                return;

            type.IsEntryPoint = true;
            type.EntryPointReason = ReferenceTypeReason;
        }

        private static void Credit(List<MemberNodeInfo> members)
        {
            foreach (MemberNodeInfo member in members)
                member.AssetUsageCount++;
        }

        private static void MarkNamed(TypeNodeInfo type, string methodName, string reason)
        {
            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.Name != methodName || member.Kind != EMemberKind.Method)
                    continue;

                member.IsEntryPoint = true;
                member.EntryPointReason = reason;
            }
        }

        private static void Add(Dictionary<string, List<MemberNodeInfo>> byName,
            string key,
            MemberNodeInfo member)
        {
            if (!byName.TryGetValue(key, out List<MemberNodeInfo> members))
            {
                members = new List<MemberNodeInfo>();
                byName[key] = members;
            }

            members.Add(member);
        }

        /// <summary>
        /// Marks every method of a given name across the project. A clip names only the method, so this
        /// is the one place name matching cannot be avoided, and the signature gate is what keeps it
        /// from being the loose matching that was taken out of the field pass.
        /// </summary>
        private void MarkAnywhere(string methodName, string reason, bool requiresEventSignature)
        {
            if (!_anyMethod.TryGetValue(methodName, out List<MemberNodeInfo> members))
                return;

            foreach (MemberNodeInfo member in members)
            {
                if (requiresEventSignature && !member.IsAnimationEventSignature)
                    continue;

                member.IsEntryPoint = true;
                member.EntryPointReason = reason;
            }
        }

        private TypeNodeInfo ReadBaseType(TypeNodeInfo type) => type.BaseTypeKey.IsValid
            ? _graph.FindType(type.BaseTypeKey)
            : null;

        private TypeNodeInfo ReadScriptType(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return null;

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script == null)
                return null;

            Type type = script.GetClass();
            if (type == null)
                return null;

            return _byFullName.GetValueOrDefault(TypeNameUtility.FormatFullName(type));
        }

        private void CollectCandidates(TypeNodeInfo type)
        {
            Dictionary<string, List<MemberNodeInfo>> byName = null;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.Kind == EMemberKind.Method)
                {
                    Add(_anyMethod, member.Name, member);
                    continue;
                }

                if (member.Kind != EMemberKind.SerializedField)
                    continue;

                byName ??= new Dictionary<string, List<MemberNodeInfo>>(StringComparer.Ordinal);

                Register(byName, member.Name, member);

                // A renamed field still answers to the name the assets were written with.
                foreach (string alias in member.SerializedAliases)
                    Register(byName, alias, member);
            }

            if (byName != null)
                _candidates[type.Key] = byName;
        }

        private void Register(Dictionary<string, List<MemberNodeInfo>> byName,
            string key,
            MemberNodeInfo member)
        {
            Add(byName, key, member);
            Add(_anyField, key, member);
        }
    }
}