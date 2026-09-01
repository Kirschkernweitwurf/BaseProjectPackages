using System.Diagnostics;
using UnityEngine;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// Draws lines, shapes and text labels that also show up in a player, unlike
    /// <see cref="Gizmos"/> and <see cref="UnityEngine.Debug.DrawLine(Vector3, Vector3)"/>, which
    /// are editor only.
    /// <para>
    /// Every call is compiled out of a release build, so neither the call nor the arguments handed
    /// to it cost anything there. Define <c>BASE_DEBUG_DRAW</c> to keep them in a release build.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Main thread only. A duration of zero draws for the current frame; a longer duration counts
    /// in unscaled seconds, so a paused game keeps showing what was drawn.
    /// </remarks>
    public static class DebugDraw
    {
        private const string DevelopmentSymbol = "DEVELOPMENT_BUILD";
        private const string EditorSymbol = "UNITY_EDITOR";
        private const string ForceSymbol = "BASE_DEBUG_DRAW";

        /// <summary>False while drawing is switched off and every call is ignored.</summary>
        public static bool IsEnabled => DebugDrawBuffer.IsEnabled;

        /// <summary>Draws a line between two world space positions.</summary>
        /// <param name="from">The world space start of the line.</param>
        /// <param name="to">The world space end of the line.</param>
        /// <param name="color">The color the line is drawn in.</param>
        /// <param name="duration">How long the line stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the line on top of everything.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Line(Vector3 from, Vector3 to, Color color, float duration = 0f, bool depthTest = true)
            => DebugDrawBuffer.AddLine(from, to, color, duration, depthTest);

        /// <summary>Draws a line from an origin along a direction.</summary>
        /// <param name="origin">The world space start of the ray.</param>
        /// <param name="direction">The direction and length of the ray.</param>
        /// <param name="color">The color the ray is drawn in.</param>
        /// <param name="duration">How long the ray stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the ray on top of everything.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Ray(Vector3 origin, Vector3 direction, Color color, float duration = 0f,
            bool depthTest = true) => DebugDrawBuffer.AddLine(origin, origin + direction, color, duration, depthTest);

        /// <summary>Draws a line with a head at its end, so its direction is readable.</summary>
        /// <param name="from">The world space start of the arrow.</param>
        /// <param name="to">The world space tip of the arrow.</param>
        /// <param name="color">The color the arrow is drawn in.</param>
        /// <param name="duration">How long the arrow stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the arrow on top of everything.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Arrow(Vector3 from, Vector3 to, Color color, float duration = 0f, bool depthTest = true)
            => DebugDrawShapes.AddArrow(from, to, color, duration, depthTest);

        /// <summary>Draws three axis aligned lines crossing at a position.</summary>
        /// <param name="position">The world space position the cross is centered on.</param>
        /// <param name="size">The full length of each line.</param>
        /// <param name="color">The color the cross is drawn in.</param>
        /// <param name="duration">How long the cross stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the cross on top of everything.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Point(Vector3 position, float size, Color color, float duration = 0f,
            bool depthTest = true) => DebugDrawShapes.AddPoint(position, size, color, duration, depthTest);

        /// <summary>Draws a wire sphere as three circles, one around each axis.</summary>
        /// <param name="center">The world space center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="color">The color the sphere is drawn in.</param>
        /// <param name="duration">How long the sphere stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the sphere on top of everything.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Sphere(Vector3 center, float radius, Color color, float duration = 0f,
            bool depthTest = true) => DebugDrawShapes.AddSphere(center, radius, color, duration, depthTest);

        /// <summary>Draws the twelve edges of an axis aligned box.</summary>
        /// <param name="center">The world space center of the box.</param>
        /// <param name="size">The full size of the box.</param>
        /// <param name="color">The color the box is drawn in.</param>
        /// <param name="duration">How long the box stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the box on top of everything.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Box(Vector3 center, Vector3 size, Color color, float duration = 0f,
            bool depthTest = true)
            => DebugDrawShapes.AddBox(center, size, Quaternion.identity, color, duration, depthTest);

        /// <summary>Draws the twelve edges of a rotated box.</summary>
        /// <param name="center">The world space center of the box.</param>
        /// <param name="size">The full size of the box along its own axes.</param>
        /// <param name="rotation">The rotation of the box.</param>
        /// <param name="color">The color the box is drawn in.</param>
        /// <param name="duration">How long the box stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the box on top of everything.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Box(Vector3 center, Vector3 size, Quaternion rotation, Color color, float duration = 0f,
            bool depthTest = true) => DebugDrawShapes.AddBox(center, size, rotation, color, duration, depthTest);

        /// <summary>Draws the twelve edges of a bounding box.</summary>
        /// <param name="bounds">The world space bounds to outline.</param>
        /// <param name="color">The color the box is drawn in.</param>
        /// <param name="duration">How long the box stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the box on top of everything.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Box(Bounds bounds, Color color, float duration = 0f, bool depthTest = true)
            => DebugDrawShapes.AddBox(bounds.center, bounds.size, Quaternion.identity, color, duration, depthTest);

        /// <summary>Draws text centered on a world space position, facing the screen.</summary>
        /// <param name="position">The world space position the text is centered on.</param>
        /// <param name="text">The text that is drawn. Rich text tags are supported.</param>
        /// <param name="color">The color the text is drawn in.</param>
        /// <param name="duration">How long the text stays, in unscaled seconds.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Label(Vector3 position, string text, Color color, float duration = 0f)
        {
            // Empty text is dropped silently rather than logged: this is called every frame, so a
            // warning would bury the console before it told anyone anything.
            if (string.IsNullOrEmpty(text))
                return;

            DebugDrawBuffer.AddLabel(position, text, color, duration);
        }

        /// <summary>Removes everything currently drawn, including commands with time left.</summary>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void Clear() => DebugDrawBuffer.Clear();

        /// <summary>Switches drawing on or off. Switching it off clears what is drawn.</summary>
        /// <param name="value">True to keep drawing.</param>
        [Conditional(EditorSymbol)] [Conditional(DevelopmentSymbol)] [Conditional(ForceSymbol)]
        public static void SetEnabled(bool value) => DebugDrawBuffer.SetEnabled(value);
    }
}