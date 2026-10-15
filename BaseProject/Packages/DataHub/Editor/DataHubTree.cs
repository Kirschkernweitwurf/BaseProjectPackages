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
    /// The type tree on the left of the Data Hub: every table and page, nested by their paths, with the
    /// number of assets next to each table.
    /// </summary>
    internal sealed class DataHubTree
    {
        /// <summary>The character between the levels of a tree path.</summary>
        internal const char PathSeparator = '/';

        private readonly List<DataHubTreeNode> _nodes = new();

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
                return;
            }
        }

        /// <summary>Draws the tree and handles clicks.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when the selection changed.</returns>
        internal bool Draw(Rect area, DataHubStyles styles)
        {
            float rowHeight = EditorTableStyles.RowHeight;
            Rect view = new(0f, 0f, area.width - GUI.skin.verticalScrollbar.fixedWidth, _nodes.Count * rowHeight);

            bool isChanged = false;

            _scroll = GUI.BeginScrollView(area, _scroll, view, false, false);

            for (int i = 0; i < _nodes.Count; i++)
            {
                Rect row = new(0f, i * rowHeight, Mathf.Max(view.width, area.width), rowHeight);

                if (DrawNode(row, i, _nodes[i], styles))
                    isChanged = true;
            }

            GUI.EndScrollView();

            return isChanged;
        }

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

        // Returns the width the badge takes, so the label stops before it.
        private static float DrawCount(Rect row, int count, DataHubStyles styles)
        {
            string text = count.ToString();
            float width = EditorRows.MeasureBadge(text, styles.Badge, EditorTableStyles.MinBadgeWidth);

            Rect badge = new(row.xMax - EditorTableStyles.RowInset - width,
                row.y + (row.height - EditorMetrics.BadgeHeight) * 0.5f, width, EditorMetrics.BadgeHeight);

            EditorRows.DrawBadge(badge, text, EditorTableStyles.NeutralBadgeColor, styles.Badge);

            return width + EditorTableStyles.BadgeGap;
        }

        private static GUIStyle LabelStyle(DataHubTreeNode node, bool isSelected, DataHubStyles styles)
        {
            if (node.IsGroup)
                return styles.Header;

            return isSelected
                ? styles.NameBold
                : styles.Name;
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
                Flatten(byPath, child.Path, depth + 1);
            }
        }

        private bool DrawNode(Rect row, int index, DataHubTreeNode node, DataHubStyles styles)
        {
            bool isSelected = node == Selected;
            bool isHovered = !node.IsGroup
                && row.Contains(Event.current.mousePosition);

            EditorRows.DrawRowBackground(row, index, isHovered, isSelected);

            float indent = EditorTableStyles.RowInset + node.Depth * EditorMetrics.Indent;
            Rect label = new(row.x + indent, row.y, Mathf.Max(0f, row.width - indent), row.height);

            if (node.Table != null)
                label.width -= DrawCount(row, node.Count, styles);

            GUI.Label(label, node.Label, LabelStyle(node, isSelected, styles));

            Event current = Event.current;

            if (node.IsGroup
                || current.type != EventType.MouseDown
                || current.button != 0
                || !row.Contains(current.mousePosition))
                return false;

            current.Use();

            if (isSelected)
                return false;

            Selected = node;

            return true;
        }
    }
}