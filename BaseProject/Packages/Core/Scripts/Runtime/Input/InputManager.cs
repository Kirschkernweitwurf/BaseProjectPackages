using Base.AttributePackage;
using Base.CorePackage.Services;
using Base.CorePackage.Tracking;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Base.CorePackage.Input
{
    /// <summary>
    /// Scene-level manager of input action maps. Action maps can be registered with a priority and the manager will
    /// enable the highest-priority map while disabling all others.
    /// The base package's input actions are always registered at a priority below any consumer maps,
    /// so they can be used for global actions like pausing or opening a menu.
    /// The manager also tracks whether the cursor is currently over a UI element,
    /// which can be used to block input when the player is interacting with the UI.
    /// </summary>
    [DefaultExecutionOrder(-97)]
    public class InputManager : GameServiceBehaviour
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsCursorOverGameObject { get; private set; }

        /// <summary>
        /// The base package's input actions. Always available.
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

        private void Update() => IsCursorOverGameObject =
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _tracker.OnCurrentActiveItemChanged -= OnActiveInputMapChanged;

            foreach (TrackedItem<InputActionMap> item in _tracker.TrackedItems)
            {
                if (item.Item is
                    {
                        enabled: true
                    })
                    item.Item.Disable();
            }

            BaseInputActions.Permanent.Disable();
            BaseInputActions?.Dispose();
        }
#endregion

        /// <summary>
        /// Register an action map. Will be active while it is the highest-priority entry.
        /// </summary>

        // ReSharper disable once MemberCanBePrivate.Global
        public void RegisterInputMap(InputActionMap map, object caller, uint priority)
        {
            if (map == null)
            {
                CustomLogger.LogError("Tried to register a null action map.", this);
                return;
            }

            if (_tracker.HasCaller(caller))
            {
                CustomLogger.LogError("Tried activating action map from same object twice.", this);
                return;
            }

            _tracker.Add(map, priority, caller);
        }

        /// <summary>
        /// Register an action map by reference. Will be active while it is the highest-priority entry.
        /// </summary>
        public void RegisterInputMap(InputActionMapReference reference, object caller, uint priority)
            => RegisterInputMap(reference.Resolve(), caller, priority);

        /// <summary>
        /// Register a prioritized action map. Will be active while it is the highest-priority entry.
        /// </summary>
        public void RegisterInputMap(PrioritizedInputMap prioritizedMap, object caller)
            => RegisterInputMap(prioritizedMap.Map, caller, (uint)prioritizedMap.Priority);

        public void DeregisterInputMap(object caller)
        {
            if (!_tracker.HasCaller(caller))
            {
                CustomLogger.LogWarning("Tried deactivating action map from unknown object.", this);
                return;
            }

            _tracker.Remove(caller);
        }

        /// <summary>
        /// Resolves a map against the package's runtime actions clone, so callers enable the
        /// exact instance they subscribe to via <see cref="BaseInputActions"/>.
        /// </summary>

        // ReSharper disable once MemberCanBePrivate.Global
        public InputActionMap ResolveBaseMap(InputActionMapReference reference)
            => BaseInputActions.asset.FindActionMap(reference.MapId);

        /// <summary>
        /// Tries to resolve a map against the package's runtime actions clone, so callers enable the
        /// exact instance they subscribe to via <see cref="BaseInputActions"/>.
        /// </summary>
        /// <returns><c>>true</c> if the reference was valid and the map was resolved; otherwise, <c>false</c>.</returns>
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