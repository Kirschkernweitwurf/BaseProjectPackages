using Base.CorePackage.Tracking;
using UnityEngine.InputSystem;

// ReSharper disable UnusedMember.Global

namespace Base.CorePackage.Input
{
    /// <summary>
    /// Bundles an <see cref="InputActionMap"/> with its <see cref="EPriority"/>, so it can be registered with the
    /// <see cref="InputManager"/> in one call.
    /// </summary>
    public readonly struct PrioritizedInputMap
    {
        /// <summary>
        /// The input action map to register.
        /// </summary>
        public InputActionMap Map { get; }

        /// <summary>
        /// The priority of the map. Higher priorities take precedence over lower ones.
        /// </summary>
        public EPriority Priority { get; }

        /// <summary>
        /// Creates a new bundle of a map and its priority.
        /// </summary>
        /// <param name="map">The input action map to register.</param>
        /// <param name="priority">The priority of the map.</param>
        public PrioritizedInputMap(InputActionMap map, EPriority priority)
        {
            Map = map;
            Priority = priority;
        }
    }
}