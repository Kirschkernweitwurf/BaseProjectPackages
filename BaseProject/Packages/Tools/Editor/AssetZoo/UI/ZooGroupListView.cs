using System;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.AssetZoo;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.ToolsPackage.Editor.AssetZoo.Generation;
using Base.ToolsPackage.Editor.AssetZoo.Overlay;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// The groups of a saved zoo as pictures, since artists find a mesh by looking at it rather than by reading its
    /// name. A row folds open into a wall of thumbnails. It stays a real list: rows drag to a new place, assets drag
    /// from group to group and in from the Project window, and plus and minus work the way they do everywhere.
    /// </summary>
    internal sealed class ZooGroupListView
    {
        private const string ChangeUndoLabel = "Change Zoo Group";
        private const string ColorLabel = "Color";
        private const float ColorWidth = 44f;
        private const string CountFormat = "{0}";
        private const string CountOfFormat = "{0} of {1}";
        private const float CountWidth = 44f;
        private const float DotSize = 8f;
        private const string DoubleClickTip = "Click to find it in the Project window. Double-click to fly to it.";
        private const string DropHint = "Drop prefabs here";
        private const string EmptyLabel = "Nothing here yet. Add a folder above.";
        private const string EyeHiddenIcon = "scenevis_hidden_hover";
        private const string EyeVisibleIcon = "scenevis_visible_hover";
        private const string FlyIcon = "Camera Icon";
        private const string FlyTooltip = "Fly to this group in the Scene view.";
        private const float FoldWidth = 14f;
        private const string HiddenBadge = "Hidden";
        private const string HideGroupTooltip = "Leave this group out of the zoo. A rescan keeps it that way.";
        private const string HideItemTooltip = "Leave this asset out of the zoo. A rescan keeps it that way.";
        private const float IconSize = 20f;
        private const string NameLabel = "Name";
        private const string NewGroupName = "New Group";
        private const float RowHeight = 36f;
        private const float RowInset = 4f;
        private const string ShowGroupTooltip = "Bring this group back into the zoo.";
        private const string ShowItemTooltip = "Bring this asset back into the zoo.";
        private const float StripGap = 3f;
        private const float StripSize = 26f;
        private const float TileGap = 6f;
        private const float TileNameHeight = 14f;
        private const float TileRadius = 6f;
        private const float TileSize = 72f;

        private readonly EditorList _list;
        private readonly SerializedProperty _categories;
        private readonly ZooConfig _config;
        private readonly Action _onChanged;
        private readonly GUIContent _sharedContent = new();

        private string _filter = string.Empty;
        private ZooWindowStyles _styles;
        private float _tilesWidth = 400f;

        /// <summary>How many groups the zoo has.</summary>
        public int Count => _categories.arraySize;

        /// <summary>Creates the view over the groups of one saved zoo.</summary>
        /// <param name="categories">The array property of the groups.</param>
        /// <param name="config">The saved zoo behind it.</param>
        /// <param name="onChanged">Runs after anything changed the zoo should follow.</param>
        public ZooGroupListView(SerializedProperty categories, ZooConfig config, Action onChanged)
        {
            _categories = categories;
            _config = config;
            _onChanged = onChanged;

            _list = new EditorList(categories)
            {
                EmptyLabel = EmptyLabel,
                DrawElement = (rect, index, isActive) => DrawRow(rect, index),
                ElementHeight = HeightOf,
                OnAdd = AddGroup
            };
        }

        /// <summary>Draws the list.</summary>
        /// <param name="styles">The built styles.</param>
        /// <param name="filter">Text a group or one of its assets has to contain, or empty.</param>
        public void Draw(ZooWindowStyles styles, string filter)
        {
            _styles = styles;
            _filter = filter;

            // A filtered row is collapsed to nothing rather than dropped, so dragging one while the filter is on
            // would move it somewhere nobody can see.
            _list.Draggable = string.IsNullOrEmpty(filter);

            _list.DrawLayout(styles);
        }

        /// <summary>Folds every group open or closed at once.</summary>
        /// <param name="isExpanded">True to open all, false to close all.</param>
        public void SetAllExpanded(bool isExpanded)
        {
            for (int index = 0; index < _categories.arraySize; index++)
                _categories.GetArrayElementAtIndex(index).isExpanded = isExpanded;
        }

        /// <summary>True while at least one group is folded open.</summary>
        public bool HasExpanded()
        {
            for (int index = 0; index < _categories.arraySize; index++)
            {
                if (_categories.GetArrayElementAtIndex(index).isExpanded)
                    return true;
            }

            return false;
        }

        private static bool IsMatch(string text, string filter)
            => text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

        private static void DrawDot(Rect rect, Color color) => ZooControls.DrawRounded(rect, color, DotSize * 0.5f);

        private static ZooItem FindBuilt(GameObject asset)
        {
            foreach (ZooItem item in ZooSceneIndex.Items)
            {
                if (item != null
                    && !item.IsFigure
                    && item.Asset == asset)
                    return item;
            }

            return null;
        }

        private static ZooGroup FindBuilt(string groupName)
        {
            foreach (ZooGroup group in ZooSceneIndex.Groups)
            {
                if (group != null
                    && group.GroupName == groupName)
                    return group;
            }

            return null;
        }

        private ZooCategory CategoryAt(int index) => _config.Categories[index];

        private bool Matches(int index)
        {
            if (string.IsNullOrEmpty(_filter))
                return true;

            ZooCategory category = CategoryAt(index);

            if (IsMatch(category.Name, _filter))
                return true;

            // An asset's name matching is enough, so searching for one asset finds the group it sits in.
            foreach (GameObject entry in category.Entries)
            {
                if (entry != null
                    && IsMatch(entry.name, _filter))
                    return true;
            }

            return false;
        }

        private bool IsOpen(int index) => _categories.GetArrayElementAtIndex(index).isExpanded
            || !string.IsNullOrEmpty(_filter);

        private int CountShownTiles(ZooCategory category)
        {
            if (string.IsNullOrEmpty(_filter)
                || IsMatch(category.Name, _filter))
                return category.Entries.Count;

            int shown = 0;

            foreach (GameObject entry in category.Entries)
            {
                if (entry != null
                    && IsMatch(entry.name, _filter))
                    shown++;
            }

            return shown;
        }

        private bool IsTileShown(ZooCategory category, GameObject entry) => string.IsNullOrEmpty(_filter)
            || IsMatch(category.Name, _filter)
            || (entry != null && IsMatch(entry.name, _filter));

        private int ColumnCount() => Mathf.Max(1, Mathf.FloorToInt((_tilesWidth + TileGap) / (TileSize + TileGap)));

        private float HeightOf(int index)
        {
            // A filtered out row is collapsed to nothing rather than skipped, because the list numbers its rows
            // itself and dropping one would shift every index behind it.
            if (index >= _config.Categories.Count
                || !Matches(index))
                return 0f;

            if (!IsOpen(index))
                return RowHeight;

            int tiles = Mathf.Max(1, CountShownTiles(CategoryAt(index)));
            int rows = Mathf.CeilToInt(tiles / (float)ColumnCount());

            return RowHeight + EditorGUIUtility.singleLineHeight + TileGap * 2f
                + rows * (TileSize + TileNameHeight + TileGap);
        }

        private void DrawRow(Rect rect, int index)
        {
            if (index >= _config.Categories.Count
                || !Matches(index))
                return;

            ZooCategory category = CategoryAt(index);

            HandleDropOnRow(rect, category);

            Rect head = new(rect.x, rect.y, rect.width, RowHeight);
            DrawHead(head, index, category);

            if (!IsOpen(index))
                return;

            float y = head.yMax;
            DrawGroupFields(new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight), index);

            y += EditorGUIUtility.singleLineHeight + TileGap;
            DrawTiles(new Rect(rect.x, y, rect.width, rect.yMax - y), category);
        }

        private void DrawHead(Rect head, int index, ZooCategory category)
        {
            SerializedProperty element = _categories.GetArrayElementAtIndex(index);
            float centerY = head.y + (head.height - IconSize) * 0.5f;
            float x = head.x;

            Rect fold = new(x, head.y, FoldWidth, head.height);
            element.isExpanded = EditorGUI.Foldout(fold, element.isExpanded, GUIContent.none, true);
            x += FoldWidth;

            bool isHidden = category.IsHidden;

            Texture eye = EditorIcons.Named(isHidden
                ? EyeHiddenIcon
                : EyeVisibleIcon);

            string eyeTip = isHidden
                ? ShowGroupTooltip
                : HideGroupTooltip;

            if (ZooControls.IconButton(new Rect(x, centerY, IconSize, IconSize), eye, eyeTip))
                SetGroupHidden(category, !isHidden);

            x += IconSize + RowInset;

            Rect dot = new(x, head.y + (head.height - DotSize) * 0.5f, DotSize, DotSize);
            DrawDot(dot, category.LabelColor);
            x += DotSize + RowInset * 1.5f;

            _sharedContent.text = category.Name;
            float nameWidth = Mathf.Min(_styles.PillGroup.CalcSize(_sharedContent).x + RowInset, head.width * 0.4f);

            Rect name = new(x, head.y, nameWidth, head.height);
            GUI.Label(name, category.Name, isHidden ? _styles.Detail : _styles.NameBold);
            x += nameWidth + RowInset;

            string count = CountText(category);
            Rect countRect = new(x, head.y, CountWidth, head.height);
            GUI.Label(countRect, count, _styles.Detail);

            Rect fly = new(head.xMax - IconSize, centerY, IconSize, IconSize);
            ZooGroup built = FindBuilt(category.Name);

            using (new EditorGUI.DisabledScope(built == null))
            {
                if (ZooControls.IconButton(fly, EditorIcons.Named(FlyIcon), FlyTooltip))
                    ZooSceneNavigator.FlyToGroup(built);
            }

            if (!element.isExpanded)
                DrawStrip(category, x + CountWidth, fly.x - RowInset, head);
        }

        private static string CountText(ZooCategory category)
        {
            int hidden = category.HiddenEntries?.Count ?? 0;

            return hidden > 0
                ? string.Format(CountOfFormat, category.Entries.Count - hidden, category.Entries.Count)
                : string.Format(CountFormat, category.Entries.Count);
        }

        // A taste of what is in a closed group, right aligned so the rows read as columns.
        private static void DrawStrip(ZooCategory category, float left, float right, Rect head)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float x = right - StripSize;
            float y = head.y + (head.height - StripSize) * 0.5f;

            foreach (GameObject entry in category.Entries)
            {
                if (x < left)
                    return;

                if (entry == null
                    || category.IsEntryHidden(entry))
                    continue;

                Rect thumb = new(x, y, StripSize, StripSize);
                DrawThumbnail(thumb, entry, 1f);
                x -= StripSize + StripGap;
            }
        }

        private static void DrawThumbnail(Rect rect, GameObject asset, float alpha)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            ZooControls.DrawRounded(rect, EditorPalette.Field, TileRadius);
            ZooThumbnails.Draw(rect, asset, alpha, TileRadius);
        }

        private void DrawGroupFields(Rect line, int index)
        {
            SerializedProperty element = _categories.GetArrayElementAtIndex(index);
            SerializedProperty name = CustomEditorUtility.FindProp(element, nameof(ZooCategory.Name));
            SerializedProperty color = CustomEditorUtility.FindProp(element, nameof(ZooCategory.LabelColor));

            float previousWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = NameLabel.Length * 8f;

            Rect colorRect = new(line.xMax - ColorWidth, line.y, ColorWidth, line.height);
            Rect nameRect = new(line.x + FoldWidth, line.y, colorRect.x - line.x - FoldWidth - RowInset, line.height);

            EditorGUI.PropertyField(nameRect, name, new GUIContent(NameLabel));
            EditorGUI.PropertyField(colorRect, color, new GUIContent(string.Empty, ColorLabel));

            EditorGUIUtility.labelWidth = previousWidth;
        }

        private void DrawTiles(Rect area, ZooCategory category)
        {
            area.xMin += FoldWidth;

            if (Event.current.type == EventType.Repaint)
                _tilesWidth = area.width;

            if (category.Entries.Count == 0)
            {
                GUI.Label(area, DropHint, _styles.Detail);
                return;
            }

            int columns = ColumnCount();
            int shown = 0;
            GameObject toHide = null;
            GameObject toShow = null;

            foreach (GameObject entry in category.Entries)
            {
                if (entry == null
                    || !IsTileShown(category, entry))
                    continue;

                int column = shown % columns;
                int row = shown / columns;

                Rect tile = new(area.x + column * (TileSize + TileGap),
                    area.y + row * (TileSize + TileNameHeight + TileGap), TileSize, TileSize + TileNameHeight);

                bool isHidden = category.IsEntryHidden(entry);

                if (DrawTile(tile, entry, isHidden))
                {
                    if (isHidden)
                        toShow = entry;
                    else
                        toHide = entry;
                }

                shown++;
            }

            if (toHide != null)
                SetEntryHidden(category, toHide, true);

            if (toShow != null)
                SetEntryHidden(category, toShow, false);
        }

        // Returns true in the pass its eye was clicked.
        private bool DrawTile(Rect tile, GameObject asset, bool isHidden)
        {
            Rect picture = new(tile.x, tile.y, TileSize, TileSize);
            Rect label = new(tile.x, picture.yMax, TileSize, TileNameHeight);

            DrawThumbnail(picture, asset, isHidden
                ? 0.25f
                : 1f);

            if (isHidden
                && Event.current.type == EventType.Repaint)
            {
                _sharedContent.text = HiddenBadge;
                float badgeWidth = _styles.PillDetail.CalcSize(_sharedContent).x + RowInset * 2f;
                Rect badge = new(picture.x + (TileSize - badgeWidth) * 0.5f, picture.y + (TileSize - 16f) * 0.5f,
                    badgeWidth, 16f);

                ZooControls.DrawRounded(badge, EditorPalette.Secondary, 8f);
                GUI.Label(badge, HiddenBadge, _styles.Badge);
            }

            ZooControls.DrawText(label, asset.name, _styles.TileName, isHidden
                ? EditorPalette.DimText
                : EditorPalette.Text);

            GUI.Label(tile, new GUIContent(string.Empty, asset.name + "\n" + DoubleClickTip), GUIStyle.none);

            bool isHovered = picture.Contains(Event.current.mousePosition);
            bool isEyeClicked = false;

            if (isHovered)
            {
                Rect eye = new(picture.xMax - IconSize - 2f, picture.y + 2f, IconSize, IconSize);

                ZooControls.DrawRounded(eye, EditorPalette.Card, TileRadius);

                Texture icon = EditorIcons.Named(isHidden
                    ? EyeHiddenIcon
                    : EyeVisibleIcon);

                string tip = isHidden
                    ? ShowItemTooltip
                    : HideItemTooltip;

                isEyeClicked = ZooControls.IconButton(eye, icon, tip);
            }

            HandleTileMouse(picture, asset);

            return isEyeClicked;
        }

        private static void HandleTileMouse(Rect picture, GameObject asset)
        {
            Event current = Event.current;

            if (!picture.Contains(current.mousePosition))
                return;

            if (current.type == EventType.MouseDown
                && current.button == 0)
            {
                if (current.clickCount > 1)
                    ZooSceneNavigator.FlyToItem(FindBuilt(asset));
                else
                    EditorGUIUtility.PingObject(asset);

                // Taken here, or the list would start dragging the whole row.
                current.Use();

                return;
            }

            if (current.type != EventType.MouseDrag)
                return;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new Object[]
            {
                asset
            };
            DragAndDrop.StartDrag(asset.name);
            current.Use();
        }

        private void HandleDropOnRow(Rect rect, ZooCategory category)
        {
            Event current = Event.current;

            if (current.type != EventType.DragUpdated
                && current.type != EventType.DragPerform)
                return;

            if (!rect.Contains(current.mousePosition)
                || !HasDraggedAssets())
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Move;

            if (current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                MoveDraggedTo(category);
            }

            current.Use();
        }

        private static bool HasDraggedAssets()
        {
            foreach (Object dragged in DragAndDrop.objectReferences)
            {
                if (dragged is GameObject
                    && EditorUtility.IsPersistent(dragged))
                    return true;
            }

            return false;
        }

        private void MoveDraggedTo(ZooCategory target)
        {
            Undo.RecordObject(_config, ChangeUndoLabel);

            foreach (Object dragged in DragAndDrop.objectReferences)
            {
                if (dragged is not GameObject asset
                    || !EditorUtility.IsPersistent(asset))
                    continue;

                foreach (ZooCategory category in _config.Categories)
                    category.RemoveEntry(asset);

                target.TryAddEntry(asset);
            }

            Commit();
        }

        private void SetGroupHidden(ZooCategory category, bool isHidden)
        {
            Undo.RecordObject(_config, ChangeUndoLabel);
            category.SetHidden(isHidden);
            Commit();
        }

        private void SetEntryHidden(ZooCategory category, GameObject asset, bool isHidden)
        {
            Undo.RecordObject(_config, ChangeUndoLabel);
            category.SetEntryHidden(asset, isHidden);
            Commit();
        }

        private void AddGroup()
        {
            Undo.RecordObject(_config, ChangeUndoLabel);

            _config.Categories.Add(new ZooCategory(NewGroupName, ZooAutoGenerator.GetCategoryColor(NewGroupName),
                Array.Empty<GameObject>()));

            Commit();

            _categories.GetArrayElementAtIndex(_categories.arraySize - 1).isExpanded = true;
            _list.Select(_categories.arraySize - 1);
        }

        private void Commit()
        {
            EditorUtility.SetDirty(_config);
            _categories.serializedObject.Update();
            _onChanged();
        }
    }
}