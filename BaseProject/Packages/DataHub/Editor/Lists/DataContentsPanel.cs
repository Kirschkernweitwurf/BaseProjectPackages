using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Extensions;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Lists
{
    /// <summary>
    /// The Contents tab: every asset list of the selected asset, shown as one line per distinct entry
    /// with its count. A deck reads "Hello x3, Joke x1" instead of four slots, and the designer changes
    /// it with minus, plus, a typed count, the add row, or by dropping assets from the table on it.
    /// </summary>
    internal sealed class DataContentsPanel
    {
        private const string AddLabel = "Add";
        private const string AddTooltip = "Add the picked asset this many times.";
        private const float AmountWidth = 40f;
        private const float CardInset = 8f;
        private const string CountTooltip = "Entries in this list, copies included.";
        private const string DropLabel = "Drop to add";
        private const string EmptyListText = "Empty. Drop rows here or pick one below.";
        private const string NoListsHint = "This kind of asset holds no lists.";
        private const string NoListsMessage = "Nothing to fill";
        private const string SelectOneHint = "Lists like the cards of a deck show up here.";
        private const string SelectOneMessage = "Select one asset";
        private const string SummaryFailedFormat = "This summary failed: {0}";

        private readonly Dictionary<string, int> _addAmounts = new();
        private readonly Dictionary<string, Object> _addPicks = new();
        private readonly List<DataListGroup> _groups = new();
        private readonly DataHubRegistry _registry;
        private readonly List<IDataSummary> _summaries = new();

        private string _dropTarget;
        private Object _owner;
        private Vector2 _scroll;
        private SerializedObject _serialized;

        /// <summary>Creates the tab.</summary>
        /// <param name="registry">The extensions, for the summaries shown above the lists.</param>
        internal DataContentsPanel(DataHubRegistry registry) => _registry = registry;

        /// <summary>Draws the lists of the selected asset.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="targets">The selected assets.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when a list was changed.</returns>
        internal bool Draw(Rect area, Object[] targets, DataHubStyles styles)
        {
            GUILayout.BeginArea(area);

            bool isChanged = DrawContent(targets, styles);

            GUILayout.EndArea();

            return isChanged;
        }

        /// <summary>Releases the cached serialized object.</summary>
        internal void Dispose()
        {
            _serialized?.Dispose();
            _serialized = null;
            _owner = null;
        }

        private static void DrawListHeader(DataListField field, int count, DataHubStyles styles)
        {
            Rect line = GUILayoutUtility.GetRect(0f, EditorTableStyles.RowHeight, GUILayout.ExpandWidth(true));

            GUIContent label = new(field.Label);
            GUI.Label(line, label, styles.SectionHeader);

            GUIContent total = new(count.ToString(), CountTooltip);
            float labelWidth = styles.SectionHeader.CalcSize(label).x;
            float pillWidth = DataHubGui.PillWidth(total.text, styles);

            Rect pill = new(line.x + labelWidth + EditorMetrics.ItemGap,
                line.center.y - EditorMetrics.BadgeHeight * 0.5f, pillWidth, EditorMetrics.BadgeHeight);

            DataHubGui.Pill(pill, total, EditorTableStyles.NeutralBadgeColor, styles);
        }

        // The project draws the inside; the title and the card around it come from here, so every summary
        // looks like the lists under it. A summary that throws shows its message instead of breaking the tab.
        private static void DrawSummary(IDataSummary summary, Object asset, DataHubStyles styles)
        {
            GUILayout.Label(summary.Title, styles.SectionHeader);

            EditorGUILayout.BeginVertical(styles.Card);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(CardInset);
            EditorGUILayout.BeginVertical();

            try
            {
                summary.Draw(asset);
            }
            catch (Exception exception) when (!(exception is ExitGUIException))
            {
                GUILayout.Label(string.Format(SummaryFailedFormat, exception.Message), styles.Detail);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(CardInset);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.Space(EditorMetrics.SectionGap);
        }

        private static void DrawEmptyLine(DataHubStyles styles)
        {
            Rect line = GUILayoutUtility.GetRect(0f, DataListRowGui.Height, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(line.x + CardInset, line.y, line.width - CardInset * 2f, line.height), EmptyListText,
                styles.Detail);
        }

        private bool DrawContent(Object[] targets, DataHubStyles styles)
        {
            if (targets.Length != 1
                || targets[0] == null)
            {
                EditorWindowChrome.DrawEmptyState(styles, null, SelectOneMessage, SelectOneHint);
                return false;
            }

            IReadOnlyList<DataListField> fields = DataListField.Of(targets[0].GetType());

            _summaries.Clear();
            _registry.CollectSummaries(targets[0].GetType(), _summaries);

            if (fields.Count == 0
                && _summaries.Count == 0)
            {
                EditorWindowChrome.DrawEmptyState(styles, null, NoListsMessage, NoListsHint);
                return false;
            }

            Bind(targets[0]);
            _serialized.Update();

            bool isChanged = false;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (IDataSummary summary in _summaries)
                DrawSummary(summary, targets[0], styles);

            foreach (DataListField field in fields)
            {
                if (DrawList(field, styles))
                    isChanged = true;

                GUILayout.Space(EditorMetrics.SectionGap);
            }

            EditorGUILayout.EndScrollView();

            return isChanged;
        }

        private void Bind(Object owner)
        {
            if (owner == _owner)
                return;

            Dispose();

            _owner = owner;
            _serialized = new SerializedObject(owner);
        }

        private bool DrawList(DataListField field, DataHubStyles styles)
        {
            SerializedProperty list = _serialized.FindProperty(field.PropertyPath);

            if (list == null)
                return false;

            DataListEditing.Group(list, _groups);
            DrawListHeader(field, list.arraySize, styles);

            Rect section = EditorGUILayout.BeginVertical(styles.Card);
            bool isChanged = false;

            if (_groups.Count == 0)
                DrawEmptyLine(styles);

            // Copied, because a change rewrites the groups while they are being drawn.
            DataListGroup[] groups = _groups.ToArray();

            foreach (DataListGroup group in groups)
            {
                int count = DataListRowGui.DrawLayout(group.Item, null, group.Count, false, true, styles,
                    out _);

                if (count == group.Count)
                    continue;

                DataListEditing.SetCount(_serialized, field, group.Item, count);
                isChanged = true;
            }

            if (DrawAddRow(field, styles))
                isChanged = true;

            EditorGUILayout.EndVertical();

            return HandleDrop(section, field, styles) || isChanged;
        }

        private bool DrawAddRow(DataListField field, DataHubStyles styles)
        {
            GUILayout.Space(EditorMetrics.TightGap);

            Rect line = GUILayoutUtility.GetRect(0f, EditorTableStyles.RowHeight, GUILayout.ExpandWidth(true));
            Rect row = new(line.x + CardInset, line.y, line.width - CardInset * 2f, line.height);

            GUIContent addContent = new(AddLabel, DataHubIcons.New, AddTooltip);
            float buttonWidth = styles.HeaderButton.CalcSize(addContent).x;
            float gap = EditorMetrics.TightGap;
            float fieldTop = row.y + (row.height - EditorGUIUtility.singleLineHeight) * 0.5f;

            Rect button = new(row.xMax - buttonWidth, row.y + (row.height - DataHubStyles.ButtonHeight) * 0.5f,
                buttonWidth, DataHubStyles.ButtonHeight);

            Rect amount = new(button.x - gap - AmountWidth, fieldTop, AmountWidth, EditorGUIUtility.singleLineHeight);
            Rect pick = new(row.x, fieldTop, Mathf.Max(0f, amount.x - gap - row.x),
                EditorGUIUtility.singleLineHeight);

            _addPicks.TryGetValue(field.PropertyPath, out Object picked);

            if (!_addAmounts.TryGetValue(field.PropertyPath, out int count))
                count = 1;

            picked = EditorGUI.ObjectField(pick, picked, field.ElementType, false);
            count = Mathf.Max(1, EditorGUI.IntField(amount, count));

            _addPicks[field.PropertyPath] = picked;
            _addAmounts[field.PropertyPath] = count;

            using (new EditorGUI.DisabledScope(picked == null))
            {
                if (!GUI.Button(button, addContent, styles.HeaderButton))
                    return false;
            }

            DataListEditing.Change(_serialized, field, picked, count);

            return true;
        }

        // Rows dragged from the table, or assets from the Project window, join the list once each. The
        // list under the mouse lights up, so it is clear which of several lists takes the drop.
        private bool HandleDrop(Rect section, DataListField field, DataHubStyles styles)
        {
            Event current = Event.current;
            bool isTarget = _dropTarget == field.PropertyPath;

            switch (current.type)
            {
                case EventType.Repaint when isTarget:
                    DataHubGui.Background(section, styles.DropHighlight);

                    GUI.Label(new Rect(section.x, section.center.y - EditorTableStyles.RowHeight * 0.5f, section.width,
                        EditorTableStyles.RowHeight), DropLabel, styles.EmptyTitle);

                    return false;

                case EventType.DragUpdated:
                    bool isOver = section.Contains(current.mousePosition)
                        && field.Accepts(DragAndDrop.objectReferences);

                    if (!isOver)
                    {
                        if (isTarget)
                            _dropTarget = null;

                        return false;
                    }

                    _dropTarget = field.PropertyPath;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    current.Use();

                    return false;

                case EventType.DragPerform when isTarget:
                    _dropTarget = null;
                    DragAndDrop.AcceptDrag();

                    foreach (Object dropped in DragAndDrop.objectReferences)
                        DataListEditing.Change(_serialized, field, dropped, 1);

                    current.Use();

                    return true;

                case EventType.DragExited:
                    _dropTarget = null;
                    return false;

                default:
                    return false;
            }
        }
    }
}