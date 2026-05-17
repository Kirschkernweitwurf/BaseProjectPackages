using System.Collections.Generic;
using Base.ToolPackage.Editor.AssetZoo.Runtime.Alignment;
using Base.ToolPackage.Editor.AssetZoo.Runtime.Config;
using Base.ToolPackage.Editor.AssetZoo.Runtime.Layout;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Builder
{
    /// <summary>
    /// Builds an asset zoo into the active scene. Pure logic, knows nothing about the
    /// editor UI. Call <see cref="Build"/> to generate, <see cref="Clear"/> to remove.
    /// </summary>
    public class ZooBuilder
    {
        private const string ZooRootName = "AssetZoo_Generated";
        private const string BuildUndoLabel = "Build Asset Zoo";
        private const string ClearUndoLabel = "Clear Asset Zoo";
        private const string UnnamedCategoryFallback = "Unnamed Category";

        // Cached reference to the most recently built zoo root. Lost on editor reload
        // or when a new builder instance is constructed; GetZooRoot() repopulates via
        // marker lookup in those cases.
        private GameObject _cachedRoot;

        /// <summary>
        /// True when a built zoo exists in the scene that this builder can act on.
        /// </summary>
        public bool HasZoo => GetZooRoot() != null;

        /// <summary>
        /// Returns the current zoo root, or null if none exists. Uses an internal cache
        /// for the fast path; falls back to a type-safe marker scan when the cache is empty.
        /// </summary>
        public GameObject GetZooRoot()
        {
            if (_cachedRoot != null)
                return _cachedRoot;

            ZooRootMarker marker = Object.FindAnyObjectByType<ZooRootMarker>();
            _cachedRoot = marker != null ? marker.gameObject : null;
            return _cachedRoot;
        }

        /// <summary>
        /// Builds the zoo. Replaces any previously built zoo under the same parent.
        /// </summary>
        public void Build(ZooConfig config, Transform parent = null)
        {
            if (config == null)
            {
                CustomLogger.LogError("No config provided.", null);
                return;
            }

#if UNITY_EDITOR
            bool useUndo = !Application.isPlaying;
            int undoGroup = 0;
            if (useUndo)
            {
                Undo.IncrementCurrentGroup();
                undoGroup = Undo.GetCurrentGroup();
            }
#endif

            ClearExisting();

            GameObject root = new(ZooRootName);
            if (parent != null)
                root.transform.SetParent(parent, false);

            root.AddComponent<ZooRootMarker>();
            RegisterTracked(root, BuildUndoLabel);
            _cachedRoot = root;

            ILayoutStrategy layoutStrategy = LayoutStrategyFactory.Create(config.Layout.Type);
            IAlignmentStrategy alignment = AlignmentStrategyFactory.Create(config.Layout.Alignment);

            float categoryOffsetZ = 0f;
            Vector3 direction = GetDirectionVector(config.Layout.CategoryDirection);

            foreach (ZooCategory category in config.Categories)
            {
                if (category?.Entries == null || category.Entries.Count == 0)
                    continue;

                BuildCategory(config, category, root.transform, layoutStrategy, alignment, direction, ref categoryOffsetZ);
            }

#if UNITY_EDITOR
            if (useUndo)
            {
                Undo.SetCurrentGroupName(BuildUndoLabel);
                Undo.CollapseUndoOperations(undoGroup);
            }
#endif
        }

        /// <summary>
        /// Clears the previously built zoo under the given parent (or in the whole scene if parent is null).
        /// </summary>
        public void Clear()
        {
#if UNITY_EDITOR
            bool useUndo = !Application.isPlaying;
            int undoGroup = 0;
            if (useUndo)
            {
                Undo.IncrementCurrentGroup();
                undoGroup = Undo.GetCurrentGroup();
            }
#endif
            ClearExisting();
#if UNITY_EDITOR
            if (useUndo)
            {
                Undo.SetCurrentGroupName(ClearUndoLabel);
                Undo.CollapseUndoOperations(undoGroup);
            }
#endif
        }

        private void ClearExisting()
        {
            GameObject existing = GetZooRoot();
            _cachedRoot = null;

            if (existing == null)
                return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(existing);
                return;
            }
#endif
            Object.Destroy(existing);
        }

        private static void RegisterTracked(GameObject go, string undoLabel)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && go != null)
                Undo.RegisterCreatedObjectUndo(go, undoLabel);
#endif
        }

        private static GameObject CreateTracked(string name, Transform parent, string undoLabel)
        {
            GameObject go = new(name);
            if (parent != null)
                go.transform.SetParent(parent, false);

            RegisterTracked(go, undoLabel);
            return go;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);

                if (instance != null)
                    Undo.RegisterCreatedObjectUndo(instance, BuildUndoLabel);

                return instance;
            }
#endif
            return Object.Instantiate(prefab, parent);
        }

        private static void BuildCategory(ZooConfig config, ZooCategory category, Transform root,
            ILayoutStrategy layoutStrategy, IAlignmentStrategy alignment, Vector3 direction, ref float categoryOffset)
        {
            string categoryName = string.IsNullOrWhiteSpace(category.Name)
                ? UnnamedCategoryFallback
                : category.Name;

            GameObject categoryRoot = CreateTracked(categoryName, root, BuildUndoLabel);
            categoryRoot.transform.localPosition = direction * categoryOffset;

            // Filter valid entries and pre-compute bounds. The biggest one defines the cell.
            List<GameObject> validEntries = new();
            List<Bounds> entryBounds = new();
            Vector3 maxCell = Vector3.zero;

            foreach (GameObject entry in category.Entries)
            {
                if (entry == null)
                    continue;

                Bounds b = BoundsCalculator.CalculatePrefabBounds(entry);
                validEntries.Add(entry);
                entryBounds.Add(b);
                maxCell = Vector3.Max(maxCell, b.size);
            }

            if (validEntries.Count == 0)
                return;

            LayoutResult result = layoutStrategy.Layout(validEntries.Count, maxCell, config.Layout);

            // When stacking in a negative direction, mirror positions along the stacking axis
            Vector3 mirror = new(
                direction.x < 0f ? -1f : 1f,
                direction.y < 0f ? -1f : 1f,
                direction.z < 0f ? -1f : 1f);

            for (int i = 0; i < validEntries.Count; i++)
            {
                Vector3 slot = Vector3.Scale(result.Positions[i], mirror);
                PlaceEntry(i, config, validEntries[i], entryBounds[i],
                    slot, alignment, categoryRoot.transform);
            }

            if (config.Labels.ShowCategoryLabels)
            {
                Vector3[] mirrored = new Vector3[result.Positions.Length];
                for (int i = 0; i < mirrored.Length; i++)
                    mirrored[i] = Vector3.Scale(result.Positions[i], mirror);

                CreateCategoryLabel(config, categoryName, category.LabelColor,
                    mirrored, maxCell, categoryRoot.transform);
            }

            float extent = Mathf.Abs(Vector3.Dot(result.TotalSize, direction));
            categoryOffset += extent + config.Layout.CategorySpacing;
        }

        private static void PlaceEntry(int index, ZooConfig config, GameObject entry, Bounds bounds,
            Vector3 slotPosition, IAlignmentStrategy alignment, Transform parent)
        {
            string entryName = $"{index:D2}_{entry.name}";
            GameObject entryRoot = CreateTracked(entryName, parent, BuildUndoLabel);
            entryRoot.transform.localPosition = slotPosition;

            GameObject instance = InstantiatePrefab(entry, entryRoot.transform);
            if (instance == null)
                return;

            instance.transform.localPosition = alignment.GetOffset(bounds);

            if (!config.Labels.ShowItemLabels)
                return;

            string label = entry.name;
            Vector3 labelLocalPos = new(0f, bounds.size.y + config.Labels.ItemLabelHeight, 0f);
            GameObject labelGo = LabelFactory.CreateLabel(label, entryRoot.transform, labelLocalPos,
                config.Labels.ItemFontSize, config.Labels.ItemColor, LabelSettings.ItemWorldScale);
            RegisterTracked(labelGo, BuildUndoLabel);
        }

        private static void CreateCategoryLabel(ZooConfig config, string categoryName, Color labelColor,
            Vector3[] positions, Vector3 maxCell, Transform parent)
        {
            Vector3 min = positions[0];
            Vector3 max = positions[0];
            for (int i = 1; i < positions.Length; i++)
            {
                min = Vector3.Min(min, positions[i]);
                max = Vector3.Max(max, positions[i]);
            }

            Vector3 center = (min + max) * 0.5f;
            Vector3 pos = new(center.x, maxCell.y + config.Labels.CategoryLabelHeight, center.z);

            int fontSize = Mathf.RoundToInt(config.Labels.CategoryFontSize);

            GameObject labelGo = LabelFactory.CreateLabel(categoryName, parent, pos,
                fontSize, labelColor, LabelSettings.CategoryWorldScale);

            RegisterTracked(labelGo, BuildUndoLabel);
        }

        private static Vector3 GetDirectionVector(ECategoryDirection direction) => direction switch
        {
            ECategoryDirection.Forward => Vector3.forward,
            ECategoryDirection.Backward => Vector3.back,
            ECategoryDirection.Left => Vector3.left,
            ECategoryDirection.Right => Vector3.right,
            _ => Vector3.up
        };
    }
}