using System.Collections.Generic;
using UnityEngine;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// Draws queued line segments with GL, one pass per depth mode. Owns the two materials the
    /// passes need and releases them on <see cref="Dispose"/>.
    /// </summary>
    internal sealed class DebugLinePainter
    {
        private const int MaterialPass = 0;

        private Material _depthTestedMaterial;
        private Material _overlayMaterial;
        private bool _hasCheckedMaterials;

        /// <summary>
        /// Draws both passes through the given camera. Does nothing when there is nothing queued
        /// or the materials could not be created.
        /// </summary>
        /// <param name="renderingCamera">The camera whose matrices the segments are drawn with.</param>
        /// <param name="depthTestedLines">The segments geometry in front of them hides.</param>
        /// <param name="overlayLines">The segments drawn on top of everything.</param>
        internal void Draw(Camera renderingCamera, IReadOnlyList<DebugLineCommand> depthTestedLines,
            IReadOnlyList<DebugLineCommand> overlayLines)
        {
            if (depthTestedLines.Count == 0
                && overlayLines.Count == 0)
                return;

            if (!TryEnsureMaterials())
                return;

            GL.PushMatrix();

            // Set explicitly rather than relying on whatever is current: under a scriptable render
            // pipeline this runs outside the camera's own matrix setup.
            GL.LoadProjectionMatrix(renderingCamera.projectionMatrix);
            GL.modelview = renderingCamera.worldToCameraMatrix;

            DrawPass(_depthTestedMaterial, depthTestedLines);
            DrawPass(_overlayMaterial, overlayLines);

            GL.PopMatrix();
        }

        /// <summary>Destroys the generated materials. Call when the owner goes away.</summary>
        internal void Dispose()
        {
            if (_depthTestedMaterial != null)
                Object.Destroy(_depthTestedMaterial);

            if (_overlayMaterial != null)
                Object.Destroy(_overlayMaterial);

            _depthTestedMaterial = null;
            _overlayMaterial = null;
        }

        private static void DrawPass(Material material, IReadOnlyList<DebugLineCommand> lines)
        {
            if (lines.Count == 0)
                return;

            material.SetPass(MaterialPass);

            GL.Begin(GL.LINES);

            // Indexed instead of a foreach: iterating the interface boxes an enumerator, and this
            // runs once per pass per camera per frame.
            for (int i = 0; i < lines.Count; i++)
            {
                DebugLineCommand line = lines[i];

                GL.Color(line.Color);
                GL.Vertex(line.From);
                GL.Vertex(line.To);
            }

            GL.End();
        }

        // Both materials come from the same shader, so either both exist or neither does. A failed
        // lookup is not retried, because it fails for the whole session and logs when it does.
        private bool TryEnsureMaterials()
        {
            if (_hasCheckedMaterials)
                return _depthTestedMaterial != null
                    && _overlayMaterial != null;

            _hasCheckedMaterials = true;

            return DebugDrawMaterialFactory.TryCreate(true, out _depthTestedMaterial)
                && DebugDrawMaterialFactory.TryCreate(false, out _overlayMaterial);
        }
    }
}