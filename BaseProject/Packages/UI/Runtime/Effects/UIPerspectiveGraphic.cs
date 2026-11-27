using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Base.UIPackage.Effects
{
    /// <summary>
    /// Bends the mesh of an image or other graphic into the tilt of the <see cref="UIPerspective"/> above it and
    /// hides it while its side faces away. The perspective adds it to every graphic on its own; add it by hand only
    /// to mark a back side, like the back of a card. Big triangles are split into smaller ones before they bend: the
    /// GPU spreads a picture evenly between a triangle's corners, not with the perspective, so a card drawn as one quad
    /// showed its rounded corners several pixels off and kinked along the quad's diagonal.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIPerspectiveGraphic : BaseMeshEffect
    {
        private const int MaxSplits = 8;

        // Longest edge of a piece as a share of the eye's distance. A card split this fine lands within a third of a
        // pixel of the true perspective at a strong tilt; as a single quad it was up to 9 px off.
        private const float PieceShare = 1f / 18f;

        private const int TriangleCorners = 3;

        private readonly List<UIVertex> _bent = new();
        private readonly List<UIVertex> _flat = new();

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
            vh.GetUIVertexStream(_flat);
            _bent.Clear();

            for (int index = 0; index + TriangleCorners <= _flat.Count; index += TriangleCorners)
                AddBent(projection, _flat[index], _flat[index + 1], _flat[index + 2]);

            vh.Clear();
            vh.AddUIVertexTriangleStream(_bent);
        }

        private static int GetSplits(in UIPerspectiveProjection projection, Vector3 a, Vector3 b, Vector3 c)
        {
            float longest = Mathf.Max(projection.GetDistanceShare(a, b),
                Mathf.Max(projection.GetDistanceShare(b, c), projection.GetDistanceShare(c, a)));

            return Mathf.Clamp(Mathf.CeilToInt(longest / PieceShare), 1, MaxSplits);
        }

        private static UIVertex GetBent(in UIPerspectiveProjection projection, UIVertex a, UIVertex b, UIVertex c,
            float towardB, float towardC)
        {
            UIVertex vertex = Blend(a, b, c, towardB, towardC);
            vertex.position = projection.Project(vertex.position);
            return vertex;
        }

        // The flat triangle spreads its picture evenly, so a plain mix of its corners is exact before the bend.
        private static UIVertex Blend(UIVertex a, UIVertex b, UIVertex c, float towardB, float towardC)
        {
            float towardA = 1f - towardB - towardC;

            UIVertex vertex = a;
            vertex.position = a.position * towardA + b.position * towardB + c.position * towardC;
            vertex.normal = a.normal * towardA + b.normal * towardB + c.normal * towardC;
            vertex.tangent = a.tangent * towardA + b.tangent * towardB + c.tangent * towardC;
            vertex.color = (Color)a.color * towardA + (Color)b.color * towardB + (Color)c.color * towardC;
            vertex.uv0 = a.uv0 * towardA + b.uv0 * towardB + c.uv0 * towardC;
            vertex.uv1 = a.uv1 * towardA + b.uv1 * towardB + c.uv1 * towardC;
            vertex.uv2 = a.uv2 * towardA + b.uv2 * towardB + c.uv2 * towardC;
            vertex.uv3 = a.uv3 * towardA + b.uv3 * towardB + c.uv3 * towardC;
            return vertex;
        }

        // Splits the triangle into a grid of smaller ones with the same winding and bends each of their corners.
        private void AddBent(in UIPerspectiveProjection projection, UIVertex a, UIVertex b, UIVertex c)
        {
            int splits = GetSplits(projection, a.position, b.position, c.position);
            float step = 1f / splits;

            for (int row = 0; row < splits; row++)
            {
                for (int column = 0; column < splits - row; column++)
                {
                    _bent.Add(GetBent(projection, a, b, c, row * step, column * step));
                    _bent.Add(GetBent(projection, a, b, c, (row + 1) * step, column * step));
                    _bent.Add(GetBent(projection, a, b, c, row * step, (column + 1) * step));

                    // The piece between two neighbors points the other way and only fits away from the third edge.
                    if (row + column + 1 >= splits)
                        continue;

                    _bent.Add(GetBent(projection, a, b, c, (row + 1) * step, column * step));
                    _bent.Add(GetBent(projection, a, b, c, (row + 1) * step, (column + 1) * step));
                    _bent.Add(GetBent(projection, a, b, c, row * step, (column + 1) * step));
                }
            }
        }
    }
}