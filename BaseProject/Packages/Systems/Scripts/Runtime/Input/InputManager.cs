using Attributes;
using Systems.Services;
using Tracking;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utility.Logging;

namespace Systems.Input
{
    /// <summary>
    /// Takes care of Activation and Deactivation of desired ActionMaps.
    /// Manages input action maps based on priority using InputMapPriorityTracker.
    /// </summary>
    [DefaultExecutionOrder(-98)]
    public class InputManager : GameServiceBehaviour
    {
        public InputSystem_Actions InputActions { get; private set; }

        public bool IsCursorOverGameObject { get; private set; }

        [SerializeField, InputActionMapName] private string permanentActionMapName;

        private readonly PriorityTracker<InputActionMap> _tracker = new();

        protected override void Awake()
        {
            base.Awake();

            _tracker.OnCurrentActiveItemChanged += OnActiveInputMapChanged;

            InputActions = new InputSystem_Actions();

            InputActionMap permanentActionMap = InputActions.asset.FindActionMap(permanentActionMapName);
            permanentActionMap?.Enable();
        }

        private void Update()
        {
            IsCursorOverGameObject = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _tracker.OnCurrentActiveItemChanged -= OnActiveInputMapChanged;

            InputActions?.Dispose();
        }

        /// <summary>
        /// Adds the input map with the given name to the tracker with the specified priority.
        /// The caller object is used to identify the registration.
        /// </summary>
        /// <param name="actionMapName">The name of the action map to register.</param>
        /// <param name="caller">The object that is registering the input map.</param>
        /// <param name="priority">The priority of the input map.</param>
        public void RegisterInputMap(string actionMapName, object caller, uint priority)
        {
            if (_tracker.HasCaller(caller))
            {
                CustomLogger.LogError("Tried activating action map same object twice.", this);
                return;
            }

            InputActionMap newActionMap = InputActions.asset.FindActionMap(actionMapName);
            if (newActionMap == null)
            {
                CustomLogger.LogError($"Could not find action map with name {actionMapName}.", this);
                return;
            }

            _tracker.Add(newActionMap, priority, caller);
        }

        /// <summary>
        /// Removes the input map associated with the given caller from the tracker.
        /// </summary>
        /// <param name="caller">The object that registered the input map.</param>
        public void DeregisterInputMap(object caller)
        {
            if (!_tracker.HasCaller(caller))
            {
                CustomLogger.LogWarning("Tried deactivating action map from unknown object.", this);
                return;
            }

            _tracker.Remove(caller);
        }

        private void OnActiveInputMapChanged(TrackedItem<InputActionMap> newActiveActionMap)
        {
            foreach (TrackedItem<InputActionMap> trackedItem in _tracker.TrackedItems)
            {
                InputActionMap map = trackedItem.Item;
                if (map == null)
                    continue;

                if (map == newActiveActionMap.Item)
                    map.Enable();
                else if (map.enabled)
                    map.Disable();
            }
        }
    }
}