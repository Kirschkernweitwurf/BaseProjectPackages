namespace Base.ToolsPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// One line of dismissal instructions and what became of it.
    /// <para>
    /// The line number and the text are carried along so the reader can find the line again. A report
    /// that says a line was ignored without saying which one leaves them re-reading the whole block.
    /// </para>
    /// </summary>
    internal readonly struct DismissalLineResult
    {
        /// <summary>Which line of the pasted text this was, counting from one.</summary>
        internal int Number { get; }

        /// <summary>The line as it was written, trimmed.</summary>
        internal string Text { get; }

        /// <summary>What happened to it.</summary>
        internal EDismissalOutcome Outcome { get; }

        /// <summary>Records the result of one line.</summary>
        /// <param name="number">Which line of the pasted text this was, counting from one.</param>
        /// <param name="text">The line as it was written.</param>
        /// <param name="outcome">What happened to it.</param>
        internal DismissalLineResult(int number, string text, EDismissalOutcome outcome)
        {
            Number = number;
            Text = text;
            Outcome = outcome;
        }

        /// <summary>Whether the line changed the stored dismissals.</summary>
        internal bool IsApplied => Outcome == EDismissalOutcome.Applied;

        /// <summary>Says in one short phrase why a line changed nothing.</summary>
        /// <returns>The reason, or an empty string when the line was applied.</returns>
        internal string Describe() => Outcome switch
        {
            EDismissalOutcome.AlreadyDismissed => "already dismissed, nothing to do",
            EDismissalOutcome.NotDismissed => "not dismissed, so there is nothing to restore",
            EDismissalOutcome.UnknownVerb => "first word is not dismiss, dismiss-tree, restore or restore-tree",
            EDismissalOutcome.UnreadableId => "id does not start with namespace:, type: or member:",
            _ => string.Empty
        };
    }
}