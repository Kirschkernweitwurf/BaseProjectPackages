using UnityEngine;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// The drawn form of one transition: a cubic curve between two boxes plus the key the monitor matches
    /// against to tell which transition fired last.
    /// </summary>
    internal readonly struct StateMachineCanvasEdge
    {
        /// <summary>Identifies the transition this curve stands for.</summary>
        internal string Key { get; }

        /// <summary>Where the curve leaves the source box.</summary>
        internal Vector2 Start { get; }

        /// <summary>The control point pulling the curve out of the source.</summary>
        internal Vector2 ControlA { get; }

        /// <summary>The control point pulling the curve into the target.</summary>
        internal Vector2 ControlB { get; }

        /// <summary>Where the curve meets the target box.</summary>
        internal Vector2 End { get; }

        /// <summary>Builds the drawn form of one transition.</summary>
        /// <param name="key">Identifies the transition this curve stands for.</param>
        /// <param name="start">Where the curve leaves the source box.</param>
        /// <param name="controlA">The control point pulling the curve out of the source.</param>
        /// <param name="controlB">The control point pulling the curve into the target.</param>
        /// <param name="end">Where the curve meets the target box.</param>
        internal StateMachineCanvasEdge(string key, Vector2 start, Vector2 controlA, Vector2 controlB, Vector2 end)
        {
            Key = key;
            Start = start;
            ControlA = controlA;
            ControlB = controlB;
            End = end;
        }

        /// <summary>The point halfway along the curve, where the label goes.</summary>
        /// <returns>The midpoint in canvas space.</returns>
        internal Vector2 Midpoint() => 0.125f * (Start + 3f * ControlA + 3f * ControlB + End);
    }
}