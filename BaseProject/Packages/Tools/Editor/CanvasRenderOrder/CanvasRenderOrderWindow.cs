using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.OverviewGui;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Overview = Base.ToolsPackage.Editor.OverviewGui.OverviewGui;

namespace Base.ToolsPackage.Editor.CanvasRenderOrder
{
    /// <summary>
    /// Editor window that lists every canvas in the order Unity draws it and changes its sorting layer and order
    /// in place, so the layering of all screens can be read and fixed from one list.
    /// </summary>
    internal sealed class CanvasRenderOrderWindow : EditorWindow
    {
        private const float ActionButtonHeight = 26f;
        private const float ActionButtonWidth = 150f;
        private const string CameraSectionLabel = "Camera and World Space: drawn first";
        private const string CanvasIconName = "Canvas Icon";
        private const string CanvasPlural = "canvases";
        private const string CanvasSingular = "canvas";
        private const string DisplayPrefix = "Display ";
        private const string EmptySubtitle = "No canvas sorts on its own in what is included.";
        private const string EmptyTitle = "No canvases";
        private const float FieldGap = 4f;
        private const float FieldInset = 2f;
        private const string FilterLabel = "Include:";
        private const float FilterLabelWidth = 54f;
        private const float GroupIconHeight = 16f;
        private const float GroupIconWidth = 18f;
        private const float IconSize = 16f;
        private const float LayerWidth = 120f;
        private const string LocationSeparator = " · ";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Canvas Render Order";
        private const float MinHeight = 320f;
        private const float MinWidth = 560f;
        private const float NameShare = 0.45f;
        private const string NoMatchHint = "No canvas matches the search.";
        private const float OrderWidth = 56f;
        private const string OverlaySectionLabel = "Screen Space Overlay: drawn on top";
        private const string PlayModeHint = "Play mode: scene changes are lost when you stop. Prefabs are locked.";
        private const float PrefabsToggleWidth = 78f;
        private const string RefreshLabel = "Refresh";
        private const float RowIndent = 22f;
        private const float ScenesToggleWidth = 74f;
        private const float SearchHeight = 20f;
        private const float SearchWidth = 200f;
        private const float Spacing = 4f;
        private const string SummaryFormat = "{0} {1}, back to front.";
        private const string TieFormat = " {0} share a layer and order with another canvas.";
        private const string TieTooltip = "Another canvas in this pass has the same layer and order. Unity then "
            + "decides by camera distance or hierarchy order, which can flip between screens.";
        private const string Title = "Canvas Render Order";

        private static readonly GUIContent PrefabsContent = new(" Prefabs",
            "Also lists the canvases of every prefab under Assets. Changes are saved into the prefab.");

        private static readonly GUIContent ScenesContent = new(" Scenes",
            "Lists the canvases of every open scene and of the prefab open in Prefab Mode.");

        private readonly List<CanvasEntry> _sceneEntries = new();
        private readonly List<CanvasEntry> _prefabEntries = new();
        private readonly List<CanvasEntry> _visible = new();

        private GUIContent[] _layerNames = Array.Empty<GUIContent>();
        private int[] _layerIds = Array.Empty<int>();
        private Vector2 _scroll;
        private string _search = string.Empty;
        private bool _isIncludingScenes = true;
        private bool _isIncludingPrefabs;
        private bool _isCameraExpanded = true;
        private bool _isOverlayExpanded = true;
        private bool _isViewDirty;
        private int _tieCount;
        private int _rowIndex;
        private CanvasEntry _hovered;
        private CanvasEntry _pendingEntry;
        private int _pendingLayerId;
        private int _pendingOrder;

#region Unity Callbacks
        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            RefreshLayers();
            RefreshScenes();
        }

        private void OnFocus()
        {
            RefreshLayers();
            RefreshScenes();
        }

        private void OnGUI()
        {
            Overview.EnsureStyles();
            HandleMouseMove();

            if (_isViewDirty)
                RebuildView();

            DrawActionBar();
            DrawFilters();
            DrawSummary();

            if (Application.isPlaying)
                EditorGUILayout.HelpBox(PlayModeHint, MessageType.Info);

            if (_sceneEntries.Count + _prefabEntries.Count == 0)
            {
                Overview.DrawSuccess(EmptyTitle, EmptySubtitle);
                return;
            }

            if (_visible.Count == 0)
            {
                Overview.DrawHint(NoMatchHint);
                return;
            }

            _hovered = null;
            _rowIndex = 0;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _isCameraExpanded = DrawSection(ECanvasPass.Camera, CameraSectionLabel, _isCameraExpanded);
            _isOverlayExpanded = DrawSection(ECanvasPass.Overlay, OverlaySectionLabel, _isOverlayExpanded);

            EditorGUILayout.EndScrollView();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            CanvasRenderOrderWindow window = GetWindow<CanvasRenderOrderWindow>();
            window.titleContent = new GUIContent(Title);
            window.minSize = new Vector2(MinWidth, MinHeight);
            window.Show();
        }

        private static void Navigate(CanvasEntry entry)
        {
            if (entry.Source == ECanvasSource.Prefab)
            {
                Overview.Navigate(entry.AssetPath);
                return;
            }

            if (entry.Canvas == null)
                return;

            Selection.activeObject = entry.Canvas.gameObject;
            EditorGUIUtility.PingObject(entry.Canvas.gameObject);
        }

        private void OnHierarchyChanged() => RefreshScenes();

        private void OnUndoRedoPerformed() => RefreshScenes();

        private void RefreshLayers()
        {
            SortingLayer[] layers = SortingLayer.layers;
            _layerNames = new GUIContent[layers.Length];
            _layerIds = new int[layers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                _layerNames[i] = new GUIContent(layers[i].name);
                _layerIds[i] = layers[i].id;
            }
        }

        private void RefreshScenes()
        {
            _sceneEntries.Clear();

            if (_isIncludingScenes)
                _sceneEntries.AddRange(CanvasScanner.ScanScenes());

            MarkViewDirty();
        }

        private void RefreshPrefabs()
        {
            _prefabEntries.Clear();

            if (_isIncludingPrefabs)
                _prefabEntries.AddRange(CanvasScanner.ScanPrefabs());

            MarkViewDirty();
        }

        private void MarkViewDirty()
        {
            _isViewDirty = true;
            Repaint();
        }

        private void DrawActionBar()
        {
            EditorGUILayout.Space(Spacing);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(Spacing, false);

                if (GUILayout.Button(RefreshLabel, GUILayout.Height(ActionButtonHeight),
                        GUILayout.Width(ActionButtonWidth)))
                {
                    RefreshLayers();
                    RefreshScenes();
                    RefreshPrefabs();
                }

                GUILayout.FlexibleSpace();

                EditorGUI.BeginChangeCheck();

                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField,
                    GUILayout.Width(SearchWidth), GUILayout.Height(SearchHeight));

                if (EditorGUI.EndChangeCheck())
                    MarkViewDirty();

                EditorGUILayout.Space(Spacing, false);
            }

            EditorGUILayout.Space(Spacing);
        }

        private void DrawFilters()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(FilterLabel, GUILayout.Width(FilterLabelWidth));

                bool isIncludingScenes = GUILayout.Toggle(_isIncludingScenes, ScenesContent,
                    GUILayout.Width(ScenesToggleWidth));

                bool isIncludingPrefabs = GUILayout.Toggle(_isIncludingPrefabs, PrefabsContent,
                    GUILayout.Width(PrefabsToggleWidth));

                GUILayout.FlexibleSpace();

                if (isIncludingScenes != _isIncludingScenes)
                {
                    _isIncludingScenes = isIncludingScenes;
                    RefreshScenes();
                }

                if (isIncludingPrefabs == _isIncludingPrefabs)
                    return;

                _isIncludingPrefabs = isIncludingPrefabs;

                // Scanning loads every prefab, so it runs after this GUI pass instead of in the middle of it.
                EditorApplication.delayCall += RefreshPrefabs;
            }
        }

        private void DrawSummary()
        {
            int count = _visible.Count;

            string message = string.Format(SummaryFormat, count, Overview.Plural(count, CanvasSingular, CanvasPlural));

            if (_tieCount > 0)
                message += string.Format(TieFormat, _tieCount);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                GUILayout.Label(message, Overview.HeaderStyle);
        }

        private void RebuildView()
        {
            _isViewDirty = false;
            _visible.Clear();

            AddMatches(_sceneEntries);
            AddMatches(_prefabEntries);

            _visible.Sort(CanvasEntry.CompareRenderOrder);
            MarkTies();
        }

        private void AddMatches(List<CanvasEntry> entries)
        {
            string term = _search.Trim();

            foreach (CanvasEntry entry in entries)
            {
                if (term.Length == 0
                    || entry.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                    || entry.Location.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    _visible.Add(entry);
            }
        }

        // The list is sorted, so canvases on the same slot sit next to each other. A scene instance and its own
        // prefab share an identity and never draw together, so they are not a tie.
        private void MarkTies()
        {
            _tieCount = 0;
            int start = 0;

            for (int i = 1; i <= _visible.Count; i++)
            {
                if (i < _visible.Count
                    && _visible[i].IsSameSlot(_visible[start]))
                    continue;

                MarkTieRun(start, i);
                start = i;
            }
        }

        private void MarkTieRun(int start, int end)
        {
            HashSet<Object> identities = new();

            for (int i = start; i < end; i++)
                identities.Add(_visible[i].Identity);

            bool hasTie = identities.Count > 1;

            for (int i = start; i < end; i++)
                _visible[i].HasTie = hasTie;

            if (hasTie)
                _tieCount += end - start;
        }

        private bool DrawSection(ECanvasPass pass, string label, bool isExpanded)
        {
            int count = 0;
            bool hasTie = false;

            foreach (CanvasEntry entry in _visible)
            {
                if (entry.Pass != pass)
                    continue;

                count++;
                hasTie |= entry.HasTie;
            }

            if (count == 0)
                return isExpanded;

            EOverviewAccent accent = hasTie
                ? EOverviewAccent.Warning
                : EOverviewAccent.Neutral;

            bool result = Overview.DrawSectionHeader(isExpanded, label, count, accent);

            if (!result)
                return false;

            CanvasEntry previous = null;

            foreach (CanvasEntry entry in _visible)
            {
                if (entry.Pass != pass)
                    continue;

                if (previous == null
                    || previous.GroupKey != entry.GroupKey)
                    DrawGroupHeader(entry);

                DrawRow(entry);
                previous = entry;
            }

            return true;
        }

        private void DrawGroupHeader(CanvasEntry entry)
        {
            string label = entry.Pass == ECanvasPass.Overlay
                ? DisplayPrefix + (entry.TargetDisplay + 1)
                : SortingLayer.IDToName(entry.SortingLayerId);

            using (new EditorGUILayout.HorizontalScope(Overview.GroupStyle))
            {
                GUILayout.Label(EditorGUIUtility.IconContent(CanvasIconName), GUILayout.Width(GroupIconWidth),
                    GUILayout.Height(GroupIconHeight));

                GUILayout.Label(label, EditorStyles.boldLabel);
            }
        }

        private void DrawRow(CanvasEntry entry)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, Overview.RowHeight);
            bool isEven = _rowIndex % 2 == 0;
            _rowIndex++;

            if (rect.Contains(Event.current.mousePosition))
                _hovered = entry;

            Overview.DrawRowBackground(rect, _hovered == entry, isEven);

            Rect body = new(rect.x + RowIndent, rect.y, rect.width - RowIndent, rect.height);
            float fieldsWidth = LayerWidth + OrderWidth + IconSize + FieldGap * 3f;
            float textWidth = body.width - fieldsWidth;

            Rect nameRect = new(body.x, body.y, textWidth * NameShare, body.height);
            Rect detailRect = new(nameRect.xMax, body.y, textWidth - nameRect.width, body.height);
            Rect layerRect = new(detailRect.xMax + FieldGap, body.y + FieldInset, LayerWidth,
                body.height - FieldInset * 2f);

            Rect orderRect = new(layerRect.xMax + FieldGap, layerRect.y, OrderWidth, layerRect.height);
            Rect tieRect = new(orderRect.xMax + FieldGap, body.y + (body.height - IconSize) * 0.5f, IconSize, IconSize);

            GUI.Label(nameRect, new GUIContent(entry.Name, entry.AssetPath), Overview.PathStyle);
            GUI.Label(detailRect, entry.Location + LocationSeparator + entry.Detail, Overview.DetailStyle);

            DrawSortingFields(entry, layerRect, orderRect);

            if (entry.HasTie)
                GUI.Label(tieRect, new GUIContent(EditorIcons.Warning, TieTooltip));

            if (Event.current.type != EventType.MouseDown
                || !nameRect.Contains(Event.current.mousePosition))
                return;

            Navigate(entry);
            Event.current.Use();
        }

        private void DrawSortingFields(CanvasEntry entry, Rect layerRect, Rect orderRect)
        {
            bool isLocked = entry.Source == ECanvasSource.Prefab && Application.isPlaying;

            using (new EditorGUI.DisabledScope(isLocked))
            {
                EditorGUI.BeginChangeCheck();

                int layerId = entry.SortingLayerId;

                // Overlay canvases ignore their sorting layer, so it is not offered there.
                if (entry.Pass == ECanvasPass.Camera)
                {
                    int index = Array.IndexOf(_layerIds, layerId);
                    int picked = EditorGUI.Popup(layerRect, index, _layerNames);

                    if (picked >= 0)
                        layerId = _layerIds[picked];
                }

                int order = EditorGUI.DelayedIntField(orderRect, entry.SortingOrder);

                if (!EditorGUI.EndChangeCheck())
                    return;

                // Saving a prefab reimports it, which must not happen in the middle of a GUI pass.
                _pendingEntry = entry;
                _pendingLayerId = layerId;
                _pendingOrder = order;
                EditorApplication.delayCall += WritePending;
            }
        }

        private void WritePending()
        {
            CanvasEntry entry = _pendingEntry;
            _pendingEntry = null;

            if (entry == null
                || !CanvasSortingWriter.TryWrite(entry, _pendingLayerId, _pendingOrder))
                return;

            entry.SetSorting(_pendingLayerId, _pendingOrder);
            MarkViewDirty();
        }

        private void HandleMouseMove()
        {
            wantsMouseMove = true;

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }
    }
}