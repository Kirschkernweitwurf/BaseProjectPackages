using UnityEngine;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// One piece of text waiting to be drawn at a world space position, together with the lifetime
    /// it was queued with.
    /// </summary>
    internal readonly struct DebugLabelCommand : IDebugDrawCommand
    {
        private readonly int _frame;
        private readonly float _expireTime;

        /// <summary>The world space position the text is centered on.</summary>
        internal Vector3 Position { get; }

        /// <summary>The text that is drawn.</summary>
        internal string Text { get; }

        /// <summary>The color the text is drawn in.</summary>
        internal Color Color { get; }

        /// <summary>Creates a queued label.</summary>
        /// <param name="position">The world space position the text is centered on.</param>
        /// <param name="text">The text that is drawn.</param>
        /// <param name="color">The color the text is drawn in.</param>
        /// <param name="duration">How long the text stays, in unscaled seconds. Zero draws one frame.</param>
        internal DebugLabelCommand(Vector3 position, string text, Color color, float duration)
        {
            Position = position;
            Text = text;
            Color = color;

            _frame = Time.frameCount;
            _expireTime = Time.unscaledTime + duration;
        }

        /// <inheritdoc/>
        public bool IsAlive(int frame, float unscaledTime) => frame == _frame || unscaledTime <= _expireTime;
    }
}