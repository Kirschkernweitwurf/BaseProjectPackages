using UnityEngine;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// One world space line segment waiting to be drawn, together with the lifetime it was queued with.
    /// Every shape ends up as a set of these, so the renderer only ever deals with segments.
    /// </summary>
    /// <remarks>
    /// Whether the segment is depth tested is not stored here. It follows from which of the two
    /// lists in <see cref="DebugDrawBuffer"/> the segment sits in, which keeps the inner draw loop
    /// free of a branch it would take for every single segment.
    /// </remarks>
    internal readonly struct DebugLineCommand : IDebugDrawCommand
    {
        private readonly int _frame;
        private readonly float _expireTime;

        /// <summary>The world space start of the segment.</summary>
        internal Vector3 From { get; }

        /// <summary>The world space end of the segment.</summary>
        internal Vector3 To { get; }

        /// <summary>The color the segment is drawn in.</summary>
        internal Color Color { get; }

        /// <summary>Creates a queued line segment.</summary>
        /// <param name="from">The world space start of the segment.</param>
        /// <param name="to">The world space end of the segment.</param>
        /// <param name="color">The color the segment is drawn in.</param>
        /// <param name="duration">How long the segment stays, in unscaled seconds. Zero draws one frame.</param>
        internal DebugLineCommand(Vector3 from, Vector3 to, Color color, float duration)
        {
            From = from;
            To = to;
            Color = color;

            _frame = Time.frameCount;
            _expireTime = Time.unscaledTime + duration;
        }

        /// <inheritdoc/>
        // The frame check is what makes a duration of zero survive exactly one frame: it is drawn
        // during the frame it was queued in, no matter where in that frame the call happened.
        public bool IsAlive(int frame, float unscaledTime) => frame == _frame || unscaledTime <= _expireTime;
    }
}