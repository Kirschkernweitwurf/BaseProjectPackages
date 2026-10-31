using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Table;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Compare
{
    /// <summary>
    /// The selected rows of a Data Hub table side by side: one column per asset, one line per field, and the
    /// lines where they differ lit, so two similar cards can be told apart at a glance. It stays live while the
    /// designer edits either of them.
    /// </summary>
    internal sealed class DataCompareWindow : EditorWindow
    {
        private const float DiffAlpha = 0.16f;
        private const string DifferencesFormat = "{0} of {1} fields differ";
        private const string EyebrowText = "Compare";
        private const float FieldWidth = 180f;
        private const float HeaderHeight = 64f;
        private const float MinValueWidth = 140f;
        private const string OnlyDifferencesLabel = "Only differences";
        private const string OnlyDifferencesTooltip = "Hide the fields every selected asset has the same value in.";
        private const string PingTooltip = "Click to find it in the Project window.";
        private const float ToolbarHeight = 30f;
        private const string WindowTitleFormat = "Compare {0}";

        private readonly List<DataColumn> _columns = new();
        private readonly List<DataRow> _rows = new();

        private bool _isOnlyDifferences;
        private Vector2 _scroll;
        private DataHubStyles _styles;
        private string _tableName;

        /// <summary>Opens a compare window for the given assets with the given columns.</summary>
        /// <param name="tableName">The name of the table, for the title.</param>
        /// <param name="assets">The assets to put side by side, two or more.</param>
        /// <param name="columns">The columns of the table, in their order. The name and the check are left out.</param>
        internal static void Show(string tableName, IReadOnlyList<Object> assets, IReadOnlyList<DataColumn> columns)
        {
            DataCompareWindow window = CreateInstance<DataCompareWindow>();
            window.titleContent = new GUIContent(string.Format(WindowTitleFormat, tableName));
            window._tableName = tableName;

            foreach (Object asset in assets)
            {
                if (asset != null)
                    window._rows.Add(new DataRow(asset));
            }

            foreach (DataColumn column in columns)
            {
                if (column.Kind != EDataColumnKind.Name
                    && column.Kind != EDataColumnKind.Check)
                    window._columns.Add(column);
            }

            window.Show();
        }

#region Unity Callbacks
        private void OnInspectorUpdate() => Repaint();

        private void OnGUI()
        {
            // The columns and rows are not serialized, so a domain reload leaves nothing to compare.
            if (_rows.Count == 0)
            {
                Close();
                return;
            }

            _styles ??= new DataHubStyles();
            _styles.EnsureBuilt();

            foreach (DataRow row in _rows)
            {
                if (row.Asset != null)
                    row.Serialized.UpdateIfRequiredOrScript();
            }

            Rect area = new(0f, 0f, position.width, position.height);
            DataHubGui.Fill(area, EditorPalette.Background);

            int differences = CountDifferences();

            DataHubGui.PaneTitle(new Rect(0f, 0f, position.width, HeaderHeight), EyebrowText, _tableName,
                string.Format(DifferencesFormat, differences, _columns.Count), _styles);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect toggle = new(DataHubStyles.PanelPadding, HeaderHeight + (ToolbarHeight - lineHeight) * 0.5f,
                FieldWidth, lineHeight);

            _isOnlyDifferences = GUI.Toggle(toggle, _isOnlyDifferences,
                new GUIContent(OnlyDifferencesLabel, OnlyDifferencesTooltip));

            float top = HeaderHeight + ToolbarHeight;
            DrawGrid(new Rect(0f, top, position.width, Mathf.Max(0f, position.height - top)));
        }

        private void OnDestroy()
        {
            foreach (DataRow row in _rows)
                row.Dispose();

            _rows.Clear();
            _styles?.Dispose();
        }
#endregion

        private bool IsDifferent(DataColumn column)
        {
            string first = _rows[0].CellText(column);

            for (int i = 1; i < _rows.Count; i++)
            {
                if (!string.Equals(first, _rows[i].CellText(column), StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private int CountDifferences()
        {
            int count = 0;

            foreach (DataColumn column in _columns)
            {
                if (IsDifferent(column))
                    count++;
            }

            return count;
        }

        private void DrawGrid(Rect area)
        {
            float rowHeight = EditorTableStyles.RowHeight;
            float valueWidth = Mathf.Max(MinValueWidth, (area.width - FieldWidth) / _rows.Count);
            float contentWidth = FieldWidth + valueWidth * _rows.Count;
            float contentHeight = rowHeight * (_columns.Count + 1);

            _scroll = GUI.BeginScrollView(area, _scroll, new Rect(0f, 0f, contentWidth, contentHeight));

            DataHubGui.Fill(new Rect(0f, 0f, contentWidth, rowHeight), EditorTableStyles.HeaderColor);

            for (int i = 0; i < _rows.Count; i++)
            {
                Rect cell = new(FieldWidth + i * valueWidth + DataHubStyles.CellPadding, 0f,
                    valueWidth - DataHubStyles.CellPadding * 2f, rowHeight);

                if (_rows[i].Asset == null)
                    continue;

                if (GUI.Button(cell, new GUIContent(_rows[i].Asset.name, PingTooltip), _styles.LinkName))
                    EditorGUIUtility.PingObject(_rows[i].Asset);
            }

            int line = 1;

            foreach (DataColumn column in _columns)
            {
                bool isDifferent = IsDifferent(column);

                if (_isOnlyDifferences
                    && !isDifferent)
                    continue;

                Rect row = new(0f, line * rowHeight, contentWidth, rowHeight);
                EditorRows.DrawRowBackground(row, line);

                if (isDifferent)
                    DataHubGui.Fill(row, EditorPalette.WithAlpha(EditorPalette.Accent, DiffAlpha));

                GUI.Label(new Rect(DataHubStyles.PanelPadding, row.y, FieldWidth - DataHubStyles.PanelPadding,
                    rowHeight), new GUIContent(column.FullTitle, column.Tooltip), _styles.Detail);

                for (int i = 0; i < _rows.Count; i++)
                {
                    string text = _rows[i].CellText(column);
                    Rect cell = new(FieldWidth + i * valueWidth + DataHubStyles.CellPadding, row.y,
                        valueWidth - DataHubStyles.CellPadding * 2f, rowHeight);

                    GUIStyle style = isDifferent
                        ? _styles.NameBold
                        : _styles.Name;

                    GUI.Label(cell, new GUIContent(text, text), style);
                }

                line++;
            }

            GUI.EndScrollView();
        }
    }
}