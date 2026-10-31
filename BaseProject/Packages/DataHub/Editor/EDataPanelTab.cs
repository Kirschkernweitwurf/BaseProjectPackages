namespace Base.DataHubPackage.Editor
{
    /// <summary>What the right panel of the Data Hub shows for the selected rows.</summary>
    internal enum EDataPanelTab : byte
    {
        /// <summary>The normal inspector.</summary>
        Inspector = 0,

        /// <summary>The asset lists of the selected asset, as counts.</summary>
        Contents = 1,

        /// <summary>Every list in the project that can hold the selection.</summary>
        UsedIn = 2,

        /// <summary>How the selected asset looks, its project previews and its checks.</summary>
        Preview = 3
    }
}