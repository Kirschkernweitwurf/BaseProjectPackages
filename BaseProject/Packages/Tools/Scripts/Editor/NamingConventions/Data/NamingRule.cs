using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// The name check of a single rule: casing, allowed prefixes and suffixes, text that should be
    /// dropped, an ignore list and an optional regular expression. Pure data, so the same rule can
    /// be evaluated, edited in the rule table and extended by the convention detector without
    /// extra state.
    /// </summary>
    [Serializable]
    public sealed class NamingRule
    {
        [Tooltip("Casing the name has to follow once prefix, suffix and number are stripped.")]
        [field: SerializeField] public ENamingStyle Style { get; set; } = ENamingStyle.PascalCase;

        [Tooltip("The name has to start with one of these. The first one is used when a fix is suggested.")]
        [field: SerializeField] public List<string> Prefixes { get; private set; } = new();

        [Tooltip("The name has to end with one of these. The first one is used when a fix is suggested.")]
        [field: SerializeField] public List<string> Suffixes { get; private set; } = new();

        [Tooltip("If true a suffix is allowed but not demanded, for assets where only some kinds carry one.")]
        [field: SerializeField] public bool SuffixOptional { get; set; } = true;

        [Tooltip("Text that has to go, dropped from the front or the back before the name is checked.")]
        [field: SerializeField] public List<string> Stripped { get; private set; } = new();

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
