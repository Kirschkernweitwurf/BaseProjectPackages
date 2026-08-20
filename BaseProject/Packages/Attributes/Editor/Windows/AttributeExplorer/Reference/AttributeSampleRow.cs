namespace Base.AttributePackage.Editor.Drawers.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// One row of the list as it is currently drawn: either a category header or an attribute under one.
    /// </summary>
    /// <remarks>
    /// The keyboard walks this rather than the entries, so the arrows step over exactly what is on
    /// screen and a header can be reached and collapsed like any other row.
    /// </remarks>
    internal readonly struct AttributeSampleRow
    {
        /// <summary>The name of the category this row belongs to.</summary>
        internal string Category { get; }

        /// <summary>The attribute on this row, or the default when the row is a header.</summary>
        internal AttributeSampleEntry Entry { get; }

        /// <summary>True when the row is a category header rather than an attribute.</summary>
        internal bool IsHeader { get; }

        /// <summary>Creates a header row.</summary>
        /// <param name="category">The name of the category the header names.</param>
        internal AttributeSampleRow(string category)
        {
            Category = category;
            Entry = default(AttributeSampleEntry);
            IsHeader = true;
        }

        /// <summary>Creates an attribute row.</summary>
        /// <param name="entry">The attribute on the row.</param>
        internal AttributeSampleRow(in AttributeSampleEntry entry)
        {
            Category = entry.CategoryName;
            Entry = entry;
            IsHeader = false;
        }
    }
}