using UnityEngine;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// Turns the shapes <see cref="DebugDraw"/> offers into the line segments the renderer draws.
    /// Pure geometry, so the renderer never has to know what a sphere or a box is.
    /// </summary>
    internal static class DebugDrawShapes
    {
        private const float ArrowHeadLengthFraction = 0.15f;
        private const float ArrowHeadWidthFraction = 0.4f;
        private const float MinimumArrowLength = 0.0001f;
        private const int SphereSegments = 24;

        /// <summary>Adds a line with a four sided head at its end.</summary>
        /// <param name="from">The world space start of the arrow.</param>
        /// <param name="to">The world space tip of the arrow.</param>
        /// <param name="color">The color the arrow is drawn in.</param>
        /// <param name="duration">How long the arrow stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the arrow on top of everything.</param>
        internal static void AddArrow(Vector3 from, Vector3 to, Color color, float duration, bool depthTest)
        {
            DebugDrawBuffer.AddLine(from, to, color, duration, depthTest);

            Vector3 direction = to - from;

            // A zero length arrow has no direction to orient the head along, and asking
            // Quaternion.LookRotation for one logs an error of its own.
            if (direction.sqrMagnitude <= MinimumArrowLength * MinimumArrowLength)
                return;

            float headLength = direction.magnitude * ArrowHeadLengthFraction;
            float headWidth = headLength * ArrowHeadWidthFraction;

            Quaternion rotation = Quaternion.LookRotation(direction);

            Vector3 back = rotation * new Vector3(0f, 0f, -headLength);
            Vector3 right = rotation * new Vector3(headWidth, 0f, 0f);
            Vector3 up = rotation * new Vector3(0f, headWidth, 0f);
            Vector3 neck = to + back;

            DebugDrawBuffer.AddLine(to, neck + right, color, duration, depthTest);
            DebugDrawBuffer.AddLine(to, neck - right, color, duration, depthTest);
            DebugDrawBuffer.AddLine(to, neck + up, color, duration, depthTest);
            DebugDrawBuffer.AddLine(to, neck - up, color, duration, depthTest);
        }

        /// <summary>Adds the twelve edges of a rotated box.</summary>
        /// <param name="center">The world space center of the box.</param>
        /// <param name="size">The full size of the box along its own axes.</param>
        /// <param name="rotation">The rotation of the box.</param>
        /// <param name="color">The color the box is drawn in.</param>
        /// <param name="duration">How long the box stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the box on top of everything.</param>
        internal static void AddBox(Vector3 center, Vector3 size, Quaternion rotation, Color color, float duration,
            bool depthTest)
        {
            Vector3 right = rotation * new Vector3(size.x * 0.5f, 0f, 0f);
            Vector3 up = rotation * new Vector3(0f, size.y * 0.5f, 0f);
            Vector3 forward = rotation * new Vector3(0f, 0f, size.z * 0.5f);

            Vector3 bottomBackLeft = center - right - up - forward;
            Vector3 bottomBackRight = center + right - up - forward;
            Vector3 bottomFrontRight = center + right - up + forward;
            Vector3 bottomFrontLeft = center - right - up + forward;

            Vector3 topBackLeft = center - right + up - forward;
            Vector3 topBackRight = center + right + up - forward;
            Vector3 topFrontRight = center + right + up + forward;
            Vector3 topFrontLeft = center - right + up + forward;

            DebugDrawBuffer.AddLine(bottomBackLeft, bottomBackRight, color, duration, depthTest);
            DebugDrawBuffer.AddLine(bottomBackRight, bottomFrontRight, color, duration, depthTest);
            DebugDrawBuffer.AddLine(bottomFrontRight, bottomFrontLeft, color, duration, depthTest);
            DebugDrawBuffer.AddLine(bottomFrontLeft, bottomBackLeft, color, duration, depthTest);

            DebugDrawBuffer.AddLine(topBackLeft, topBackRight, color, duration, depthTest);
            DebugDrawBuffer.AddLine(topBackRight, topFrontRight, color, duration, depthTest);
            DebugDrawBuffer.AddLine(topFrontRight, topFrontLeft, color, duration, depthTest);
            DebugDrawBuffer.AddLine(topFrontLeft, topBackLeft, color, duration, depthTest);

            DebugDrawBuffer.AddLine(bottomBackLeft, topBackLeft, color, duration, depthTest);
            DebugDrawBuffer.AddLine(bottomBackRight, topBackRight, color, duration, depthTest);
            DebugDrawBuffer.AddLine(bottomFrontRight, topFrontRight, color, duration, depthTest);
            DebugDrawBuffer.AddLine(bottomFrontLeft, topFrontLeft, color, duration, depthTest);
        }

        /// <summary>Adds three axis aligned lines crossing at a position.</summary>
        /// <param name="position">The world space position the cross is centered on.</param>
        /// <param name="size">The full length of each line.</param>
        /// <param name="color">The color the cross is drawn in.</param>
        /// <param name="duration">How long the cross stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the cross on top of everything.</param>
        internal static void AddPoint(Vector3 position, float size, Color color, float duration, bool depthTest)
        {
            float extent = size * 0.5f;

            Vector3 right = new(extent, 0f, 0f);
            Vector3 up = new(0f, extent, 0f);
            Vector3 forward = new(0f, 0f, extent);

            DebugDrawBuffer.AddLine(position - right, position + right, color, duration, depthTest);
            DebugDrawBuffer.AddLine(position - up, position + up, color, duration, depthTest);
            DebugDrawBuffer.AddLine(position - forward, position + forward, color, duration, depthTest);
        }

        /// <summary>Adds a wire sphere as three circles, one around each axis.</summary>
        /// <param name="center">The world space center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="color">The color the sphere is drawn in.</param>
        /// <param name="duration">How long the sphere stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the sphere on top of everything.</param>
        internal static void AddSphere(Vector3 center, float radius, Color color, float duration, bool depthTest)
        {
            Vector3 right = new(radius, 0f, 0f);
            Vector3 up = new(0f, radius, 0f);
            Vector3 forward = new(0f, 0f, radius);

            AddCircle(center, right, up, color, duration, depthTest);
            AddCircle(center, right, forward, color, duration, depthTest);
            AddCircle(center, up, forward, color, duration, depthTest);
        }

        // The axes arrive already scaled to the radius, so the loop only has to weigh them.
        private static void AddCircle(Vector3 center, Vector3 firstAxis, Vector3 secondAxis, Color color,
            float duration, bool depthTest)
        {
            float step = Mathf.PI * 2f / SphereSegments;
            Vector3 previous = center + firstAxis;

            for (int i = 1; i <= SphereSegments; i++)
            {
                float angle = step * i;
                Vector3 current = center + firstAxis * Mathf.Cos(angle) + secondAxis * Mathf.Sin(angle);

                DebugDrawBuffer.AddLine(previous, current, color, duration, depthTest);

                previous = current;
            }
        }
    }
}