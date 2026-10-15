using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Sheets
{
    /// <summary>
    /// Shows what an import from a sheet would change before anything is written: every changed cell as
    /// before and after, grouped by asset, and everything that could not be used. Nothing changes until Apply.
    /// </summary>
    internal sealed class DataImportWindow : EditorWindow
    {
        private const string ApplyFormat = "Apply {0}";
        private const string CancelLabel = "Cancel";
        private const float CellLabelWidth = 160f;
        private const string ChangeCountFormat = "{0} changes";
        private const string ChangesFormat = "{0} changes in {1} of {2} matched rows";
        private const float MinHeight = 360f;
        private const float MinWidth = 520f;
        private const string NoChangesText = "Nothing to change. The sheet matches the table.";
        private const string OneChangeFormat = "1 change in {0} of {1} matched rows";
        private const string OneChangeLabel = "1 change";
        private const float Padding = 12f;
        private const string ProblemsTitle = "Could not use";
        private const string ResultFormat = "{0}  →  {1}";
        private const string SkippedTitle = "Left alone";
        private const string TitleFormat = "Import into {0}";
        private const string WindowTitle = "Import Preview";

        private Action _onApplied;
        private DataImportPlan _plan;
        private Vector2 _scroll;
        private string _source;
        private DataHubStyles _styles;
        private string _tableName;

        /// <summary>Opens the preview for a plan.</summary>
        /// <param name="plan">What the import would change.</param>
        /// <param name="tableName">The table imported into, for the title and the undo step.</param>
        /// <param name="source">Where the sheet came from, like "the clipboard" or a file name.</param>
        /// <param name="onApplied">Called after the changes were written.</param>
        internal static void Show(DataImportPlan plan, string tableName, string source, Action onApplied)
        {
            DataImportWindow window = CreateInstance<DataImportWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(MinWidth, MinHeight);
            window._plan = plan;
            window._tableName = tableName;
            window._source = source;
            window._onApplied = onApplied;
            window.ShowUtility();
        }

#region Unity Callbacks
        private void OnGUI()
        {
            // A domain reload drops the plan, and with it everything this window had to show.
            if (_plan == null)
            {
                Close();
                return;
            }

            _styles ??= new DataHubStyles();
            _styles.EnsureBuilt();

            if (HandleKeys())
                return;

            // One margin for everything, so the header, the changes and the notes start at the same edge.
            GUILayout.BeginArea(new Rect(Padding, 0f, position.width - Padding * 2f, position.height));

            EditorWindowChrome.DrawHeader(_styles, string.Format(TitleFormat, _tableName), Summary());

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawChanges();
            DrawNotes(ProblemsTitle, _plan.Problems);
            DrawNotes(SkippedTitle, _plan.Skipped);
            EditorGUILayout.EndScrollView();

            DrawButtons();

            GUILayout.EndArea();
        }

        private void OnDisable() => _styles?.Dispose();
#endregion

        private string Summary()
        {
            int assets = CountAssets();

            string changes = _plan.Changes.Count switch
            {
                0 => NoChangesText,
                1 => string.Format(OneChangeFormat, assets, _plan.MatchedRows),
                _ => string.Format(ChangesFormat, _plan.Changes.Count, assets, _plan.MatchedRows)
            };

            return _source + ". " + changes;
        }

        private int CountAssets()
        {
            HashSet<Object> assets = new();

            foreach (DataImportChange change in _plan.Changes)
                assets.Add(change.Asset);

            return assets.Count;
        }

        // Grouped under the asset name, in the order of the sheet, so it reads like the rows that changed.
        private void DrawChanges()
        {
            Object current = null;

            foreach (DataImportChange change in _plan.Changes)
            {
                if (change.Asset != current)
                {
                    current = change.Asset;
                    GUILayout.Space(EditorMetrics.TightGap);

                    // Placed by hand, because a style that cuts long text with an ellipsis measures as nothing
                    // in an automatic layout and would leave the asset name out.
                    Rect heading = EditorGUILayout.GetControlRect();

                    GUI.Label(heading, current == null
                        ? string.Empty
                        : current.name, _styles.NameBold);
                }

                Rect line = EditorGUILayout.GetControlRect();
                Rect label = new(line.x + EditorMetrics.Indent, line.y, CellLabelWidth, line.height);
                Rect result = new(label.xMax, line.y, line.xMax - label.xMax, line.height);

                GUI.Label(label, change.Column.FullTitle, _styles.Detail);
                GUI.Label(result, string.Format(ResultFormat, change.Before, change.After), _styles.Name);
            }
        }

        private void DrawNotes(string title, List<string> notes)
        {
            if (notes.Count == 0)
                return;

            GUILayout.Space(EditorMetrics.SectionGap);
            GUILayout.Label(title, _styles.SectionHeader);

            foreach (string note in notes)
                GUILayout.Label(note, _styles.Description);
        }

        private void DrawButtons()
        {
            EditorWindowChrome.DrawSeparator();
            GUILayout.Space(EditorMetrics.ItemGap);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(CancelLabel, _styles.HeaderButton))
                Close();

            using (new EditorGUI.DisabledScope(_plan.Changes.Count == 0))
            {
                string label = string.Format(ApplyFormat, _plan.Changes.Count == 1
                    ? OneChangeLabel
                    : string.Format(ChangeCountFormat, _plan.Changes.Count));

                if (GUILayout.Button(label, _styles.HeaderPrimaryButton))
                    Apply();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(EditorMetrics.ItemGap);
        }

        // Enter applies and Escape cancels, the same as the boxes of the hub.
        private bool HandleKeys()
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return false;

            switch (current.keyCode)
            {
                case KeyCode.Return when _plan.Changes.Count > 0:
                case KeyCode.KeypadEnter when _plan.Changes.Count > 0:
                    current.Use();
                    Apply();

                    return true;

                case KeyCode.Escape:
                    current.Use();
                    Close();

                    return true;

                default:
                    return false;
            }
        }

        private void Apply()
        {
            DataSheetTable.Apply(_plan, _tableName);
            _onApplied?.Invoke();
            Close();
        }
    }
}