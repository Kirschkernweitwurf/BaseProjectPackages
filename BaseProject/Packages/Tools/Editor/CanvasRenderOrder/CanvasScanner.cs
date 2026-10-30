using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.CanvasRenderOrder
{
    /// <summary>
    /// Finds the canvases that sort on their own in the open scenes and in the project's prefabs.
    /// </summary>
    internal static class CanvasScanner
    {
        private const string AssetsFolder = "Assets";
        private const string CameraLabel = "Screen Space Camera: ";
        private const string NestedPrefix = "Nested in ";
        private const string NestedSeparator = ", ";
        private const string NoCameraLabel = "Screen Space Camera, no camera: drawn as overlay";
        private const string OverlayLabel = "Screen Space Overlay";
        private const string PrefabFilter = "t:Prefab";
        private const string PrefabModeSuffix = " (Prefab Mode)";
        private const string ProgressTitle = "Canvas Render Order";
        private const string WorldLabel = "World Space";

        /// <summary>
        /// Collects the canvases of every loaded scene, including DontDestroyOnLoad in play mode, and of the
        /// prefab open in Prefab Mode.
        /// </summary>
        internal static List<CanvasEntry> ScanScenes()
        {
            List<CanvasEntry> entries = new();
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();

            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (Canvas canvas in canvases)
            {
                if (IsHidden(canvas)
                    || (stage != null && canvas.gameObject.scene == stage.scene))
                    continue;

                TryAdd(entries, canvas, ECanvasSource.Scene, null, null, canvas.gameObject.scene.name);
            }

            if (stage == null)
                return entries;

            string location = Path.GetFileNameWithoutExtension(stage.assetPath) + PrefabModeSuffix;

            foreach (Canvas canvas in stage.prefabContentsRoot.GetComponentsInChildren<Canvas>(true))
                TryAdd(entries, canvas, ECanvasSource.Scene, null, null, location);

            return entries;
        }

        /// <summary>
        /// Collects the canvases of every prefab under Assets. Package prefabs are skipped, they are read only.
        /// A canvas that comes from a nested prefab or a variant base is only listed where its sorting changes.
        /// </summary>
        internal static List<CanvasEntry> ScanPrefabs()
        {
            List<CanvasEntry> entries = new();
            string[] guids = AssetDatabase.FindAssets(PrefabFilter, new[] { AssetsFolder });

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    EditorUtility.DisplayProgressBar(ProgressTitle, path, (float)i / guids.Length);
                    AddPrefab(entries, path);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return entries;
        }

        private static void AddPrefab(List<CanvasEntry> entries, string path)
        {
            GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (root == null)
                return;

            string location = Path.GetFileNameWithoutExtension(path);

            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                if (!IsAuthoredHere(canvas))
                    continue;

                TryAdd(entries, canvas, ECanvasSource.Prefab, path, GetSiblingPath(root.transform, canvas.transform),
                    location);
            }
        }

        private static bool IsHidden(Canvas canvas) => (canvas.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0;

        // Without this every canvas of a nested prefab or variant base shows up once per prefab that contains it.
        private static bool IsAuthoredHere(Canvas canvas)
        {
            Canvas source = PrefabUtility.GetCorrespondingObjectFromSource(canvas);

            if (source == null)
                return true;

            return source.sortingLayerID != canvas.sortingLayerID
                || source.sortingOrder != canvas.sortingOrder
                || source.overrideSorting != canvas.overrideSorting;
        }

        private static void TryAdd(List<CanvasEntry> entries, Canvas canvas, ECanvasSource source, string assetPath,
            int[] siblingPath, string location)
        {
            Canvas root = FindRootCanvas(canvas);
            bool isRoot = root == canvas;

            // A nested canvas without Override Sorting draws as part of its parent.
            if (!isRoot && !canvas.overrideSorting)
                return;

            ECanvasPass pass = root.renderMode == RenderMode.ScreenSpaceOverlay
                ? ECanvasPass.Overlay
                : ECanvasPass.Camera;

            string detail = isRoot
                ? Describe(root)
                : NestedPrefix + root.name + NestedSeparator + Describe(root);

            Object identity = PrefabUtility.GetCorrespondingObjectFromOriginalSource(canvas);

            if (identity == null)
                identity = canvas;

            entries.Add(new CanvasEntry(canvas, identity, source, assetPath, siblingPath, location, detail, pass,
                root.targetDisplay));
        }

        private static Canvas FindRootCanvas(Canvas canvas)
        {
            Canvas root = canvas;

            while (root.transform.parent != null)
            {
                Canvas parent = root.transform.parent.GetComponentInParent<Canvas>(true);

                if (parent == null)
                    break;

                root = parent;
            }

            return root;
        }

        private static string Describe(Canvas root)
        {
            switch (root.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    return OverlayLabel;
                case RenderMode.WorldSpace:
                    return WorldLabel;
                default:
                    return root.worldCamera == null
                        ? NoCameraLabel
                        : CameraLabel + root.worldCamera.name;
            }
        }

        private static int[] GetSiblingPath(Transform root, Transform target)
        {
            List<int> path = new();

            for (Transform current = target; current != root; current = current.parent)
                path.Add(current.GetSiblingIndex());

            path.Reverse();
            return path.ToArray();
        }
    }
}