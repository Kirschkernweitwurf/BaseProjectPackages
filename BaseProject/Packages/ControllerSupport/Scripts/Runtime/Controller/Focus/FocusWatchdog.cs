using System.Collections.Generic;
using Base.ControllerSupport.Controller.Navigation;
using Base.ControllerSupport.InputPrompts.Devices;
using Base.CorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.ControllerSupport.Controller.Focus
{
    /// <summary>
    /// Global safety net that keeps a valid selection while a gamepad is in use. When the current
    /// selection becomes null or inactive it restores focus to the highest priority active
    /// <see cref="NavigableGroup"/>, so the UI never goes dead for a gamepad user. Registering a group
    /// also promotes focus immediately when a higher priority group appears. With an
    /// <see cref="InputDeviceTracker"/> available, restoration only runs while the gamepad is the
    /// active device, so mouse users can deselect freely. Without one it always runs.
    /// </summary>
    public sealed class FocusWatchdog : GameServiceBehaviour
    {
        private readonly List<NavigableGroup> _groups = new();

        private bool _hasWarnedMissingEventSystem;
        private InputDeviceTracker _deviceTracker;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();
            ServiceLocator.TryGet(out _deviceTracker);
        }

        private void LateUpdate()
        {
            if (_groups.Count == 0 || !ShouldGuardFocus())
                return;

            if (EventSystem.current == null)
            {
                // LateUpdate retries every frame, so warn only once instead of flooding the log.
                if (!_hasWarnedMissingEventSystem)
                {
                    _hasWarnedMissingEventSystem = true;
                    CustomLogger.LogWarning("No EventSystem exists in the scene, cannot operate.", this);
                }

                return;
            }

            _hasWarnedMissingEventSystem = false;

            GameObject current = EventSystem.current.currentSelectedGameObject;
            if (current != null && current.activeInHierarchy)
                return;

            if (TryResolveActiveGroup(out NavigableGroup group))
                group.RestoreFocus();
        }
#endregion

        /// <summary>Registers a group as active and promotes focus if it is now the highest priority one.</summary>
        public void RegisterGroup(NavigableGroup group)
        {
            if (group == null)
            {
                CustomLogger.LogWarning("Tried to register a null group.", this);
                return;
            }

            _groups.Remove(group);
            _groups.Add(group);

            PromoteToActiveGroup();
        }

        /// <summary>Removes a group from the active focus set.</summary>
        public void DeregisterGroup(NavigableGroup group)
        {
            if (group == null)
            {
                CustomLogger.LogWarning("Tried to deregister a null group.", this);
                return;
            }

            _groups.Remove(group);
        }

        private void PromoteToActiveGroup()
        {
            if (EventSystem.current == null || !ShouldGuardFocus())
                return;

            if (!TryResolveActiveGroup(out NavigableGroup target))
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;
            if (current != null && current.activeInHierarchy && target.Contains(current))
                return;

            target.RestoreFocus();
        }

        private bool ShouldGuardFocus() => _deviceTracker == null || _deviceTracker.IsUsingGamepad;

        private bool TryResolveActiveGroup(out NavigableGroup best)
        {
            best = null;

            // Walk newest to oldest so the most recently activated group wins ties on priority.
            // Destroyed groups that never deregistered are pruned along the way.
            for (int index = _groups.Count - 1; index >= 0; index--)
            {
                NavigableGroup group = _groups[index];

                if (group == null)
                {
                    _groups.RemoveAt(index);
                    continue;
                }

                if (best != null && group.Priority <= best.Priority)
                    continue;

                best = group;
            }

            return best != null;
        }
    }
}