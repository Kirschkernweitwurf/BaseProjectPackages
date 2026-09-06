using System.Collections.Generic;
using UnityEngine;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// Draws queued labels as screen space text. Owns the style and the content it reuses, so a
    /// frame full of labels does not allocate one of each per label.
    /// </summary>
    internal sealed class DebugLabelPainter
    {
        private const int FontSize = 12;
        private const float MinimumDepth = 0f;
        private const float ShadowOffset = 1f;

        private static readonly Color ShadowColor = new(0f, 0f, 0f, 0.75f);

        private readonly GUIContent _content = new();

        private GUIStyle _style;

        /// <summary>
        /// Draws every queued label. Call from <c>OnGUI</c> during a repaint event, otherwise the
        /// measured sizes are the ones IMGUI throws away again.
        /// </summary>
        /// <param name="labelCamera">The camera the world positions are projected through.</param>
        /// <param name="labels">The labels to draw.</param>
        internal void Draw(Camera labelCamera, IReadOnlyList<DebugLabelCommand> labels)
        {
            if (labels.Count == 0)
                return;

            // Built here rather than in a constructor: a GUIStyle copied from the skin is only
            // valid to build from inside the IMGUI callback.
            _style ??= CreateStyle();

            Rect screen = new(0f, 0f, Screen.width, Screen.height);

            for (int i = 0; i < labels.Count; i++)
                Draw(labels[i], labelCamera, screen);
        }

        private static GUIStyle CreateStyle() => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = FontSize,
            richText = true
        };

        private void Draw(DebugLabelCommand label, Camera labelCamera, Rect screen)
        {
            Vector3 screenPoint = labelCamera.WorldToScreenPoint(label.Position);

            // Behind the camera the projection mirrors the point back into view.
            if (screenPoint.z <= MinimumDepth)
                return;

            _content.text = label.Text;

            Vector2 size = _style.CalcSize(_content);

            Rect area = new(screenPoint.x - size.x * 0.5f, Screen.height - screenPoint.y - size.y * 0.5f,
                size.x, size.y);

            if (!area.Overlaps(screen))
                return;

            Rect shadow = new(area.x + ShadowOffset, area.y + ShadowOffset, area.width, area.height);

            // Drawn twice so the text stays readable on top of a bright scene.
            _style.normal.textColor = ShadowColor;
            GUI.Label(shadow, _content, _style);

            _style.normal.textColor = label.Color;
            GUI.Label(area, _content, _style);
        }
    }
}