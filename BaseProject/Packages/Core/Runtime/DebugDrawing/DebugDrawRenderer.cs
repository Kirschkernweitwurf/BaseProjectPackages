using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// Hosts debug drawing in a running player: expires what is queued in
    /// <see cref="DebugDrawBuffer"/>, hands the line segments to <see cref="DebugLinePainter"/>
    /// after each game and scene view camera, and the labels to <see cref="DebugLabelPainter"/>.
    /// Both the built-in pipeline and a scriptable one are covered, through
    /// <see cref="Camera.onPostRender"/> and <see cref="RenderPipelineManager.endCameraRendering"/>.
    /// </summary>
    /// <remarks>
    /// Creates itself at startup, but only in the editor and in development builds. Define
    /// <c>BASE_DEBUG_DRAW</c> to get it in a release build too.
    /// </remarks>
    [DefaultExecutionOrder(ExecutionOrder)]
    internal sealed class DebugDrawRenderer : MonoBehaviour
    {
        // Runs before any game code, so the previous frame's commands are gone before new ones arrive.
        private const int ExecutionOrder = -10000;

        private readonly DebugLinePainter _linePainter = new();
        private readonly DebugLabelPainter _labelPainter = new();

#region Unity Callbacks
        private void OnEnable()
        {
            Camera.onPostRender += Render;
            RenderPipelineManager.endCameraRendering += Render;
        }

        private void Update() => DebugDrawBuffer.Prune();

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            IReadOnlyList<DebugLabelCommand> labels = DebugDrawBuffer.LabelCommands;

            if (labels.Count == 0)
                return;

            Camera labelCamera = Camera.main;

            if (labelCamera == null)
                return;

            _labelPainter.Draw(labelCamera, labels);
        }

        private void OnDisable()
        {
            Camera.onPostRender -= Render;
            RenderPipelineManager.endCameraRendering -= Render;
        }

        private void OnDestroy() => _linePainter.Dispose();
#endregion

#if UNITY_EDITOR || DEVELOPMENT_BUILD || BASE_DEBUG_DRAW
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            GameObject host = new(nameof(DebugDrawRenderer));

            host.AddComponent<DebugDrawRenderer>();

            DontDestroyOnLoad(host);
        }
#endif

        private void Render(ScriptableRenderContext context, Camera renderingCamera) => Render(renderingCamera);

        private void Render(Camera renderingCamera)
        {
            // Preview and reflection cameras would draw the same lines again into targets nobody
            // is looking at.
            if (renderingCamera.cameraType != CameraType.Game
                && renderingCamera.cameraType != CameraType.SceneView)
                return;

            _linePainter.Draw(renderingCamera, DebugDrawBuffer.DepthTestedLineCommands,
                DebugDrawBuffer.OverlayLineCommands);
        }
    }
}