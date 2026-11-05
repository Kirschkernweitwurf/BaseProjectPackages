using System.Collections.Generic;
using Base.ToolsPackage.AssetZoo;
using Base.ToolsPackage.Editor.AssetZoo.Alignment;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.ToolsPackage.Editor.AssetZoo.Layout;
using Base.ToolsPackage.Editor.AssetZoo.Overlay;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Base.ToolsPackage.Editor.AssetZoo.Builder
{
    /// <summary>
    /// Builds an asset zoo into the active scene. Building and arranging are two steps on purpose: arranging only
    /// moves what stands there already, using what was measured when it was built, so it is fast enough to follow a
    /// slider while it is being dragged.
    /// </summary>
    internal sealed class ZooBuilder
    {
        /// <summary>How far the floor reaches past the zoo on every side.</summary>
        internal const float FloorMargin = 6f;

        private const float BoardDepth = 0.02f;
        private const string BoardName = "Decal Board";
        private const float BoardPadding = 1.2f;
        private const string BuildUndoLabel = "Build Asset Zoo";
        private const string ClearUndoLabel = "Clear Asset Zoo";
        private const float DecalTurn = 180f;
        private const string FloorName = "Floor";
        private const float FloorUnitsPerScale = 10f;
        private const int ProgressBarFrom = 30;
        private const string ProgressTitle = "Building Asset Zoo";
        private const string UnnamedGroup = "Unnamed Group";
        private const string ZooRootName = "Asset Zoo";

        /// <summary>
        /// Builds the zoo and arranges it. A zoo that stands in the scene already is replaced, and the new one
        /// takes its place, so one the artist moved or parented stays where they put it.
        /// </summary>
        /// <param name="config">The saved zoo.</param>
        /// <param name="newAssets">Assets to mark as new in the Scene view, or null for none.</param>
        public void Build(ZooConfig config, IReadOnlyCollection<GameObject> newAssets = null)
        {
            if (config == null)
            {
                CustomLogger.LogError("No saved zoo selected.", null);
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();

            ZooRootMarker previous = ZooSceneIndex.Root;
            bool hasPrevious = previous != null;
            Transform parent = null;
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            if (hasPrevious)
            {
                parent = previous.transform.parent;
                position = previous.transform.localPosition;
                rotation = previous.transform.localRotation;

                Undo.DestroyObjectImmediate(previous.gameObject);
            }

            // Measured at the origin with no rotation, so what is measured is the asset and not the zoo.
            GameObject root = new(ZooRootName);
            root.AddComponent<ZooRootMarker>();
            Undo.RegisterCreatedObjectUndo(root, BuildUndoLabel);

            HashSet<GameObject> isNew = newAssets == null
                ? new HashSet<GameObject>()
                : new HashSet<GameObject>(newAssets);

            BuildGroups(config, root.transform, isNew);

            ZooSceneIndex.Invalidate();
            Arrange(config);

            root.transform.SetParent(parent, false);
            root.transform.localRotation = rotation;

            root.transform.localPosition = hasPrevious
                ? position
                : ZooPlacement.FindFreeCorner(root.scene, root.transform);

            ZooSceneIndex.Invalidate();

            Undo.SetCurrentGroupName(BuildUndoLabel);
            Undo.CollapseUndoOperations(undoGroup);
        }

        /// <summary>
        /// Moves everything to where the current settings want it, without building anything. Does nothing while
        /// no zoo stands in the scene.
        /// </summary>
        /// <param name="config">The saved zoo.</param>
        public void Arrange(ZooConfig config)
        {
            ZooRootMarker root = ZooSceneIndex.Root;

            if (root == null
                || config == null
                || Application.isPlaying)
                return;

            ShowSettings show = config.Show;
            root.SetLabels(show.HasGroupNames, show.HasItemNames, show.HasSizes, show.HasRuler);

            List<ZooGroup> groups = CollectGroups(root);
            IAlignmentStrategy alignment = AlignmentStrategyFactory.Create(config.Layout.Alignment);
            ILayoutStrategy layout = LayoutStrategyFactory.Create(config.Layout.Type);

            List<ZooGroup> visible = new();
            List<Vector2> blocks = new();

            foreach (ZooCategory category in config.Categories)
            {
                ZooGroup group = FindGroup(groups, category);
                if (group == null)
                    continue;

                if (!TryArrangeGroup(group, category, config.Layout, layout, alignment))
                    continue;

                visible.Add(group);
                blocks.Add(group.Size);
            }

            LayoutResult zoo = ZooGroupLayout.Arrange(blocks, config.Layout.Groups, config.Layout.CategorySpacing);

            for (int i = 0; i < visible.Count; i++)
                visible[i].transform.localPosition = ToFloor(zoo.Positions[i]);

            ArrangeFloor(root, zoo.Size, show);

            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
            SceneView.RepaintAll();
        }

        /// <summary>
        /// Removes the built zoo.
        /// </summary>
        public void Clear()
        {
            ZooRootMarker root = ZooSceneIndex.Root;

            if (root == null)
                return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();

            Undo.DestroyObjectImmediate(root.gameObject);
            ZooSceneIndex.Invalidate();

            Undo.SetCurrentGroupName(ClearUndoLabel);
            Undo.CollapseUndoOperations(undoGroup);
        }

        // The zoo grows from its corner toward -X and -Z. The art faces +Z, so someone looking at its front sees
        // the first asset on the left and the first row closest to them, and reads the zoo like a page.
        private static Vector3 ToFloor(Vector2 position) => new(-position.x, 0f, -position.y);

        private static List<ZooGroup> CollectGroups(ZooRootMarker root)
        {
            List<ZooGroup> groups = new();

            foreach (Transform child in root.transform)
            {
                if (child.TryGetComponent(out ZooGroup group))
                    groups.Add(group);
            }

            return groups;
        }

        private static ZooGroup FindGroup(List<ZooGroup> groups, ZooCategory category)
        {
            string name = GetGroupName(category);

            foreach (ZooGroup group in groups)
            {
                if (group.GroupName == name)
                    return group;
            }

            return null;
        }

        private static string GetGroupName(ZooCategory category) => string.IsNullOrWhiteSpace(category.Name)
            ? UnnamedGroup
            : category.Name;

        private static bool TryArrangeGroup(ZooGroup group, ZooCategory category, LayoutSettings settings,
            ILayoutStrategy layout, IAlignmentStrategy alignment)
        {
            ZooItem[] all = group.GetComponentsInChildren<ZooItem>(true);
            List<ZooItem> visible = new();
            int assetCount = 0;

            foreach (ZooItem item in all)
            {
                if (item.IsFigure)
                    continue;

                bool isShown = !category.IsHidden && !category.IsEntryHidden(item.Asset);
                item.gameObject.SetActive(isShown);

                if (!isShown)
                    continue;

                visible.Add(item);
                assetCount++;
            }

            bool hasAssets = assetCount > 0;
            group.gameObject.SetActive(hasAssets);
            group.SetLayout(Vector2.zero, assetCount);

            if (!hasAssets)
                return false;

            List<Vector2> footprints = new();

            foreach (ZooItem item in visible)
                footprints.Add(alignment.GetFootprint(item.MeasuredBounds));

            SortItems(visible, footprints, settings.Order);
            InsertFigure(all, visible, footprints, alignment);

            LayoutResult result = layout.Layout(footprints, settings);

            for (int i = 0; i < visible.Count; i++)
            {
                ZooItem item = visible[i];
                item.transform.localPosition = ToFloor(result.Positions[i]);
                item.SetPlacement(alignment.GetOffset(item.MeasuredBounds), footprints[i]);
            }

            group.SetLayout(result.Size, assetCount);

            return true;
        }

        private static void SortItems(List<ZooItem> items, List<Vector2> footprints, EItemOrder order)
        {
            int[] indices = new int[items.Count];

            for (int i = 0; i < indices.Length; i++)
                indices[i] = i;

            if (order == EItemOrder.AsListed)
            {
                System.Array.Sort(indices, (a, b) => items[a].ListIndex.CompareTo(items[b].ListIndex));
            }
            else
            {
                // Ties keep the list order, so two chairs of one size do not trade places between builds.
                System.Array.Sort(indices, (a, b) =>
                {
                    int bySize = (footprints[a].x * footprints[a].y).CompareTo(footprints[b].x * footprints[b].y);

                    return bySize != 0
                        ? bySize
                        : items[a].ListIndex.CompareTo(items[b].ListIndex);
                });
            }

            ZooItem[] sortedItems = new ZooItem[items.Count];
            Vector2[] sortedFootprints = new Vector2[items.Count];

            for (int i = 0; i < indices.Length; i++)
            {
                sortedItems[i] = items[indices[i]];
                sortedFootprints[i] = footprints[indices[i]];
            }

            items.Clear();
            items.AddRange(sortedItems);
            footprints.Clear();
            footprints.AddRange(sortedFootprints);
        }

        // The figure always starts the group, so a reader knows where to find it in every block.
        private static void InsertFigure(ZooItem[] all, List<ZooItem> visible, List<Vector2> footprints,
            IAlignmentStrategy alignment)
        {
            foreach (ZooItem item in all)
            {
                if (!item.IsFigure)
                    continue;

                visible.Insert(0, item);
                footprints.Insert(0, alignment.GetFootprint(item.MeasuredBounds));

                return;
            }
        }

        private static void ArrangeFloor(ZooRootMarker root, Vector2 size, ShowSettings show)
        {
            Transform floor = root.Floor;

            if (!show.HasFloor)
            {
                if (floor != null)
                    floor.gameObject.SetActive(false);

                return;
            }

            if (floor == null)
            {
                floor = CreateFloor(root.transform);
                root.SetFloor(floor);
            }

            floor.gameObject.SetActive(true);
            floor.GetComponent<MeshRenderer>().sharedMaterial = GetSurfaceMaterial(show);

            Vector2 floorSize = size + Vector2.one * (FloorMargin * 2f);

            floor.localPosition = new Vector3(-size.x * 0.5f, 0f, -size.y * 0.5f);
            floor.localScale = new Vector3(floorSize.x / FloorUnitsPerScale, 1f, floorSize.y / FloorUnitsPerScale);
        }

        private static Transform CreateFloor(Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = FloorName;
            floor.transform.SetParent(parent, false);

            Object.DestroyImmediate(floor.GetComponent<Collider>());

            return floor.transform;
        }

        private static Material GetSurfaceMaterial(ShowSettings show)
        {
            if (show.FloorMaterial != null)
                return show.FloorMaterial;

            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;

            return pipeline != null
                ? pipeline.defaultMaterial
                : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
        }

        private static void BuildGroups(ZooConfig config, Transform root, HashSet<GameObject> isNew)
        {
            int total = 0;

            foreach (ZooCategory category in config.Categories)
                total += category?.Entries?.Count ?? 0;

            int done = 0;
            bool hasProgressBar = total >= ProgressBarFrom;

            try
            {
                foreach (ZooCategory category in config.Categories)
                {
                    if (category?.Entries == null
                        || category.Entries.Count == 0)
                        continue;

                    GameObject groupObject = new(GetGroupName(category));
                    groupObject.transform.SetParent(root, false);

                    ZooGroup group = groupObject.AddComponent<ZooGroup>();
                    group.Setup(GetGroupName(category), category.LabelColor);

                    BuildFigure(config.Show, groupObject.transform);

                    for (int i = 0; i < category.Entries.Count; i++)
                    {
                        GameObject asset = category.Entries[i];

                        if (hasProgressBar)
                            EditorUtility.DisplayProgressBar(ProgressTitle, category.Name, done / (float)total);

                        done++;

                        if (asset == null
                            || !ZooVisuals.HasVisuals(asset))
                            continue;

                        BuildItem(asset, groupObject.transform, config.Show, i, false, isNew.Contains(asset));
                    }
                }
            }
            finally
            {
                if (hasProgressBar)
                    EditorUtility.ClearProgressBar();
            }
        }

        private static void BuildFigure(ShowSettings show, Transform group)
        {
            if (show.ScaleFigure != null)
                BuildItem(show.ScaleFigure, group, show, -1, true, false);
        }

        private static void BuildItem(GameObject asset, Transform group, ShowSettings show, int index,
            bool isFigure, bool isNew)
        {
            GameObject slot = new(asset.name);
            slot.transform.SetParent(group, false);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, slot.transform);
            if (instance == null)
            {
                Object.DestroyImmediate(slot);
                return;
            }

            instance.transform.localPosition = Vector3.zero;

            bool isDecal = ZooVisuals.TryGetDecal(instance, out Behaviour decal);

            if (isDecal)
                AddDecalBoard(instance, decal, show);

            ZooMeasurement measurement = ZooVisuals.Measure(instance, slot.transform);

            ZooItem item = slot.AddComponent<ZooItem>();

            // A board is there to be projected on and is no part of the asset, so its numbers do not count.
            item.SetMeasurement(asset, instance.transform, measurement.Bounds,
                isDecal
                    ? 0
                    : measurement.Triangles,
                isDecal
                    ? 0
                    : measurement.Materials,
                isDecal || isFigure
                    ? string.Empty
                    : measurement.Problem);

            item.SetRole(index, isFigure, isNew);
        }

        // A decal shows nothing without something to land on, so each one brings its own little wall. It is turned
        // around too: decals project along their forward axis, and the zoo is looked at from the other side.
        private static void AddDecalBoard(GameObject instance, Behaviour decal, ShowSettings show)
        {
            instance.transform.localRotation = Quaternion.Euler(0f, DecalTurn, 0f) * instance.transform.localRotation;

            ZooVisuals.GetDecalBox(decal, out Vector3 size, out Vector3 offset);

            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = BoardName;

            Object.DestroyImmediate(board.GetComponent<Collider>());

            board.GetComponent<MeshRenderer>().sharedMaterial = GetSurfaceMaterial(show);

            Transform boardTransform = board.transform;
            boardTransform.SetParent(decal.transform, false);
            boardTransform.localPosition = offset;
            boardTransform.localRotation = Quaternion.identity;
            boardTransform.localScale = new Vector3(size.x * BoardPadding, size.y * BoardPadding, BoardDepth);
        }
    }
}