using System.Collections.Generic;
using Base.DataHubPackage.Editor.Extensions;
using Base.DataHubPackage.Editor.Table;
using Base.DataHubPackage.Editor.Validation;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Overview
{
    /// <summary>
    /// The page the Data Hub opens on: one tile per table with its size and its problems, and every asset that
    /// needs fixing in one list, so a designer sees the state of the whole project before picking a table.
    /// </summary>
    internal sealed class DataOverviewPage
    {
        private const string AllGoodHint = "Every asset passes its checks.";
        private const string AllGoodMessage = "Nothing to fix";
        private const string IssuesTitle = "Needs a look";
        private const string IssueTooltip = "Click to open it in its table.";
        private const string IssueWhereFormat = "{0}  ·  {1}";
        private const int MaxIssues = 60;
        private const string MoreIssuesFormat = "And {0} more. Open a table and use its Problems filter.";
        private const float NameShare = 0.4f;
        private const float NameWidth = 260f;
        private const string SubtitleFormat = "{0} tables, {1} assets, {2} with problems, {3} with warnings";
        private const string TablesTitle = "Tables";
        private const float TileGap = 8f;
        private const float TileHeight = 64f;
        private const float TileInset = 10f;
        private const float TileMinWidth = 170f;
        private const string TileTooltipFormat = "{0}\n\nClick to open the table.";

        private readonly List<(Object Asset, IDataTable Table, string Text, bool IsProblem)> _issues = new();
        private readonly List<(IDataTable Table, int Count, int Problems, int Warnings)> _tiles = new();

        private int _assetCount;
        private int _problemCount;
        private Vector2 _scroll;
        private int _warningCount;

        /// <summary>The table whose tile was clicked this pass, or null.</summary>
        internal IDataTable OpenedTable { get; private set; }

        /// <summary>The asset clicked in the issue list this pass, or null.</summary>
        internal Object Revealed { get; private set; }

        /// <summary>One line on what the project holds, for the page header.</summary>
        internal string Subtitle => string.Format(SubtitleFormat, _tiles.Count, _assetCount, _problemCount,
            _warningCount);

        /// <summary>Checks every listed asset once and counts the results per table.</summary>
        /// <param name="registry">The extensions, for the table each asset belongs to.</param>
        /// <param name="tables">The tables in the order of the tree.</param>
        /// <param name="index">The assets of the project.</param>
        /// <param name="validation">The checks.</param>
        internal void Rebuild(DataHubRegistry registry, IEnumerable<IDataTable> tables, DataAssetIndex index,
            DataValidation validation)
        {
            _issues.Clear();
            _tiles.Clear();

            // A card sits in "All Cards" and in "Dialogue", so every asset is checked once and shared.
            Dictionary<string, (int Problems, int Warnings)> checks = new();
            List<string> paths = new();

            foreach (IDataTable table in tables)
            {
                paths.Clear();
                index.CollectPaths(table.DataType, paths);

                int problems = 0;
                int warnings = 0;

                foreach (string path in paths)
                {
                    if (!checks.TryGetValue(path, out (int Problems, int Warnings) check))
                    {
                        check = Check(path, registry, validation);
                        checks.Add(path, check);
                    }

                    if (check.Problems > 0)
                        problems++;

                    if (check.Warnings > 0)
                        warnings++;
                }

                _tiles.Add((table, paths.Count, problems, warnings));
            }

            _assetCount = checks.Count;
            _problemCount = 0;
            _warningCount = 0;

            foreach ((int Problems, int Warnings) check in checks.Values)
            {
                if (check.Problems > 0)
                    _problemCount++;

                if (check.Warnings > 0)
                    _warningCount++;
            }

            // Broken first, then by name, so the list reads like a to-do list.
            _issues.Sort((first, second) => first.IsProblem != second.IsProblem
                ? second.IsProblem.CompareTo(first.IsProblem)
                : string.CompareOrdinal(first.Asset.name, second.Asset.name));
        }

        /// <summary>Draws the page and records what was clicked.</summary>
        /// <param name="area">The area below the page header.</param>
        /// <param name="styles">The built window styles.</param>
        internal void Draw(Rect area, DataHubStyles styles)
        {
            OpenedTable = null;
            Revealed = null;

            GUILayout.BeginArea(area);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            GUILayout.Label(TablesTitle, styles.SectionHeader);
            DrawTiles(area.width, styles);

            GUILayout.Space(EditorMetrics.SectionGap);
            GUILayout.Label(IssuesTitle, styles.SectionHeader);
            DrawIssues(styles);

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private (int Problems, int Warnings) Check(string path, DataHubRegistry registry,
            DataValidation validation)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(path);

            if (asset == null)
                return (0, 0);

            using DataRow row = new(asset);
            validation.Validate(row);

            IDataTable home = registry.FindTable(asset.GetType());

            if (row.HasProblems)
                _issues.Add((asset, home, row.Problems[0], true));
            else if (row.HasWarnings)
                _issues.Add((asset, home, row.Warnings[0], false));

            return (row.Problems.Count, row.Warnings.Count);
        }

        private void DrawTiles(float width, DataHubStyles styles)
        {
            // The scroll bar may take part of the width, so the tiles leave room for it.
            float available = Mathf.Max(TileMinWidth, width - GUI.skin.verticalScrollbar.fixedWidth);
            int perRow = Mathf.Max(1, Mathf.FloorToInt((available + TileGap) / (TileMinWidth + TileGap)));
            float tileWidth = (available - TileGap * (perRow - 1)) / perRow;
            int rows = Mathf.CeilToInt(_tiles.Count / (float)perRow);

            Rect grid = GUILayoutUtility.GetRect(available, rows * (TileHeight + TileGap));

            for (int i = 0; i < _tiles.Count; i++)
            {
                float x = grid.x + i % perRow * (tileWidth + TileGap);
                float y = grid.y + i / perRow * (TileHeight + TileGap);
                Rect tile = new(x, y, tileWidth, TileHeight);

                if (DrawTile(tile, _tiles[i], styles))
                    OpenedTable = _tiles[i].Table;
            }
        }

        private static bool DrawTile(Rect tile, (IDataTable Table, int Count, int Problems, int Warnings) entry,
            DataHubStyles styles)
        {
            bool isHovered = tile.Contains(Event.current.mousePosition);

            DataHubGui.Background(tile, styles.Card);

            if (isHovered)
                DataHubGui.Highlight(tile, EditorPalette.Hover, styles);

            Rect inner = new(tile.x + TileInset, tile.y + TileInset, tile.width - TileInset * 2f,
                tile.height - TileInset * 2f);

            string label = entry.Table.Path.Substring(entry.Table.Path.LastIndexOf(DataHubTree.PathSeparator) + 1);
            Rect icon = new(inner.x, inner.y + 1f, EditorTableStyles.IconSize, EditorTableStyles.IconSize);

            DataHubGui.Icon(icon, DataHubIcons.Of(entry.Table.DataType));
            GUI.Label(new Rect(icon.xMax + EditorTableStyles.IconGap, inner.y, inner.width, EditorTableStyles.IconSize),
                label, styles.NameBold);

            Rect count = new(inner.x, inner.yMax - EditorMetrics.BadgeHeight - 2f, inner.width,
                EditorMetrics.BadgeHeight + 2f);

            GUI.Label(count, entry.Count.ToString(), styles.PageTitle);

            float right = inner.xMax;
            DrawCountPill(ref right, count, entry.Warnings, EditorTableStyles.WarningBadgeColor, styles);
            DrawCountPill(ref right, count, entry.Problems, EditorTableStyles.DangerBadgeColor, styles);

            GUI.Label(tile, new GUIContent(string.Empty, string.Format(TileTooltipFormat,
                entry.Table.Description ?? label)));

            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || current.button != 0
                || !isHovered)
                return false;

            current.Use();

            return true;
        }

        private static void DrawCountPill(ref float right, Rect line, int count, Color color, DataHubStyles styles)
        {
            if (count == 0)
                return;

            string text = count.ToString();
            float width = DataHubGui.PillWidth(text, styles);
            right -= width;

            Rect pill = new(right, line.y + (line.height - EditorMetrics.BadgeHeight) * 0.5f, width,
                EditorMetrics.BadgeHeight);

            DataHubGui.Pill(pill, new GUIContent(text), color, styles);
            right -= EditorMetrics.TightGap;
        }

        private void DrawIssues(DataHubStyles styles)
        {
            if (_issues.Count == 0)
            {
                EditorGUILayout.BeginVertical(styles.Card);
                GUILayout.Label(new GUIContent(AllGoodMessage, EditorIcons.Success), styles.CheckLine);
                GUILayout.Label(AllGoodHint, styles.Detail);
                EditorGUILayout.EndVertical();

                return;
            }

            EditorGUILayout.BeginVertical(styles.Card);

            int shown = Mathf.Min(_issues.Count, MaxIssues);

            for (int i = 0; i < shown; i++)
            {
                (Object asset, IDataTable table, string text, bool isProblem) = _issues[i];

                if (asset == null)
                    continue;

                if (DrawIssue(asset, table, text, isProblem, i, styles))
                    Revealed = asset;
            }

            if (_issues.Count > shown)
                GUILayout.Label(string.Format(MoreIssuesFormat, _issues.Count - shown), styles.Detail);

            EditorGUILayout.EndVertical();
        }

        private static bool DrawIssue(Object asset, IDataTable table, string text, bool isProblem, int index,
            DataHubStyles styles)
        {
            Rect row = GUILayoutUtility.GetRect(0f, EditorTableStyles.RowHeight, GUILayout.ExpandWidth(true));
            bool isHovered = row.Contains(Event.current.mousePosition);

            EditorRows.DrawRowBackground(row, index, isHovered);

            Texture icon = isProblem
                ? EditorIcons.Error
                : EditorIcons.Warning;

            Rect iconRect = new(row.x + EditorTableStyles.RowInset,
                row.y + (row.height - EditorTableStyles.IconSize) * 0.5f, EditorTableStyles.IconSize,
                EditorTableStyles.IconSize);

            DataHubGui.Icon(iconRect, icon);

            Rect name = new(iconRect.xMax + EditorTableStyles.IconGap, row.y,
                Mathf.Min(NameWidth, row.width * NameShare), row.height);
            Rect detail = new(name.xMax + EditorMetrics.ItemGap, row.y,
                Mathf.Max(0f, row.xMax - name.xMax - EditorMetrics.ItemGap - EditorTableStyles.RowInset), row.height);

            string where = table == null
                ? text
                : string.Format(IssueWhereFormat, table.Path, text);

            GUI.Label(name, new GUIContent(asset.name, IssueTooltip), styles.NameBold);
            GUI.Label(detail, new GUIContent(where, text), styles.Detail);

            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || current.button != 0
                || !isHovered)
                return false;

            current.Use();

            return true;
        }
    }
}