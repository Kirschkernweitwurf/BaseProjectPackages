using UnityEngine;
using UnityEngine.UI;

namespace Base.UIPackage.Effects
{
    /// <summary>
    /// Bends the mesh of an image or other graphic into the tilt of the <see cref="UIPerspective"/> above it and
    /// hides it while its side faces away. The perspective adds it to every graphic on its own; add it by hand only
    /// to mark a back side, like the back of a card.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIPerspectiveGraphic : BaseMeshEffect
    {
        [Tooltip("Whether this graphic is on the back, so it draws only while the UI is turned around.")]
        [SerializeField] private bool isBackSide;

        private UIPerspective _perspective;

        /// <summary>Rebuilds the mesh with the current tilt.</summary>
        public void Refresh() => graphic.SetVerticesDirty();

        /// <inheritdoc/>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            if (_perspective == null)
                _perspective = GetComponentInParent<UIPerspective>(includeInactive: true);

            if (_perspective == null)
                return;

            if (!_perspective.IsSideShown(isBackSide))
            {
                vh.Clear();
                return;
            }

            if (!isBackSide
                && _perspective.IsFlat)
                return;

            UIPerspectiveProjection projection = _perspective.GetProjection(transform, isBackSide);
            UIVertex vertex = default;

            for (int index = 0; index < vh.currentVertCount; index++)
            {
                vh.PopulateUIVertex(ref vertex, index);
                vertex.position = projection.Project(vertex.position);
                vh.SetUIVertex(vertex, index);
            }
        }
    }
}