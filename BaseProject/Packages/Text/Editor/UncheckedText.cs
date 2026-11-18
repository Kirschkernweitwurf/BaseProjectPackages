namespace Base.TextPackage.Editor
{
    /// <summary>
    /// A text box with words in it that no source checks: its text is set in code nobody told the check about or
    /// it is not localized at all.
    /// </summary>
    internal sealed class UncheckedText
    {
        /// <summary>Creates the entry.</summary>
        /// <param name="location">Where the box lives in the project.</param>
        /// <param name="text">The text the box holds in the prefab or scene.</param>
        internal UncheckedText(TextBoxLocation location, string text)
        {
            Location = location;
            Text = text;
        }

        /// <summary>Where the box lives in the project.</summary>
        internal TextBoxLocation Location { get; }

        /// <summary>The text the box holds in the prefab or scene.</summary>
        internal string Text { get; }
    }
}