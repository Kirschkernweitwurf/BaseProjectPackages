using UnityEngine;

namespace Base.UIPackage.Effects
{
    /// <summary>
    /// Turns the vertices of one graphic under a <see cref="UIPerspective"/> in 3D around the root's center and
    /// projects them back onto the flat canvas, so a near edge grows and a far edge shrinks like on a real card. A
    /// graphic whose z sits behind the plane (positive) or in front of it (negative) moves against the rest while the
    /// UI tilts, like a parallax layer.
    /// </summary>
    public readonly struct UIPerspectiveProjection
    {
        private readonly Matrix4x4 _toRoot;
        private readonly Matrix4x4 _fromRoot;
        private readonly Quaternion _rotation;
        private readonly Vector3 _center;
        private readonly float _distance;

        /// <summary>
        /// Projects the vertices of <paramref name="source"/> into the <paramref name="root"/>'s turned plane.
        /// <paramref name="distance"/> is how far the eye sits in front of the root, in the root's units.
        /// </summary>
        public UIPerspectiveProjection(RectTransform root, Transform source, Quaternion rotation, float distance)
        {
            _toRoot = root.worldToLocalMatrix * source.localToWorldMatrix;
            _fromRoot = _toRoot.inverse;
            _rotation = rotation;
            _center = root.rect.center;
            _distance = distance;
        }

        /// <summary>
        /// Returns where <paramref name="vertex"/>, in the source's local space, shows on the canvas.
        /// </summary>
        public Vector3 Project(Vector3 vertex)
        {
            Vector3 local = _toRoot.MultiplyPoint3x4(vertex) - _center;

            // A layer off the plane is spread to its depth, so it shows at its laid out size while flat (the flat
            // UI skips this projection) and only shifts against the plane once it tilts.
            float spread = (_distance + local.z) / _distance;
            Vector3 placed = new(local.x * spread, local.y * spread, local.z);
            Vector3 turned = _rotation * placed;

            // The canvas looks along +z, so a point turned toward the eye has a negative z and grows.
            float scale = _distance / Mathf.Max(_distance + turned.z, Mathf.Epsilon);
            Vector3 flat = new(turned.x * scale + _center.x, turned.y * scale + _center.y, local.z);

            return _fromRoot.MultiplyPoint3x4(flat);
        }
    }
}