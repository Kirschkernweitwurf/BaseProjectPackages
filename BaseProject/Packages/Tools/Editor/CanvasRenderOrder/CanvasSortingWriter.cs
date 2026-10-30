using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.CanvasRenderOrder
{
    /// <summary>
    /// Writes a new sorting layer and order back to the canvas an entry stands for.
    /// </summary>
    internal static class CanvasSortingWriter
    {
        private const string UndoName = "Change Canvas Sorting";

        /// <summary>
        /// Writes the sorting. Scene canvases go through Undo and become instance overrides where they belong to
        /// a prefab; prefab canvases are written into the asset, variants keep it as their own override.
        /// </summary>
        /// <returns>Whether the canvas now has the new values.</returns>
        internal static bool TryWrite(CanvasEntry entry, int sortingLayerId, int sortingOrder)
        {
            if (entry == null)
            {
                CustomLogger.LogError("No canvas entry to write to.", null);
                return false;
            }

            return entry.Source == ECanvasSource.Scene
                ? TryWriteToScene(entry.Canvas, sortingLayerId, sortingOrder)
                : TryWriteToPrefab(entry, sortingLayerId, sortingOrder);
        }

        private static bool TryWriteToScene(Canvas canvas, int sortingLayerId, int sortingOrder)
        {
            // Destroyed since the last scan, for example by leaving play mode.
            if (canvas == null)
                return false;

            Undo.RecordObject(canvas, UndoName);
            Apply(canvas, sortingLayerId, sortingOrder);
            PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);

            return true;
        }

        // Prefab contents instead of the loaded asset, so variants and nested prefabs save their override the same
        // way the Prefab Mode does.
        private static bool TryWriteToPrefab(CanvasEntry entry, int sortingLayerId, int sortingOrder)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(entry.AssetPath);

            try
            {
                Transform target = contents.transform;

                foreach (int index in entry.SiblingPath)
                {
                    if (index >= target.childCount)
                        return Fail(entry);

                    target = target.GetChild(index);
                }

                if (!target.TryGetComponent(out Canvas canvas))
                    return Fail(entry);

                Apply(canvas, sortingLayerId, sortingOrder);
                PrefabUtility.SaveAsPrefabAsset(contents, entry.AssetPath, out bool isSaved);

                return isSaved;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void Apply(Canvas canvas, int sortingLayerId, int sortingOrder)
        {
            canvas.sortingLayerID = sortingLayerId;
            canvas.sortingOrder = sortingOrder;
        }

        private static bool Fail(CanvasEntry entry)
        {
            CustomLogger.LogWarning($"The canvas {entry.Name} moved in {entry.AssetPath}. Refresh and try again.",
                null);

            return false;
        }
    }
}