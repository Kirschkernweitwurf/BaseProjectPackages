namespace Base.DataHubPackage.Editor.BulkEdit
{
    /// <summary>How a bulk edit changes the numbers of the chosen rows.</summary>
    internal enum EDataBulkMode : byte
    {
        /// <summary>Adds the value, negative to subtract.</summary>
        Add = 0,

        /// <summary>Multiplies by the value.</summary>
        Multiply = 1,

        /// <summary>Sets every row to the value.</summary>
        Set = 2
    }
}