using Base.ToolPackage.AssetZoo;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Builder
{
    /// <summary>
    /// Turns every <see cref="ZooLabelBillboard"/> toward the scene view camera while the editor is
    /// not playing. Living in the editor assembly keeps the scene view hook out of the runtime
    /// component, so no <c>UNITY_EDITOR</c> guard is needed there.
    /// <para>
    /// <see cref="InitializeOnLoadAttribute"/> means this runs in every project that installs the
    /// Tools package, and almost none of them hold a zoo. So the scene view hook is only attached
    /// while there is something to turn, and drops itself again the moment a rebuild comes back
    /// empty. The hierarchy hook stays, because a label can arrive at any time and setting a flag
    /// costs nothing next to being called on every repaint.
    /// </para>
    /// </summary>
    [InitializeOnLoad]
    internal static class ZooLabelBillboardDriver
    {
        private static ZooLabelBillboard[] _billboards;
        private static bool _isAttached;
        private static bool _isDirty = true;

        static ZooLabelBillboardDriver()
        {
            EditorApplication.hierarchyChanged += Invalidate;

            Attach();
        }

        // The driver re-attaches from more than one place, and subscribing twice would turn every
        // label twice per repaint, so the flag rather than the delegate is what decides.
        private static void Attach()
        {
            if (_isAttached)
                return;

            SceneView.duringSceneGui += HandleSceneGui;
            _isAttached = true;
        }

        private static void Detach()
        {
            if (!_isAttached)
                return;

            SceneView.duringSceneGui -= HandleSceneGui;
            _isAttached = false;
        }

        // A label can appear through the zoo builder, an undo, or a scene being opened, and all
        // three show up here. So this is also where a driver that dropped itself comes back.
        private static void Invalidate()
        {
            _isDirty = true;

            Attach();
        }

        private static void HandleSceneGui(SceneView sceneView)
        {
            if (Application.isPlaying)
                return;

            if (_isDirty
                && !TryRebuild())
                return;

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

        // Returns whether the scene holds anything to turn. Coming back empty is the normal case in
        // a project that has no zoo, and the hook goes with it rather than scanning on every change.
        private static bool TryRebuild()
        {
            _billboards = Object.FindObjectsByType<ZooLabelBillboard>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            _isDirty = false;

            if (_billboards.Length > 0)
                return true;

            Detach();

            return false;
        }
    }
}