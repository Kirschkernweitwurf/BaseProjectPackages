using System.Collections.Generic;

namespace Base.DataHubPackage.Editor.Sheets
{
    /// <summary>What an import would do: every cell it changes, and every line or value it could not use.</summary>
    internal sealed class DataImportPlan
    {
        /// <summary>The cells that change.</summary>
        internal List<DataImportChange> Changes { get; } = new();

        /// <summary>Lines or values that could not be read, one sentence each.</summary>
        internal List<string> Problems { get; } = new();

        /// <summary>Columns of the sheet that are left alone, with the reason.</summary>
        internal List<string> Skipped { get; } = new();

        /// <summary>How many lines of the sheet matched an asset of the table.</summary>
        internal int MatchedRows { get; set; }
    }
}