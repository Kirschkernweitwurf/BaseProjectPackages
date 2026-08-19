using System.Collections.Generic;
using Base.ServicePackage;
using UnityEngine.InputSystem;

namespace Base.CorePackage.Input
{
    /// <summary>
    /// Shared behavior of the project's input service: reference counted map enabling and
    /// resolving <see cref="InputActionMapReference"/> against the runtime action asset.
    /// <para>
    /// The generated actions wrapper lives in the project assembly and cannot be referenced from
    /// a package, so the project supplies only <see cref="RuntimeAsset"/> and this class holds
    /// everything that does not depend on the generated type.
    /// </para>
    /// </summary>
    public abstract class ProjectInputServiceBase : GameServiceBehaviour
    {
        private readonly Dictionary<InputActionMap, int> _enabledMapCounts = new();

        /// <summary>
        /// The runtime action asset maps are resolved and enabled against. This is the clone the
        /// generated wrapper creates, not the source asset, so callers enable the exact instance
        /// they subscribe to.
        /// </summary>
        protected abstract InputActionAsset RuntimeAsset { get; }

        /// <summary>
        /// Enables the given map. Reference counted, so it stays enabled until every caller
        /// that enabled it has disabled it again.
        /// </summary>
        /// <param name="map">The map to enable.</param>
        public void EnableMap(InputActionMap map)
        {
            if (map == null)
                return;

            _enabledMapCounts.TryGetValue(map, out int count);
            _enabledMapCounts[map] = count + 1;

            if (count == 0)
                map.Enable();
        }

        /// <summary>
        /// Releases the given map. Only actually disables it once every caller that enabled
        /// it has released it again.
        /// </summary>
        /// <param name="map">The map to release.</param>
        public void DisableMap(InputActionMap map)
        {
            if (map == null || !_enabledMapCounts.TryGetValue(map, out int count))
                return;

            count--;

            if (count > 0)
            {
                _enabledMapCounts[map] = count;
                return;
            }

            _enabledMapCounts.Remove(map);
            map.Disable();
        }

        /// <summary>
        /// Tries to resolve a map against the runtime actions clone, so callers enable the
        /// exact instance they subscribe to via <see cref="BaseInputActions"/>.
        /// </summary>
        /// <param name="reference">The reference to resolve.</param>
        /// <param name="map">The resolved map, or null if the reference did not match.</param>
        /// <returns><c>true</c> if the reference was valid and the map was resolved; otherwise, <c>false</c>.</returns>
        public bool TryResolveBaseMap(InputActionMapReference reference, out InputActionMap map)
        {
            map = ResolveBaseMap(reference);

            return map != null;
        }

        /// <summary>
        /// Resolves a map against the runtime actions clone, so callers enable the
        /// exact instance they subscribe to via <see cref="BaseInputActions"/>.
        /// </summary>
        /// <param name="reference">The reference to resolve.</param>
        /// <returns>The resolved map, or null if the reference did not match.</returns>
        private InputActionMap ResolveBaseMap(InputActionMapReference reference)
            => RuntimeAsset.FindActionMap(reference.MapId);
    }
}