namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>How the scan results are grouped and ordered.</summary>
    public enum EAssetNamingSort : byte
    {
        /// <summary>One collapsible group per folder.</summary>
        Folder = 0,

        /// <summary>One flat list, ordered by file name.</summary>
        Name = 1,

        /// <summary>One collapsible group per rule.</summary>
        Rule = 2
    }
}