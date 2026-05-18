using Base.SystemsCorePackage.Tracking;
using UnityEngine.InputSystem;

namespace Base.SystemsCorePackage.Input
{
    /// <summary>
    /// Bundles an <see cref="InputActionMap"/> with its <see cref="EPriority"/>.
    /// Used to register a map with the <see cref="InputManager"/> in one call.
    /// </summary>
    public readonly struct PrioritizedInputMap
    {
        /// <summary>
        /// The input action map to register.
        /// </summary>
        public InputActionMap Map { get; }

        /// <summary>
        /// The priority of the input action map. Higher priority maps will take precedence over lower priority ones.
        /// </summary>
        public EPriority Priority { get; }

        public PrioritizedInputMap(InputActionMap map, EPriority priority)
        {
            Map = map;
            Priority = priority;
        }
    }
}