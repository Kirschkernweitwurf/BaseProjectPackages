using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// Holds every command queued through <see cref="DebugDraw"/> until its duration is up.
    /// Filled from the main thread and read back by <see cref="DebugDrawRenderer"/>.
    /// </summary>
    /// <remarks>
    /// Segments are sorted into a depth tested and an overlay list as they arrive, because those
    /// are the two GL passes they are drawn in. Sorting once on the way in beats testing a flag
    /// per segment in both passes on every camera.
    /// </remarks>
    internal static class DebugDrawBuffer
    {
        private const int MaxCommands = 8192;
        private const string OverflowFormat = "Debug draw is holding more than {0} commands of one kind. "
            + "The excess is dropped until the queued ones expire.";

        private static readonly List<DebugLineCommand> DepthTestedLines = new();
        private static readonly List<DebugLineCommand> OverlayLines = new();
        private static readonly List<DebugLabelCommand> Labels = new();

        private static bool _hasWarnedOverflow;

        /// <summary>False while drawing is switched off, which makes every add a no-op.</summary>
        internal static bool IsEnabled { get; private set; } = true;

        /// <summary>The line segments that geometry in front of them hides.</summary>
        internal static IReadOnlyList<DebugLineCommand> DepthTestedLineCommands => DepthTestedLines;

        /// <summary>The line segments drawn on top of everything.</summary>
        internal static IReadOnlyList<DebugLineCommand> OverlayLineCommands => OverlayLines;

        /// <summary>The labels waiting to be drawn.</summary>
        internal static IReadOnlyList<DebugLabelCommand> LabelCommands => Labels;

        /// <summary>Queues a line segment.</summary>
        /// <param name="from">The world space start of the segment.</param>
        /// <param name="to">The world space end of the segment.</param>
        /// <param name="color">The color the segment is drawn in.</param>
        /// <param name="duration">How long the segment stays, in unscaled seconds.</param>
        /// <param name="depthTest">False to draw the segment on top of everything.</param>
        internal static void AddLine(Vector3 from, Vector3 to, Color color, float duration, bool depthTest)
        {
            if (!IsEnabled)
                return;

            List<DebugLineCommand> target = depthTest
                ? DepthTestedLines
                : OverlayLines;

            if (!HasRoom(target.Count))
                return;

            target.Add(new DebugLineCommand(from, to, color, duration));
        }

        /// <summary>Queues a label.</summary>
        /// <param name="position">The world space position the text is centered on.</param>
        /// <param name="text">The text that is drawn.</param>
        /// <param name="color">The color the text is drawn in.</param>
        /// <param name="duration">How long the text stays, in unscaled seconds.</param>
        internal static void AddLabel(Vector3 position, string text, Color color, float duration)
        {
            if (!IsEnabled
                || !HasRoom(Labels.Count))
                return;

            Labels.Add(new DebugLabelCommand(position, text, color, duration));
        }

        /// <summary>Switches drawing on or off. Switching it off drops everything queued.</summary>
        /// <param name="value">True to keep accepting commands.</param>
        internal static void SetEnabled(bool value)
        {
            IsEnabled = value;

            if (!value)
                Clear();
        }

        /// <summary>Drops everything queued, including commands that would still be alive.</summary>
        internal static void Clear()
        {
            DepthTestedLines.Clear();
            OverlayLines.Clear();
            Labels.Clear();

            _hasWarnedOverflow = false;
        }

        /// <summary>
        /// Drops every command that has outlived its duration. Call once at the start of a frame,
        /// before any game code gets to queue new ones.
        /// </summary>
        internal static void Prune()
        {
            int frame = Time.frameCount;
            float unscaledTime = Time.unscaledTime;

            Prune(DepthTestedLines, frame, unscaledTime);
            Prune(OverlayLines, frame, unscaledTime);
            Prune(Labels, frame, unscaledTime);

            if (DepthTestedLines.Count < MaxCommands
                && OverlayLines.Count < MaxCommands
                && Labels.Count < MaxCommands)
                _hasWarnedOverflow = false;
        }

        // Compacted in place. The lists are refilled every frame, so removing entries one by one or
        // rebuilding the list would do a lot of copying and allocating for nothing.
        private static void Prune<T>(List<T> commands, int frame, float unscaledTime)
            where T : struct, IDebugDrawCommand
        {
            int alive = 0;

            for (int i = 0; i < commands.Count; i++)
            {
                if (!commands[i].IsAlive(frame, unscaledTime))
                    continue;

                commands[alive] = commands[i];
                alive++;
            }

            commands.RemoveRange(alive, commands.Count - alive);
        }

        // Warns once instead of per dropped command: whatever floods the buffer does so every frame.
        private static bool HasRoom(int count)
        {
            if (count < MaxCommands)
                return true;

            if (!_hasWarnedOverflow)
            {
                CustomLogger.LogWarning(string.Format(OverflowFormat, MaxCommands), null);
                _hasWarnedOverflow = true;
            }

            return false;
        }

        // Static state outlives a play session when domain reloading is switched off.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            DepthTestedLines.Clear();
            OverlayLines.Clear();
            Labels.Clear();

            IsEnabled = true;
            _hasWarnedOverflow = false;
        }
    }
}