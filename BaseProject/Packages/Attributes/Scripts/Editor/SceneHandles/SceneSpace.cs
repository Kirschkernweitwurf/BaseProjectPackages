using UnityEngine;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>
    /// Converts between the space a field stores its value in and the world space the scene view draws
    /// in. A vector on a component almost always means a local offset, so local is the default and this
    /// is where that assumption is applied instead of being repeated in every drawer.
    /// </summary>
    internal static class SceneSpace
    {
        private static readonly Color Fallback = new(0.35f, 0.75f, 1f);

        /// <summary>Converts a stored position into a world position.</summary>
        /// <param name="transform">The transform local space is measured against, may be null.</param>
        /// <param name="value">The stored position.</param>
        /// <param name="space">How the stored value is interpreted.</param>
        /// <returns>The world position.</returns>
        public static Vector3 ToWorld(Transform transform, Vector3 value, ESpace space)
        {
            if (space == ESpace.World || transform == null)
                return value;

            return transform.TransformPoint(value);
        }

        /// <summary>Converts a world position back into the stored representation.</summary>
        /// <param name="transform">The transform local space is measured against, may be null.</param>
        /// <param name="world">The world position.</param>
        /// <param name="space">How the stored value is interpreted.</param>
        /// <returns>The value to store.</returns>
        public static Vector3 ToStored(Transform transform, Vector3 world, ESpace space)
        {
            if (space == ESpace.World || transform == null)
                return world;

            return transform.InverseTransformPoint(world);
        }

        /// <summary>The rotation a gizmo is oriented by in the given space.</summary>
        /// <param name="transform">The transform local space is measured against, may be null.</param>
        /// <param name="space">How the stored value is interpreted.</param>
        /// <returns>The gizmo orientation.</returns>
        public static Quaternion Rotation(Transform transform, ESpace space)
        {
            if (space == ESpace.World || transform == null)
                return Quaternion.identity;

            return transform.rotation;
        }

        /// <summary>The world-space normal of a flat handle oriented around the given axis.</summary>
        /// <param name="transform">The transform local space is measured against, may be null.</param>
        /// <param name="axis">The axis the handle faces along.</param>
        /// <param name="space">How the stored value is interpreted.</param>
        /// <returns>The normal vector.</returns>
        public static Vector3 Normal(Transform transform, ENormalAxis axis, ESpace space)
        {
            if (space == ESpace.World || transform == null)
            {
                return axis switch
                {
                    ENormalAxis.X => Vector3.right,
                    ENormalAxis.Z => Vector3.forward,
                    _ => Vector3.up
                };
            }

            return axis switch
            {
                ENormalAxis.X => transform.right,
                ENormalAxis.Z => transform.forward,
                _ => transform.up
            };
        }

        /// <summary>
        /// Resolves a preset color for a handle. Default maps to a readable blue rather than white,
        /// which disappears against a bright skybox.
        /// </summary>
        /// <param name="color">The preset to resolve.</param>
        /// <returns>The color to draw with.</returns>
        public static Color Resolve(EColor color) => color == EColor.Default
            ? Fallback
            : color.ToColor();

        /// <summary>The point a gizmo is anchored at, honoring an optional offset member.</summary>
        /// <param name="context">The field being visualized.</param>
        /// <param name="member">Name of an optional Vector3 member holding the offset.</param>
        /// <param name="space">How the offset is interpreted.</param>
        /// <returns>The world position of the anchor.</returns>
        public static Vector3 Anchor(in HandleContext context, string member, ESpace space)
        {
            if (context.TryResolveVector(member, out Vector3 offset))
                return ToWorld(context.Transform, offset, space);

            return context.Transform == null
                ? Vector3.zero
                : context.Transform.position;
        }
    }
}