using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// The name check of a single rule: casing, allowed prefixes and suffixes, an ignore list and
    /// an optional regular expression. Pure data, so the same rule can be evaluated, edited in the
    /// rule table and rewritten by the convention detector without extra state.
    /// </summary>
    [Serializable]
    public sealed class NamingRule
    {
        [Tooltip("Casing the name has to follow once prefix and suffix are stripped.")]
        [field: SerializeField] public ENamingStyle Style { get; set; } = ENamingStyle.PascalCase;

        [Tooltip("The name has to start with one of these. The first entry is used for suggestions.")]
        [field: SerializeField] public List<string> Prefixes { get; private set; } = new();

        [Tooltip("The name has to end with one of these. The first entry is used for suggestions.")]
        [field: SerializeField] public List<string> Suffixes { get; private set; } = new();

        [Tooltip("Names this rule skips. Supports * as a wildcard, for example Temp*.")]
        [field: SerializeField] public List<string> IgnoredNames { get; private set; } = new();

        [Tooltip("Optional regular expression. When set it replaces the casing, prefix and suffix checks.")]
        [field: SerializeField] public string Pattern { get; set; } = string.Empty;

        /// <summary>Prefix used when a fix is suggested, or an empty string when none is required.</summary>
        public string PrimaryPrefix => Prefixes.Count > 0
            ? Prefixes[0]
            : string.Empty;

        /// <summary>Suffix used when a fix is suggested, or an empty string when none is required.</summary>
        public string PrimarySuffix => Suffixes.Count > 0
            ? Suffixes[0]
            : string.Empty;

        /// <summary>Creates an empty rule. Needed by the serializer.</summary>
        public NamingRule() { }
    }
}