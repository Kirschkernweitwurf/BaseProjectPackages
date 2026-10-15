using Base.DataHubPackage.Editor.Table;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Sheets
{
    /// <summary>One cell an import would change, with the value before and after for the preview.</summary>
    internal sealed class DataImportChange
    {
        /// <summary>Creates the change.</summary>
        /// <param name="asset">The asset of the row.</param>
        /// <param name="column">The column of the cell.</param>
        /// <param name="before">The value now, as text.</param>
        /// <param name="after">The value from the sheet, as text.</param>
        /// <param name="value">The value from the sheet, read into the field's type.</param>
        internal DataImportChange(Object asset, DataColumn column, string before, string after, object value)
        {
            Asset = asset;
            Column = column;
            Before = before;
            After = after;
            Value = value;
        }

        /// <summary>The asset of the row.</summary>
        internal Object Asset { get; }

        /// <summary>The column of the cell.</summary>
        internal DataColumn Column { get; }

        /// <summary>The value now, as text.</summary>
        internal string Before { get; }

        /// <summary>The value from the sheet, as text.</summary>
        internal string After { get; }

        /// <summary>The value from the sheet, read into the field's type.</summary>
        internal object Value { get; }
    }
}