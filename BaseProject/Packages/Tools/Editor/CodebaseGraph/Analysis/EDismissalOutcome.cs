namespace Base.ToolsPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// What happened to one line of dismissal instructions.
    /// <para>
    /// A line that changed nothing is not the same as a line that could not be read, and telling the
    /// two apart is the difference between "there is nothing to do" and "the id you wrote is wrong".
    /// Reporting a single count of ignored lines leaves the reader unable to tell which they have.
    /// </para>
    /// </summary>
    internal enum EDismissalOutcome : byte
    {
        /// <summary>The line changed the stored dismissals.</summary>
        Applied = 0,

        /// <summary>The entry was already dismissed exactly this way, so nothing needed doing.</summary>
        AlreadyDismissed = 1,

        /// <summary>The line asked to restore something that is not dismissed.</summary>
        NotDismissed = 2,

        /// <summary>The first word is not one of the four verbs.</summary>
        UnknownVerb = 3,

        /// <summary>
        /// The rest of the line does not read as an id. Ids start with `namespace:`, `type:` or
        /// `member:`, which is the most common thing to get wrong when writing one by hand.
        /// </summary>
        UnreadableId = 4
    }
}