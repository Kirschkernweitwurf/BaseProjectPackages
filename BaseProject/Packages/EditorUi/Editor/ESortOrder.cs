namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// The direction a list window is sorted in, and the third state a sortable column header
    /// cycles back to.
    /// </summary>
    /// <remarks>
    /// <see cref="Default"/> is not an absence of sorting. It means the window falls back to
    /// whatever order it defines as its own, which is usually the one that keeps rows from moving
    /// around between two reads of live data.
    /// </remarks>
    public enum ESortOrder : byte
    {
        /// <summary>Smallest first.</summary>
        Ascending = 0,

        /// <summary>The window's own order, with no column driving it.</summary>
        Default = 1,

        /// <summary>Largest first.</summary>
        Descending = 2
    }
}