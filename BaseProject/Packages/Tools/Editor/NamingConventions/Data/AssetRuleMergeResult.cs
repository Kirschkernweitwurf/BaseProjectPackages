namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>What a detection run changed about the rule set.</summary>
    internal sealed class AssetRuleMergeResult
    {
        /// <summary>Rules that were added for an asset kind that had none.</summary>
        public int Added { get; set; }

        /// <summary>Rules whose untouched fields were refreshed.</summary>
        public int Updated { get; set; }

        /// <summary>Rules that were dropped because nothing needs them anymore.</summary>
        public int Removed { get; set; }

        /// <summary>True when the run would leave the rule set exactly as it is.</summary>
        public bool IsEmpty => Added == 0 && Updated == 0 && Removed == 0;

        /// <summary>One line summary for the dialog and the log.</summary>
        public override string ToString() => $"{Added} added, {Updated} updated, {Removed} removed";
    }
}