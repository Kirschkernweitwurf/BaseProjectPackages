using System;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Extensions
{
    /// <summary>
    /// A computed, read-only column of the Data Hub table, for values no single serialized field holds.
    /// Found automatically and shown on every table whose type is a <see cref="TargetType"/>.
    /// </summary>
    public interface IDataColumn
    {
        /// <summary>The type the column can read. It also shows on tables of derived types.</summary>
        Type TargetType { get; }

        /// <summary>The title of the column.</summary>
        string Header { get; }

        /// <summary>Position among the computed columns. Lower comes first.</summary>
        int Order { get; }

        /// <summary>
        /// What the column means, in a sentence a designer understands, shown on its title. Null shows only
        /// the note that the column is read only.
        /// </summary>
        string Tooltip => null;

        /// <summary>True when the column holds numbers, so they line up on the right like number fields.</summary>
        bool IsNumeric => false;

        /// <summary>
        /// The text of the cell for one asset. Numbers are sorted as numbers.
        /// </summary>
        /// <param name="asset">The asset of the row, always of <see cref="TargetType"/>.</param>
        /// <returns>The cell text.</returns>
        string GetText(Object asset);
    }
}