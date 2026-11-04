using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.ToolsPackage.Editor.AssetZoo.Generation;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// The first thing an artist touches: the folders the zoo shows, as chips, with the whole card a drop target
    /// for more. Adding or removing one scans right away, because that is the only reason anyone would do it.
    /// </summary>
    internal sealed class ZooSourcesView
    {
        private const string AddLabel = "Add Folder…";
        private const string AddTooltip = "Pick a folder of art.";
        private const string AssetsPrefix = "Assets/";
        private const float ChipGap = 6f;
        private const float ChipHeight = 24f;
        private const float ChipMaxWidth = 280f;
        private const float ChipPadding = 10f;
        private const float ChipRadius = 12f;
        private const float DropLineWidth = 1.5f;
        private const float DropRadius = 8f;
        private const string EmptyHint = "Drop folders of art here";
        private const string PanelTitle = "Pick a folder of art";
        private const float RemoveGap = 4f;
        private const string RemoveLabel = "×";
        private const string RemoveTooltip = "Stop showing this. Its assets leave the zoo.";
        private const float RemoveWidth = 16f;

        private static readonly GUIContent SharedContent = new();

        private readonly Action _onSourcesChanged;

        private float _width = 300f;

        /// <summary>Creates the view.</summary>
        /// <param name="onSourcesChanged">Runs after a folder was added or removed, to scan.</param>
        public ZooSourcesView(Action onSourcesChanged) => _onSourcesChanged = onSourcesChanged;

        /// <summary>True while something that could be dropped here is being dragged.</summary>
        public static bool IsDraggingArt()
        {
            foreach (UnityEngine.Object dragged in DragAndDrop.objectReferences)
            {
                if (IsArtSource(dragged))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Draws the chips and the button to add more. Call inside the card.
        /// </summary>
        /// <param name="styles">The built styles.</param>
        /// <param name="config">The saved zoo.</param>
        public void Draw(ZooWindowStyles styles, ZooConfig config)
        {
            List<string> sources = config.Generation.Sources;
            float height = MeasureHeight(styles, sources);

            Rect area = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
                _width = area.width;

            float x = area.x;
            float y = area.y;
            string removed = null;

            foreach (string source in sources)
            {
                float width = MeasureChip(styles, source);
                Wrap(area, width, ref x, ref y);

                Rect chip = new(x, y, width, ChipHeight);

                if (DrawChip(styles, chip, source))
                    removed = source;

                x += width + ChipGap;
            }

            float addWidth = MeasureAdd(styles, sources);
            Wrap(area, addWidth, ref x, ref y);

            if (DrawAdd(styles, new Rect(x, y, addWidth, ChipHeight), sources))
                PickFolder(config);

            if (removed == null)
                return;

            ZooAutoGenerator.RemoveSource(config, removed);
            _onSourcesChanged();
            GUIUtility.ExitGUI();
        }

        /// <summary>
        /// Takes folders and art dropped on an area, and tints that area while something droppable is dragged.
        /// </summary>
        /// <param name="area">The area, usually the whole card.</param>
        /// <param name="config">The saved zoo.</param>
        public void HandleDrop(Rect area, ZooConfig config)
        {
            Event current = Event.current;

            if (IsDraggingArt())
            {
                ZooControls.DrawRoundedOutline(area, EditorPalette.Accent, DropLineWidth, DropRadius);
                ZooControls.DrawRounded(area, EditorPalette.WithAlpha(EditorPalette.Accent, 0.08f), DropRadius);
            }

            if (current.type != EventType.DragUpdated
                && current.type != EventType.DragPerform)
                return;

            if (!area.Contains(current.mousePosition)
                || !IsDraggingArt())
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                if (AddDropped(config))
                    _onSourcesChanged();
            }

            current.Use();
        }

        private static bool IsArtSource(UnityEngine.Object dragged)
        {
            string path = AssetDatabase.GetAssetPath(dragged);

            return !string.IsNullOrEmpty(path)
                && (AssetDatabase.IsValidFolder(path) || dragged is GameObject);
        }

        private static bool AddDropped(ZooConfig config)
        {
            bool isChanged = false;

            foreach (UnityEngine.Object dragged in DragAndDrop.objectReferences)
            {
                if (IsArtSource(dragged)
                    && ZooAutoGenerator.TryAddSource(config, AssetDatabase.GetAssetPath(dragged)))
                    isChanged = true;
            }

            return isChanged;
        }

        private static string GetLabel(string source) => source.StartsWith(AssetsPrefix, StringComparison.Ordinal)
            ? source[AssetsPrefix.Length..]
            : source;

        private static float MeasureChip(ZooWindowStyles styles, string source)
        {
            SharedContent.text = GetLabel(source);

            return Mathf.Min(ChipMaxWidth, styles.PillName.CalcSize(SharedContent).x + ChipPadding * 2f + RemoveWidth
                + RemoveGap);
        }

        private static float MeasureAdd(ZooWindowStyles styles, List<string> sources)
        {
            SharedContent.text = sources.Count == 0
                ? EmptyHint + " · " + AddLabel
                : AddLabel;

            return styles.PillName.CalcSize(SharedContent).x + ChipPadding * 2f;
        }

        private static void Wrap(Rect area, float width, ref float x, ref float y)
        {
            if (x <= area.x
                || x + width <= area.xMax)
                return;

            x = area.x;
            y += ChipHeight + ChipGap;
        }

        private static bool DrawChip(ZooWindowStyles styles, Rect chip, string source)
        {
            ZooControls.DrawRounded(chip, EditorPalette.Secondary, ChipRadius);

            Rect label = new(chip.x + ChipPadding, chip.y, chip.width - ChipPadding - RemoveWidth - RemoveGap,
                chip.height);
            ZooControls.DrawText(label, GetLabel(source), styles.Chip, EditorPalette.SecondaryText);

            if (Event.current.type == EventType.MouseDown
                && label.Contains(Event.current.mousePosition))
            {
                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(source);

                if (asset != null)
                    EditorGUIUtility.PingObject(asset);

                Event.current.Use();
            }

            Rect remove = new(chip.xMax - RemoveWidth - RemoveGap, chip.y, RemoveWidth, chip.height);

            Color previous = GUI.contentColor;
            GUI.contentColor = EditorPalette.SecondaryText;
            bool isRemoved = GUI.Button(remove, new GUIContent(RemoveLabel, RemoveTooltip), styles.Segment);
            GUI.contentColor = previous;

            return isRemoved;
        }

        private static bool DrawAdd(ZooWindowStyles styles, Rect rect, List<string> sources)
        {
            ZooControls.DrawRoundedOutline(rect, EditorPalette.Border, 1f, ChipRadius);

            if (rect.Contains(Event.current.mousePosition))
                ZooControls.DrawRounded(rect, EditorPalette.Hover, ChipRadius);

            string label = sources.Count == 0
                ? EmptyHint + " · " + AddLabel
                : AddLabel;

            Color previous = GUI.contentColor;
            GUI.contentColor = EditorPalette.DimText;
            bool isClicked = GUI.Button(rect, new GUIContent(label, AddTooltip), styles.Segment);
            GUI.contentColor = previous;

            return isClicked;
        }

        private float MeasureHeight(ZooWindowStyles styles, List<string> sources)
        {
            float x = 0f;
            float y = 0f;
            Rect area = new(0f, 0f, _width, 0f);

            foreach (string source in sources)
            {
                float width = MeasureChip(styles, source);
                Wrap(area, width, ref x, ref y);
                x += width + ChipGap;
            }

            Wrap(area, MeasureAdd(styles, sources), ref x, ref y);

            return y + ChipHeight;
        }

        private void PickFolder(ZooConfig config)
        {
            string picked = EditorUtility.OpenFolderPanel(PanelTitle, Application.dataPath, string.Empty);

            if (string.IsNullOrEmpty(picked))
                GUIUtility.ExitGUI();

            string relative = FileUtil.GetProjectRelativePath(picked);

            if (AssetDatabase.IsValidFolder(relative)
                && ZooAutoGenerator.TryAddSource(config, relative))
                _onSourcesChanged();

            GUIUtility.ExitGUI();
        }
    }
}