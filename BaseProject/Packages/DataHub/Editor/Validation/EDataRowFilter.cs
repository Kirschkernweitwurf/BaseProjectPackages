namespace Base.DataHubPackage.Editor.Validation
{
    /// <summary>Which rows the table shows, by what their checks found.</summary>
    internal enum EDataRowFilter : byte
    {
        /// <summary>Every row.</summary>
        All = 0,

        /// <summary>Rows with at least one problem.</summary>
        Problems = 1,

        /// <summary>Rows with at least one warning.</summary>
        Warnings = 2
    }
}