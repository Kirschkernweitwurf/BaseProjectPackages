using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// What the asset pass needs to look things up, and where its answers are written back. Holding it
    /// apart from the walking keeps the YAML reader to reading YAML.
    /// </summary>
    public sealed class AssetScanContext
    {
        private const string ReferenceTypeReason = "Stored in an asset by SerializeReference";
        private const string UnityEventReason = "Called by a UnityEvent wired in the inspector";

        private readonly Dictionary<string, TypeNodeInfo> _byFullName = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TypeNodeInfo> _byGuid = new(StringComparer.Ordinal);

        private readonly Dictionary<TypeKey, Dictionary<string, List<MemberNodeInfo>>> _candidates = new();

        /// <summary>True when nothing was reported that an asset could answer for.</summary>
        public bool IsEmpty => _candidates.Count == 0 && _byFullName.Count == 0;

        /// <summary>Gathers the members worth asking the assets about.</summary>
        /// <param name="graph">Graph to read from and later annotate.</param>
        /// <returns>The prepared context.</returns>
        public static AssetScanContext Build(CodebaseGraphData graph)
        {
            AssetScanContext context = new();

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                context._byFullName[type.FullName] = type;
                context.CollectCandidates(type);
            }

            return context;
        }

        /// <summary>Finds the type a script guid points at, remembering the answer.</summary>
        /// <param name="guid">Guid read out of an m_Script reference.</param>
        /// <returns>The type, or null when it is outside the scan.</returns>
        public TypeNodeInfo ResolveByGuid(string guid)
        {
            if (_byGuid.TryGetValue(guid, out TypeNodeInfo cached))
                return cached;

            TypeNodeInfo resolved = ReadScriptType(guid);
            _byGuid[guid] = resolved;

            return resolved;
        }

        /// <summary>Records that one asset carries a value for a field of this type.</summary>
        /// <param name="owner">Type the document belongs to.</param>
        /// <param name="key">Serialized key read from the document.</param>
        public void CreditField(TypeNodeInfo owner, string key)
        {
            if (!_candidates.TryGetValue(owner.Key, out Dictionary<string, List<MemberNodeInfo>> byName))
                return;

            if (!byName.TryGetValue(key, out List<MemberNodeInfo> members))
                return;

            foreach (MemberNodeInfo member in members)
                member.AssetUsageCount++;
        }

        /// <summary>Marks a method named by an inspector wired UnityEvent as reachable.</summary>
        /// <param name="typeName">Namespace qualified name of the target type.</param>
        /// <param name="methodName">Method the event calls.</param>
        public void MarkEventTarget(string typeName, string methodName)
        {
            if (!_byFullName.TryGetValue(typeName, out TypeNodeInfo type))
                return;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.Name != methodName)
                    continue;

                member.IsEntryPoint = true;
                member.EntryPointReason = UnityEventReason;
            }
        }

        /// <summary>Marks a type stored by SerializeReference as reachable.</summary>
        /// <param name="typeName">Namespace qualified name read from the reference entry.</param>
        public void MarkReferenceType(string typeName)
        {
            if (!_byFullName.TryGetValue(typeName, out TypeNodeInfo type))
                return;

            type.IsEntryPoint = true;
            type.EntryPointReason = ReferenceTypeReason;
        }

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

            return _byFullName.TryGetValue(TypeNameFormatter.FormatFullName(type), out TypeNodeInfo node)
                ? node
                : null;
        }

        private void CollectCandidates(TypeNodeInfo type)
        {
            Dictionary<string, List<MemberNodeInfo>> byName = null;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (!member.Issues.HasFlag(EMemberIssue.SerializedNeverRead))
                    continue;

                byName ??= new Dictionary<string, List<MemberNodeInfo>>(StringComparer.Ordinal);

                Add(byName, member.Name, member);

                // A renamed field still answers to the name the assets were written with.
                foreach (string alias in member.SerializedAliases)
                    Add(byName, alias, member);
            }

            if (byName != null)
                _candidates[type.Key] = byName;
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
    }
}
