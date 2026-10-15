using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Lists
{
    /// <summary>
    /// The Used In tab: every list in the project that can hold the selected assets, with how often they
    /// are in it. Select a card, press plus on a deck five times, and the deck holds five more of it.
    /// With several cards selected, every change applies to each of them. Lists that hold the selection
    /// come first, the ones it could still go into after them.
    /// </summary>
    internal sealed class DataUsagePanel
    {
        private const string InUseFormat = "In use  ({0})";
        private const string InUseTooltip = "Lists that hold the selected rows at least once. The number is how "
            + "often, a dash when the selected rows differ.";
        private const string NoListsHint = "No asset in the project has a list of this type.";
        private const string NoListsMessage = "Not usable anywhere";
        private const string NothingSelectedHint = "Every deck and collection that can hold the rows shows up here.";
        private const string NothingSelectedMessage = "Select one or more rows";
        private const string NotInUseFormat = "Not in use  ({0})";
        private const string NotInUseTooltip = "Lists that could hold the selected rows but do not yet. Press plus "
            + "to add them.";

        private readonly List<DataListUse> _inUse = new();
        private readonly List<DataListUse> _notInUse = new();
        private readonly Dictionary<Object, SerializedObject> _serialized = new();
        private readonly List<DataListUse> _uses = new();

        private Object[] _boundTargets;
        private Vector2 _scroll;

        /// <summary>Forgets every list found, so the next draw searches the project again.</summary>
        internal void Invalidate() => _boundTargets = null;

        /// <summary>Draws where the selected assets are used.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="targets">The selected assets.</param>
        /// <param name="index">The assets of the project, to find the lists.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when a list was changed.</returns>
        internal bool Draw(Rect area, Object[] targets, DataAssetIndex index, DataHubStyles styles)
        {
            GUILayout.BeginArea(area);

            bool isChanged = DrawContent(targets, index, styles);

            GUILayout.EndArea();

            return isChanged;
        }

        /// <summary>Releases every cached serialized object.</summary>
        internal void Dispose()
        {
            foreach (SerializedObject serialized in _serialized.Values)
                serialized.Dispose();

            _serialized.Clear();
            _uses.Clear();
            _boundTargets = null;
        }

        private static int CountAll(SerializedProperty list, Object[] targets, out bool isMixed)
        {
            int first = DataListEditing.Count(list, targets[0]);
            isMixed = false;

            for (int i = 1; i < targets.Length; i++)
            {
                if (DataListEditing.Count(list, targets[i]) != first)
                    isMixed = true;
            }

            return first;
        }

        // One undo step for the whole selection, however many assets it touches. A step changes each
        // asset by one, a typed number sets each to it, so mixed counts behave as the user expects.
        private static void Apply(SerializedObject serialized, DataListField field, Object[] targets, int shown,
            int wanted, bool isStep)
        {
            int group = Undo.GetCurrentGroup();

            foreach (Object asset in targets)
            {
                if (isStep)
                    DataListEditing.Change(serialized, field, asset, wanted - shown);
                else
                    DataListEditing.SetCount(serialized, field, asset, wanted);
            }

            Undo.CollapseUndoOperations(group);
        }

        private static void DrawSectionLabel(string format, string tooltip, int count, DataHubStyles styles)
        {
            Rect line = GUILayoutUtility.GetRect(0f, EditorTableStyles.RowHeight, GUILayout.ExpandWidth(true));
            GUI.Label(line, new GUIContent(string.Format(format, count), tooltip), styles.SectionLabel);
        }

        private bool DrawContent(Object[] targets, DataAssetIndex index, DataHubStyles styles)
        {
            if (targets.Length == 0
                || Array.Exists(targets, target => target == null))
            {
                EditorWindowChrome.DrawEmptyState(styles, null, NothingSelectedMessage, NothingSelectedHint);
                return false;
            }

            Bind(targets, index);

            if (_uses.Count == 0)
            {
                EditorWindowChrome.DrawEmptyState(styles, null, NoListsMessage, NoListsHint);
                return false;
            }

            Split(targets);

            bool isChanged = false;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (DrawSection(InUseFormat, InUseTooltip, _inUse, targets, styles))
                isChanged = true;

            if (DrawSection(NotInUseFormat, NotInUseTooltip, _notInUse, targets, styles))
                isChanged = true;

            EditorGUILayout.EndScrollView();

            return isChanged;
        }

        // Searched once per selection rather than per frame, because it walks every asset of the project.
        private void Bind(Object[] targets, DataAssetIndex index)
        {
            if (targets == _boundTargets)
                return;

            Dispose();

            _boundTargets = targets;

            HashSet<Object> selected = new(targets);

            foreach (DataAssetEntry entry in index.Entries)
            {
                foreach (DataListField field in DataListField.Of(entry.Type))
                {
                    if (!field.Accepts(targets))
                        continue;

                    Object owner = AssetDatabase.LoadMainAssetAtPath(entry.Path);

                    if (owner == null
                        || selected.Contains(owner))
                        continue;

                    _uses.Add(new DataListUse(owner, field));
                }
            }

            _uses.Sort((first, second) => string.Compare(first.Owner.name, second.Owner.name,
                StringComparison.OrdinalIgnoreCase));
        }

        // Sorted into the two sections on the layout pass and kept for the repaint after it, so a count
        // changed by a click only moves its line once the next layout comes.
        private void Split(Object[] targets)
        {
            if (Event.current.type != EventType.Layout)
                return;

            _inUse.Clear();
            _notInUse.Clear();

            foreach (DataListUse use in _uses)
            {
                // Deleted since the selection was bound. The next project change binds again.
                if (use.Owner == null)
                    continue;

                SerializedProperty list = ListOf(use);

                if (list == null)
                    continue;

                int count = CountAll(list, targets, out bool isMixed);

                List<DataListUse> section = count > 0 || isMixed
                    ? _inUse
                    : _notInUse;

                section.Add(use);
            }
        }

        private SerializedProperty ListOf(DataListUse use)
        {
            if (!_serialized.TryGetValue(use.Owner, out SerializedObject serialized))
            {
                serialized = new SerializedObject(use.Owner);
                _serialized.Add(use.Owner, serialized);
            }

            serialized.Update();

            return serialized.FindProperty(use.Field.PropertyPath);
        }

        private bool DrawSection(string format, string tooltip, List<DataListUse> uses, Object[] targets,
            DataHubStyles styles)
        {
            if (uses.Count == 0)
                return false;

            DrawSectionLabel(format, tooltip, uses.Count, styles);
            EditorGUILayout.BeginVertical(styles.Card);

            bool isChanged = false;

            foreach (DataListUse use in uses)
            {
                if (DrawUse(use, targets, styles))
                    isChanged = true;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(EditorMetrics.SectionGap);

            return isChanged;
        }

        private bool DrawUse(DataListUse use, Object[] targets, DataHubStyles styles)
        {
            if (use.Owner == null)
                return false;

            SerializedProperty list = ListOf(use);

            if (list == null)
                return false;

            int count = CountAll(list, targets, out bool isMixed);
            int wanted = DataListRowGui.DrawLayout(use.Owner, use.Field.Label, count, isMixed, false, styles,
                out bool isStep);

            if (wanted == count
                && !isStep)
                return false;

            Apply(list.serializedObject, use.Field, targets, count, wanted, isStep);

            return true;
        }
    }
}