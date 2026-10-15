using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Table;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Sheets
{
    /// <summary>
    /// A table as spreadsheet rows and back: the name first, then one column per field, titled as in the
    /// hub. Rows of a sheet are matched to assets by name, columns by title, so the sheet can be sorted,
    /// shortened or rearranged before it comes back.
    /// </summary>
    internal static class DataSheetTable
    {
        private const string AmbiguousRowFormat = "Line {0}: more than one asset is called '{1}'";
        private const string ChangeUndoFormat = "Import into {0}";
        private const string NoHeaderText = "Nothing to read. Copy the table with its title row.";
        private const string NoNameColumnFormat = "The title row needs a column called '{0}' to match the rows.";
        private const string ReadOnlyFormat = "'{0}' is read only here";
        private const string UnknownColumnFormat = "'{0}' is no column of this table";
        private const string UnknownRowFormat = "Line {0}: no asset of this table is called '{1}'";
        private const string ValueFormat = "Line {0}, {1}: {2}";

        /// <summary>The rows to write: a title row, then one line per table row.</summary>
        /// <param name="columns">The columns of the table.</param>
        /// <param name="rows">The rows to export, in their order.</param>
        /// <returns>The sheet rows.</returns>
        internal static List<IReadOnlyList<string>> Export(IReadOnlyList<DataColumn> columns,
            IReadOnlyList<DataRow> rows)
        {
            List<DataColumn> exported = new List<DataColumn>(columns).FindAll(
                column => column.Kind != EDataColumnKind.Check);
            List<IReadOnlyList<string>> sheet = new() { exported.ConvertAll(column => column.FullTitle) };

            foreach (DataRow row in rows)
            {
                if (row.Asset == null)
                    continue;

                row.Serialized.UpdateIfRequiredOrScript();
                sheet.Add(exported.ConvertAll(column => ExportCell(row, column)));
            }

            return sheet;
        }

        /// <summary>Works out what importing a sheet would change, without changing anything.</summary>
        /// <param name="columns">The columns of the table.</param>
        /// <param name="rows">Every row of the table.</param>
        /// <param name="sheet">The sheet rows, the first one the titles.</param>
        /// <returns>The plan, for the preview and for <see cref="Apply"/>.</returns>
        internal static DataImportPlan Plan(IReadOnlyList<DataColumn> columns, IReadOnlyList<DataRow> rows,
            List<List<string>> sheet)
        {
            DataImportPlan plan = new();

            if (sheet.Count == 0)
            {
                plan.Problems.Add(NoHeaderText);
                return plan;
            }

            DataColumn[] mapped = MapColumns(columns, sheet[0], plan);
            int nameIndex = Array.FindIndex(mapped, column => column?.Kind == EDataColumnKind.Name);

            if (nameIndex < 0)
            {
                plan.Problems.Add(string.Format(NoNameColumnFormat, columns[0].Header));
                return plan;
            }

            Dictionary<string, DataRow> byName = RowsByName(rows, out HashSet<string> ambiguous);
            Dictionary<string, Object> references = new();

            for (int line = 1; line < sheet.Count; line++)
            {
                List<string> cells = sheet[line];
                string name = Cell(cells, nameIndex).Trim();

                // Spreadsheet line numbers start at one and count the title row.
                int number = line + 1;

                if (ambiguous.Contains(name))
                {
                    plan.Problems.Add(string.Format(AmbiguousRowFormat, number, name));
                    continue;
                }

                if (!byName.TryGetValue(name, out DataRow row))
                {
                    plan.Problems.Add(string.Format(UnknownRowFormat, number, name));
                    continue;
                }

                plan.MatchedRows++;
                PlanRow(row, mapped, cells, number, references, plan);
            }

            return plan;
        }

        /// <summary>Applies every change of a plan as one undo step.</summary>
        /// <param name="plan">The plan of <see cref="Plan"/>.</param>
        /// <param name="tableName">The table name, for the undo step.</param>
        internal static void Apply(DataImportPlan plan, string tableName)
        {
            int group = Undo.GetCurrentGroup();
            Dictionary<Object, SerializedObject> byAsset = new();

            foreach (DataImportChange change in plan.Changes)
            {
                if (change.Asset == null)
                    continue;

                if (!byAsset.TryGetValue(change.Asset, out SerializedObject serialized))
                {
                    serialized = new SerializedObject(change.Asset);
                    byAsset.Add(change.Asset, serialized);
                }

                SerializedProperty property = serialized.FindProperty(change.Column.PropertyPath);

                if (property != null)
                    DataSheetValue.Write(property, change.Value);
            }

            foreach (SerializedObject serialized in byAsset.Values)
            {
                serialized.ApplyModifiedProperties();
                serialized.Dispose();
            }

            Undo.SetCurrentGroupName(string.Format(ChangeUndoFormat, tableName));
            Undo.CollapseUndoOperations(group);
        }

        private static string ExportCell(DataRow row, DataColumn column) => column.Kind == EDataColumnKind.Field
            ? DataSheetValue.Export(row.Serialized.FindProperty(column.PropertyPath))
            : row.CellText(column);

        private static string Cell(List<string> cells, int index) => index < cells.Count
            ? cells[index]
            : string.Empty;

        // Titles are matched loosely, so a sheet that changed their case still comes back.
        private static DataColumn[] MapColumns(IReadOnlyList<DataColumn> columns, List<string> titles,
            DataImportPlan plan)
        {
            DataColumn[] mapped = new DataColumn[titles.Count];

            for (int i = 0; i < titles.Count; i++)
            {
                string title = titles[i].Trim();

                if (title.Length == 0)
                    continue;

                DataColumn column = FindColumn(columns, title);

                if (column == null)
                {
                    plan.Skipped.Add(string.Format(UnknownColumnFormat, title));
                    continue;
                }

                if (column.Kind != EDataColumnKind.Name
                    && !IsImportable(column))
                {
                    plan.Skipped.Add(string.Format(ReadOnlyFormat, title));
                    continue;
                }

                mapped[i] = column;
            }

            return mapped;
        }

        private static DataColumn FindColumn(IReadOnlyList<DataColumn> columns, string title)
        {
            foreach (DataColumn column in columns)
            {
                if (string.Equals(column.FullTitle, title, StringComparison.OrdinalIgnoreCase))
                    return column;
            }

            return null;
        }

        private static bool IsImportable(DataColumn column) => column.Kind == EDataColumnKind.Field
            && column.IsEditable;

        private static Dictionary<string, DataRow> RowsByName(IReadOnlyList<DataRow> rows,
            out HashSet<string> ambiguous)
        {
            Dictionary<string, DataRow> byName = new(StringComparer.OrdinalIgnoreCase);
            ambiguous = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in rows)
            {
                if (row.Asset == null)
                    continue;

                if (!byName.TryAdd(row.Asset.name, row))
                    ambiguous.Add(row.Asset.name);
            }

            return byName;
        }

        private static void PlanRow(DataRow row, DataColumn[] mapped, List<string> cells, int number,
            Dictionary<string, Object> references, DataImportPlan plan)
        {
            row.Serialized.UpdateIfRequiredOrScript();

            for (int i = 0; i < mapped.Length; i++)
            {
                DataColumn column = mapped[i];

                // A short line leaves the cells it does not have alone, the same as an empty spreadsheet cell
                // would not be a reason to clear a value.
                if (column == null
                    || column.Kind == EDataColumnKind.Name
                    || i >= cells.Count)
                    continue;

                SerializedProperty property = row.Serialized.FindProperty(column.PropertyPath);

                if (property == null)
                    continue;

                if (!DataSheetValue.TryRead(column, property, cells[i], references, out object value,
                        out string error))
                {
                    plan.Problems.Add(string.Format(ValueFormat, number, column.FullTitle, error));
                    continue;
                }

                if (DataSheetValue.IsSame(property, DataSheetValue.Current(property), value))
                    continue;

                plan.Changes.Add(new DataImportChange(row.Asset, column, DataSheetValue.Export(property),
                    cells[i].Trim(), value));
            }
        }
    }
}