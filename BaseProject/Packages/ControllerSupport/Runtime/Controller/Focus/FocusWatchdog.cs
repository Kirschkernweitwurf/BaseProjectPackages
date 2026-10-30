using System.Collections.Generic;
using Base.ControllerSupportPackage.Controller.Navigation;
using Base.ControllerSupportPackage.InputPrompts.Devices;
using Base.ServicesPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.ControllerSupportPackage.Controller.Focus
{
    /// <summary>
    /// Global safety net that keeps the focus on the screen on top. The highest priority active
    /// <see cref="NavigableGroup"/> owns the focus, the one activated last on ties, and a selection that sits in a
    /// group behind it is moved out, so keys and buttons never act on a screen the player can't reach. While the
    /// player steers, a lost focus returns to that group, so the UI never goes dead. While the player points, the
    /// selection is only cleared off covered screens. Steering comes from an <see cref="ISteeringSource"/> when one
    /// is registered, otherwise from an <see cref="InputDeviceTracker"/> reporting a gamepad. Without either it
    /// always counts as steering.
    /// </summary>

    // The loop with NavigableGroup is by design. This exists to notice focus leaving a group, and a
    // group is what it hands focus back to, so neither half means anything without the other. The
    // same reason is written at NavigableGroup, because the tool anchors the finding on either one.
    public sealed class FocusWatchdog : GameServiceBehaviour
    {
        private readonly List<NavigableGroup> _groups = new();

        private bool _hasWarnedMissingEventSystem;
        private InputDeviceTracker _deviceTracker;
        private ISteeringSource _steering;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            // Optional. Without a tracker the watchdog guards focus for every device.
            ServiceLocator.TryGetOptional(out _deviceTracker);
        }

        private void LateUpdate()
        {
            if (_groups.Count == 0)
                return;

            if (!HasEventSystem())
                return;

            if (!TryResolveActiveGroup(out NavigableGroup top))
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (IsInCoveredGroup(current, top))
            {
                MoveFocusTo(top);
                return;
            }

            if (!IsSteering())
                return;

            if (current != null
                && current.activeInHierarchy)
                return;

            top.RestoreFocus();
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

        /// <summary>
        /// Removes a group from the active focus set. A selection inside it moves on at once, so a press in the same
        /// frame can't reach a screen that is closing.
        /// </summary>
        public void DeregisterGroup(NavigableGroup group)
        {
            if (group == null)
            {
                CustomLogger.LogWarning("Tried to deregister a null group.", this);
                return;
            }

            if (!_groups.Remove(group))
                return;

            if (EventSystem.current == null
                || !group.Contains(EventSystem.current.currentSelectedGameObject))
                return;

            PromoteToActiveGroup();
        }

        // The focus belongs to the new top group, unless it is in there already.
        private void PromoteToActiveGroup()
        {
            if (EventSystem.current == null)
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (!TryResolveActiveGroup(out NavigableGroup target))
            {
                // The last screen closed. A selection left on it would still take the next press while it fades.
                if (current != null)
                    EventSystem.current.SetSelectedGameObject(null);

                return;
            }

            if (current != null
                && current.activeInHierarchy
                && target.Contains(current))
                return;

            MoveFocusTo(target);
        }

        // Steering lights the group's focus target; pointing needs no focus, so the selection only goes away.
        private void MoveFocusTo(NavigableGroup group)
        {
            if (IsSteering())
                group.RestoreFocus();
            else if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        // Looked up on use, since the source may register after this scene service woke.
        private bool IsSteering()
        {
            if (_steering != null
                || ServiceLocator.TryGetOptional(out _steering))
                return _steering.IsSteering;

            return _deviceTracker == null
                || _deviceTracker.IsUsingGamepad;
        }

        private bool IsInCoveredGroup(GameObject current, NavigableGroup top)
        {
            if (current == null
                || top.Contains(current))
                return false;

            foreach (NavigableGroup group in _groups)
            {
                if (group != null
                    && group.Contains(current))
                    return true;
            }

            return false;
        }

        // This runs every frame, so the missing EventSystem is reported once instead of once per frame.
        private bool HasEventSystem()
        {
            if (EventSystem.current != null)
            {
                _hasWarnedMissingEventSystem = false;
                return true;
            }

            if (_hasWarnedMissingEventSystem)
                return false;

            _hasWarnedMissingEventSystem = true;
            CustomLogger.LogWarning("No EventSystem exists in the scene, cannot operate.", this);

            return false;
        }

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

                if (best != null
                    && group.Priority <= best.Priority)
                    continue;

                best = group;
            }

            return best != null;
        }
    }
}