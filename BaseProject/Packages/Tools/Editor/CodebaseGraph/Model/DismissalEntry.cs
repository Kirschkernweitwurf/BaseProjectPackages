namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One entry that was dismissed during triage, in the shape the overview list draws.</summary>
    internal sealed class DismissalEntry
    {
        /// <summary>The stable id the store holds.</summary>
        internal string Id { get; }

        /// <summary>What the id points at.</summary>
        internal EDismissalKind Kind { get; }

        /// <summary>The id with its prefix removed, which is what a reader recognizes.</summary>
        internal string DisplayName { get; }

        /// <summary>Finding this entry silences, or none when it silences everything on the entry.</summary>
        internal EFinding Finding { get; set; }

        /// <summary>True when everything inside this entry was dismissed along with it.</summary>
        internal bool IncludesContents { get; }

        /// <summary>True when the entry can hold others, so restoring it can reach further.</summary>
        internal bool CanHoldContents => Kind != EDismissalKind.Member;

        /// <summary>Why this entry stopped matching, or none when it still does.</summary>
        internal EStaleReason StaleReason { get; set; }

        /// <summary>True when nothing in the current scan carries this id anymore.</summary>
        internal bool IsStale => StaleReason != EStaleReason.None;

        /// <summary>ID this one most likely became, when a single signature change explains the loss.</summary>
        internal string SuggestedId { get; set; }

        /// <summary>Creates an overview entry.</summary>
        /// <param name="id">The stable id the store holds.</param>
        /// <param name="kind">What the id points at.</param>
        /// <param name="displayName">The id with its prefix removed.</param>
        /// <param name="includesContents">Whether the contents were dismissed along with it.</param>
        public DismissalEntry(string id, EDismissalKind kind, string displayName, bool includesContents)
        {
            Id = id;
            Kind = kind;
            DisplayName = displayName;
            IncludesContents = includesContents;
        }
    }
}