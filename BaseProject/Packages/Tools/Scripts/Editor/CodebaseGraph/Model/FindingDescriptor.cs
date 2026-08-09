namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// Everything the window needs to say about one kind of finding: what to call it, what it actually
    /// means, what to do about it, and whether the window can do that for you.
    /// </summary>
    public sealed class FindingDescriptor
    {
        /// <summary>Wording used in the findings dropdown, phrased as a group of things.</summary>
        public string FilterLabel { get; }

        /// <summary>Short wording used on a node badge and as the detail heading.</summary>
        public string Title { get; }

        /// <summary>What the scan actually observed, in plain words.</summary>
        public string Explanation { get; }

        /// <summary>What to do about it.</summary>
        public string Action { get; }

        /// <summary>True when the window can apply the change itself.</summary>
        public bool CanQuickFix { get; }

        /// <summary>Creates a descriptor.</summary>
        /// <param name="filterLabel">Wording used in the findings dropdown.</param>
        /// <param name="title">Short wording used on a badge.</param>
        /// <param name="explanation">What the scan observed.</param>
        /// <param name="action">What to do about it.</param>
        /// <param name="canQuickFix">Whether the window can apply the change itself.</param>
        public FindingDescriptor(string filterLabel,
            string title,
            string explanation,
            string action,
            bool canQuickFix)
        {
            FilterLabel = filterLabel;
            Title = title;
            Explanation = explanation;
            Action = action;
            CanQuickFix = canQuickFix;
        }
    }
}
