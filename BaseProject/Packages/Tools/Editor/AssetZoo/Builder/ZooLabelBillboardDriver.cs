using Base.ToolPackage.AssetZoo;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Builder
{
    /// <summary>
    /// Turns every <see cref="ZooLabelBillboard"/> toward the scene view camera while the editor is
    /// not playing. Living in the editor assembly keeps the scene view hook out of the runtime
    /// component, so no <c>UNITY_EDITOR</c> guard is needed there.
    /// </summary>
    [InitializeOnLoad]
    internal static class ZooLabelBillboardDriver
    {
        private static ZooLabelBillboard[] _billboards;
        private static bool _isDirty = true;

        static ZooLabelBillboardDriver()
        {
            SceneView.duringSceneGui += HandleSceneGui;
            EditorApplication.hierarchyChanged += Invalidate;
        }

        private static void Invalidate() => _isDirty = true;

        private static void HandleSceneGui(SceneView sceneView)
        {
            if (Application.isPlaying)
                return;

            if (_isDirty)
                Rebuild();

            foreach (ZooLabelBillboard billboard in _billboards)
            {
                // A label deleted since the last rebuild leaves a destroyed entry behind
                if (billboard == null)
                {
                    Invalidate();
                    continue;
                }

                billboard.FaceCamera(sceneView.camera);
            }
        }

        private static void Rebuild()
        {
            _billboards = Object.FindObjectsByType<ZooLabelBillboard>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            _isDirty = false;
        }
    }
}