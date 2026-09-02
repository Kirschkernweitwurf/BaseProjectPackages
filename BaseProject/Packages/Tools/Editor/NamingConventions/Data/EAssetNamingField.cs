namespace Base.ToolsPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// One editable field of a rule. Used to remember which fields a person changed by hand, so
    /// the auto detection can refresh everything else without overwriting a deliberate decision.
    /// </summary>
    internal enum EAssetNamingField : byte
    {
        /// <summary>The display name of the rule.</summary>
        Label = 0,

        /// <summary>Whether the rule takes part in scans.</summary>
        Enabled = 1,

        /// <summary>The asset kind or type the rule applies to.</summary>
        TypeName = 2,

        /// <summary>The path fragment that narrows the rule down.</summary>
        PathFilter = 3,

        /// <summary>The casing style.</summary>
        Style = 4,

        /// <summary>The list of allowed prefixes.</summary>
        Prefixes = 5,

        /// <summary>The list of allowed suffixes.</summary>
        Suffixes = 6,

        /// <summary>Whether a suffix is demanded or only allowed.</summary>
        SuffixOptional = 7,

        /// <summary>The list of text that has to be dropped.</summary>
        Stripped = 8,

        /// <summary>The regular expression.</summary>
        Pattern = 9,

        /// <summary>The length of the number at the end.</summary>
        Digits = 10
    }
}