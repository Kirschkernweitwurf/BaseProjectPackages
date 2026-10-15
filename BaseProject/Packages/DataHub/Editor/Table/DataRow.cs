using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>One asset in the table, with the serialized object every cell of the row reads and writes.</summary>
    internal sealed class DataRow : IDisposable
    {
        private const string ColumnFailedFormat = "({0})";
        private const string ProblemSeparator = "\n";

        /// <summary>Creates the row for an asset.</summary>
        /// <param name="asset">The asset of the row.</param>
        internal DataRow(Object asset)
        {
            Asset = asset;
            Serialized = new SerializedObject(asset);
        }

        /// <summary>The asset of the row.</summary>
        internal Object Asset { get; }

        /// <summary>The serialized object every cell of the row goes through.</summary>
        internal SerializedObject Serialized { get; }

        /// <summary>What is wrong with the asset, one line per problem.</summary>
        internal List<string> Problems { get; } = new();

        /// <summary>True when at least one check failed.</summary>
        internal bool HasProblems => Problems.Count > 0;

        /// <summary>Every problem on its own line, for the tooltip of the badge.</summary>
        internal string ProblemText => string.Join(ProblemSeparator, Problems);

        /// <summary>The text of one cell, for search and sorting.</summary>
        /// <param name="column">The column of the cell.</param>
        /// <returns>The text.</returns>
        internal string CellText(DataColumn column) => column.Kind switch
        {
            EDataColumnKind.Name => Asset.name,
            EDataColumnKind.Check => Problems.Count.ToString(),
            EDataColumnKind.Field => DataCell.Text(Serialized.FindProperty(column.PropertyPath)),
            EDataColumnKind.Computed => ComputedText(column),
            _ => string.Empty
        };

        /// <summary>Releases the serialized object.</summary>
        public void Dispose() => Serialized.Dispose();

        // A computed column comes from project code, and one that throws must not break every repaint
        // of the table. The message lands in the cell, where whoever wrote the column sees it.
        private string ComputedText(DataColumn column)
        {
            try
            {
                return column.Computed.GetText(Asset);
            }
            catch (Exception exception)
            {
                return string.Format(ColumnFailedFormat, exception.Message);
            }
        }
    }
}