namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One entry that was set aside during triage, in the shape the overview list draws.</summary>
    public sealed class DismissalEntry
    {
        /// <summary>The stable id the store holds.</summary>
        public string Id { get; }

        /// <summary>What the id points at.</summary>
        public EDismissalKind Kind { get; }

        /// <summary>The id with its prefix removed, which is what a reader recognizes.</summary>
        public string DisplayName { get; }

        /// <summary>True when everything inside this entry was dismissed along with it.</summary>
        public bool IncludesContents { get; }

        /// <summary>True when the entry can hold others, so restoring it can reach further.</summary>
        public bool CanHoldContents => Kind != EDismissalKind.Member;

        /// <summary>True when nothing in the current scan carries this id any more.</summary>
        public bool IsStale { get; set; }

        /// <summary>Id this one most likely became, when a single signature change explains the loss.</summary>
        public string SuggestedId { get; set; }

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
