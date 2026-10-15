using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Extensions;
using Base.EditorUIPackage.Editor;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// The type tree on the left of the Data Hub: every table and page, nested by their paths and
    /// foldable, with the number of assets next to each table.
    /// </summary>
    internal sealed class DataHubTree
    {
        /// <summary>The character between the levels of a tree path.</summary>
        internal const char PathSeparator = '/';

        private const string CollapsedKeyFormat = "Base.DataHubPackage.Collapsed.{0}";
        private const float FoldoutWidth = 14f;
        private const string GroupTooltip = "Click to fold or unfold.";
        private const string PageTooltip = "A page made for this project.";
        private const float RowGap = 1f;
        private const float TopPadding = 6f;

        private readonly HashSet<string> _collapsed = new();
        private readonly List<DataHubTreeNode> _nodes = new();
        private readonly List<DataHubTreeNode> _visible = new();

        private Vector2 _scroll;

        /// <summary>The selected node, or null when the tree is empty.</summary>
        internal DataHubTreeNode Selected { get; private set; }

        /// <summary>True when no table and no page was found.</summary>
        internal bool IsEmpty => _nodes.Count == 0;

        /// <summary>Lays the tree out again from the registry.</summary>
        /// <param name="registry">The tables and pages to show.</param>
        internal void Build(DataHubRegistry registry)
        {
            Dictionary<string, DataHubTreeNode> byPath = new();

            foreach (IDataTable table in registry.Tables)
                AddNode(byPath, new DataHubTreeNode(table.Path, table.Order, table, null));

            foreach (IDataHubPage page in registry.Pages)
                AddNode(byPath, new DataHubTreeNode(page.Path, page.Order, null, page));

            AddMissingParents(byPath);

            _nodes.Clear();
            Flatten(byPath, null, 0);

            _collapsed.Clear();

            foreach (DataHubTreeNode node in _nodes)
            {
                if (node.HasChildren
                    && EditorPrefs.GetBool(CollapsedKey(node.Path)))
                    _collapsed.Add(node.Path);
            }

            CollectVisible();
        }

        /// <summary>Reads the asset counts of every table again.</summary>
        /// <param name="index">The assets of the project.</param>
        internal void RefreshCounts(DataAssetIndex index)
        {
            foreach (DataHubTreeNode node in _nodes)
            {
                if (node.Table != null)
                    node.Count = index.Count(node.Table.DataType);
            }
        }

        /// <summary>
        /// Selects the node with <paramref name="path"/>, or the first selectable node when there is
        /// none, so a renamed or removed entry never leaves the hub without a selection.
        /// </summary>
        /// <param name="path">The path to select.</param>
        internal void Select(string path)
        {
            Selected = null;

            foreach (DataHubTreeNode node in _nodes)
            {
                if (node.IsGroup)
                    continue;

                Selected ??= node;

                if (node.Path != path)
                    continue;

                Selected = node;
                break;
            }

            // A selection hidden in a folded group would leave the user wondering where they are.
            if (Selected != null)
                ExpandParents(Selected);
        }

        /// <summary>The path of the selected node's parents, for the title of the page, or null at the top.</summary>
        /// <returns>The parent path with spaced separators, or null.</returns>
        internal string SelectedParentPath() => Selected?.ParentPath?.Replace(PathSeparator.ToString(),
            " " + PathSeparator + " ");

        /// <summary>Draws the tree and handles clicks.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when the selection changed.</returns>
        internal bool Draw(Rect area, DataHubStyles styles)
        {
            float rowHeight = EditorTableStyles.RowHeight;
            float height = TopPadding + _visible.Count * (rowHeight + RowGap);
            bool hasScrollbar = height > area.height;

            float width = hasScrollbar
                ? area.width - GUI.skin.verticalScrollbar.fixedWidth
                : area.width;

            Rect view = new(0f, 0f, width, height);
            bool isChanged = false;

            _scroll = GUI.BeginScrollView(area, _scroll, view, false, false);

            // Copied, because a fold changes the visible lines while they are being drawn.
            DataHubTreeNode[] visible = _visible.ToArray();

            for (int i = 0; i < visible.Length; i++)
            {
                Rect row = new(0f, TopPadding + i * (rowHeight + RowGap), width, rowHeight);

                if (DrawNode(row, visible[i], styles))
                    isChanged = true;
            }

            GUI.EndScrollView();

            return isChanged;
        }

        private static string CollapsedKey(string path) => string.Format(CollapsedKeyFormat, path);

        private static void AddNode(Dictionary<string, DataHubTreeNode> byPath, DataHubTreeNode node)
        {
            if (string.IsNullOrEmpty(node.Path))
                return;

            // Two entries on one path would fight over the same line, so the second one is reported
            // instead of silently hiding one of them.
            if (byPath.ContainsKey(node.Path))
            {
                CustomLogger.LogError($"Two Data Hub entries use the path "
                    + $"'{node.Path}'. Only the first one is shown.", null);

                return;
            }

            byPath.Add(node.Path, node);
        }

        private static void AddMissingParents(Dictionary<string, DataHubTreeNode> byPath)
        {
            List<DataHubTreeNode> pending = new(byPath.Values);

            while (pending.Count > 0)
            {
                DataHubTreeNode node = pending[pending.Count - 1];
                pending.RemoveAt(pending.Count - 1);

                if (node.ParentPath == null)
                    continue;

                if (byPath.TryGetValue(node.ParentPath, out DataHubTreeNode parent))
                {
                    parent.Order = Math.Min(parent.Order, node.Order);
                    continue;
                }

                parent = new DataHubTreeNode(node.ParentPath, node.Order, null, null);
                byPath.Add(parent.Path, parent);
                pending.Add(parent);
            }
        }

        private static int CompareSiblings(DataHubTreeNode first, DataHubTreeNode second)
        {
            int result = first.Order.CompareTo(second.Order);

            return result != 0
                ? result
                : string.Compare(first.Label, second.Label, StringComparison.OrdinalIgnoreCase);
        }

        private static string TooltipOf(DataHubTreeNode node)
        {
            // A table needs no tooltip, its name and count say it all.
            if (node.Table != null)
                return string.Empty;

            return node.Page != null
                ? PageTooltip
                : GroupTooltip;
        }

        private static Texture IconOf(DataHubTreeNode node)
        {
            if (node.Table != null)
                return DataHubIcons.Of(node.Table.DataType);

            return node.Page != null
                ? DataHubIcons.Page
                : DataHubIcons.Folder;
        }

        // A plain dimmed number at the right edge. A badge per line made the tree read like a list of
        // warnings. Returns the width it takes, so the label stops before it.
        private static float DrawCount(Rect row, int count, bool isSelected, DataHubStyles styles)
        {
            GUIContent text = new(count.ToString());

            GUIStyle style = isSelected
                ? styles.CountSelected
                : styles.Count;

            float width = style.CalcSize(text).x;
            float right = row.xMax - DataHubStyles.RowHighlightInset - EditorTableStyles.RowInset;

            GUI.Label(new Rect(right - width, row.y, width, row.height), text, style);

            return width + EditorTableStyles.BadgeGap;
        }

        private void Flatten(Dictionary<string, DataHubTreeNode> byPath, string parentPath, int depth)
        {
            List<DataHubTreeNode> children = new();

            foreach (DataHubTreeNode node in byPath.Values)
            {
                if (node.ParentPath == parentPath)
                    children.Add(node);
            }

            children.Sort(CompareSiblings);

            foreach (DataHubTreeNode child in children)
            {
                child.Depth = depth;
                _nodes.Add(child);

                int before = _nodes.Count;
                Flatten(byPath, child.Path, depth + 1);
                child.HasChildren = _nodes.Count > before;
            }
        }

        private void CollectVisible()
        {
            _visible.Clear();

            int hiddenBelow = int.MaxValue;

            foreach (DataHubTreeNode node in _nodes)
            {
                if (node.Depth > hiddenBelow)
                    continue;

                hiddenBelow = _collapsed.Contains(node.Path)
                    ? node.Depth
                    : int.MaxValue;

                _visible.Add(node);
            }
        }

        private void ExpandParents(DataHubTreeNode node)
        {
            bool isChanged = false;

            for (string parent = node.ParentPath; parent != null; parent = DataHubTreeNode.ParentOf(parent))
            {
                if (_collapsed.Remove(parent))
                {
                    EditorPrefs.DeleteKey(CollapsedKey(parent));
                    isChanged = true;
                }
            }

            if (isChanged)
                CollectVisible();
        }

        private void Toggle(DataHubTreeNode node)
        {
            if (_collapsed.Remove(node.Path))
                EditorPrefs.DeleteKey(CollapsedKey(node.Path));
            else if (_collapsed.Add(node.Path))
                EditorPrefs.SetBool(CollapsedKey(node.Path), true);

            CollectVisible();
        }

        private bool DrawNode(Rect row, DataHubTreeNode node, DataHubStyles styles)
        {
            bool isSelected = node == Selected;
            bool isHovered = row.Contains(Event.current.mousePosition);

            Rect highlight = new(row.x + DataHubStyles.RowHighlightInset, row.y,
                row.width - DataHubStyles.RowHighlightInset * 2f, row.height);

            if (isSelected)
                DataHubGui.Highlight(highlight, DataHubStyles.TreeSelectionColor, styles);
            else if (isHovered)
                DataHubGui.Highlight(highlight, EditorPalette.Hover, styles);

            float x = highlight.x + EditorTableStyles.RowInset * 0.5f + node.Depth * EditorMetrics.Indent;
            Rect foldout = new(x, row.y, FoldoutWidth, row.height);

            if (node.HasChildren)
            {
                bool isExpanded = !_collapsed.Contains(node.Path);

                if (EditorGUI.Foldout(foldout, isExpanded, GUIContent.none) != isExpanded)
                {
                    Toggle(node);
                    return false;
                }
            }

            Rect icon = new(foldout.xMax, row.y + (row.height - EditorTableStyles.IconSize) * 0.5f,
                EditorTableStyles.IconSize, EditorTableStyles.IconSize);

            DataHubGui.Icon(icon, IconOf(node));

            Rect label = new(icon.xMax + EditorTableStyles.IconGap, row.y, 0f, row.height);
            label.width = Mathf.Max(0f, highlight.xMax - label.x);

            if (node.Table != null)
                label.width -= DrawCount(row, node.Count, isSelected, styles);

            GUIStyle labelStyle = isSelected
                ? styles.NameBold
                : styles.Name;

            GUI.Label(label, new GUIContent(node.Label, TooltipOf(node)), labelStyle);

            return HandleClick(row, node, isSelected);
        }

        private bool HandleClick(Rect row, DataHubTreeNode node, bool isSelected)
        {
            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || current.button != 0
                || !row.Contains(current.mousePosition))
                return false;

            current.Use();

            // A group has nothing to show, so a click on it folds it instead.
            if (node.IsGroup)
            {
                Toggle(node);
                return false;
            }

            if (isSelected)
                return false;

            Selected = node;

            return true;
        }
    }
}