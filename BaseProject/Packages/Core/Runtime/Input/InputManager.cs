using Base.ServicesPackage;
using Base.ServicesPackage.Tracking;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// ReSharper disable UnusedMember.Global

namespace Base.CorePackage.Input
{
    /// <summary>
    /// Scene-level manager of input action maps. Maps are registered with a priority and the manager enables the
    /// highest-priority map while disabling all others.
    /// The package's own Permanent map from <see cref="BaseInputActions"/> stays enabled at all times, so global
    /// actions like pausing or opening a menu always work.
    /// The manager also tracks whether the cursor is over a UI element, which can be used to block input while the
    /// player interacts with the UI.
    /// </summary>
    [DefaultExecutionOrder(-97)]
    public class InputManager : GameServiceBehaviour
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        /// <summary>
        /// Whether the cursor is currently over a UI element.
        /// </summary>
        public bool IsCursorOverGameObject { get; private set; }

        /// <summary>
        /// The package's input actions. Always available.
        /// </summary>
        public BaseInputActions BaseInputActions { get; private set; }

        private readonly PriorityTracker<InputActionMap> _tracker = new();

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            _tracker.OnCurrentActiveItemChanged += OnActiveInputMapChanged;

            BaseInputActions = new BaseInputActions();
            BaseInputActions.Permanent.Enable();
        }

        private void Update() => IsCursorOverGameObject = EventSystem.current != null
            && EventSystem.current.IsPointerOverGameObject();

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _tracker.OnCurrentActiveItemChanged -= OnActiveInputMapChanged;

            foreach (TrackedItem<InputActionMap> item in _tracker.TrackedItems)
            {
                if (item.Item == null
                    || !item.Item.enabled)
                    continue;

                item.Item.Disable();
            }

            BaseInputActions.Permanent.Disable();
            BaseInputActions.Dispose();
        }
#endregion

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// Registers an action map. It is active while it is the highest-priority entry.
        /// </summary>
        /// <param name="map">The map to activate.</param>
        /// <param name="caller">The object owning the registration. Used as key for deregistering.</param>
        /// <param name="priority">Higher priorities take precedence over lower ones.</param>
        public void RegisterInputMap(InputActionMap map, object caller, uint priority)
        {
            if (map == null)
            {
                CustomLogger.LogError("Tried to register a null action map.", this);
                return;
            }

            if (_tracker.HasCaller(caller))
            {
                CustomLogger.LogError("Tried activating an action map from the same object twice.", this);
                return;
            }

            _tracker.Add(map, priority, caller);
        }

        /// <summary>
        /// Registers an action map by reference. It is active while it is the highest-priority entry.
        /// </summary>
        /// <param name="reference">Reference to the map to activate.</param>
        /// <param name="caller">The object owning the registration. Used as key for deregistering.</param>
        /// <param name="priority">Higher priorities take precedence over lower ones.</param>
        public void RegisterInputMap(InputActionMapReference reference, object caller, uint priority)
            => RegisterInputMap(reference.Resolve(), caller, priority);

        /// <summary>
        /// Registers a prioritized action map. It is active while it is the highest-priority entry.
        /// </summary>
        /// <param name="prioritizedMap">The map and its priority.</param>
        /// <param name="caller">The object owning the registration. Used as key for deregistering.</param>
        public void RegisterInputMap(PrioritizedInputMap prioritizedMap, object caller)
            => RegisterInputMap(prioritizedMap.Map, caller, (uint)prioritizedMap.Priority);

        /// <summary>
        /// Removes the registration made by the given caller.
        /// </summary>
        /// <param name="caller">The object that registered the map.</param>
        public void DeregisterInputMap(object caller)
        {
            if (!_tracker.HasCaller(caller))
            {
                CustomLogger.LogWarning("Tried deactivating an action map from an unknown object.", this);
                return;
            }

            _tracker.Remove(caller);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// Resolves a map against the package's runtime actions clone, so callers enable the exact instance they
        /// subscribe to via <see cref="BaseInputActions"/>.
        /// </summary>
        /// <param name="reference">Reference to the map to resolve.</param>
        /// <returns>The resolved map, or <c>null</c> if the reference is invalid or the map does not exist.</returns>
        public InputActionMap ResolveBaseMap(InputActionMapReference reference) => !reference.IsValid
            ? null
            : BaseInputActions.asset.FindActionMap(reference.MapId);

        /// <summary>
        /// Tries to resolve a map against the package's runtime actions clone, so callers enable the exact instance
        /// they subscribe to via <see cref="BaseInputActions"/>.
        /// </summary>
        /// <param name="reference">Reference to the map to resolve.</param>
        /// <param name="map">The resolved map, or <c>null</c> if it could not be resolved.</param>
        /// <returns><c>true</c> if the map was resolved; otherwise <c>false</c>.</returns>
        public bool TryResolveBaseMap(InputActionMapReference reference, out InputActionMap map)
        {
            map = ResolveBaseMap(reference);
            return map != null;
        }

        private void OnActiveInputMapChanged(TrackedItem<InputActionMap> newActive)
        {
            foreach (TrackedItem<InputActionMap> item in _tracker.TrackedItems)
            {
                InputActionMap map = item.Item;

                if (map == null)
                    continue;

                if (map == newActive.Item)
                    map.Enable();
                else if (map.enabled)
                    map.Disable();
            }
        }
    }
}