using System.Collections.Generic;
using Base.ControllerSupport.Controller.Navigation;
using Base.SystemsCorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.ControllerSupport.Controller.Focus
{
    /// <summary>
    /// Global safety net that keeps a valid selection while a gamepad is in use. When the current
    /// selection becomes null or inactive it restores focus to the highest priority active
    /// <see cref="NavigableGroup"/>, so the UI never goes dead for a gamepad user. Registering a group
    /// also promotes focus immediately when a higher priority group appears.
    /// </summary>
    public sealed class FocusWatchdog : GameServiceBehaviour
    {
        private readonly List<NavigableGroup> _groups = new();

#region Unity Callbacks
        private void LateUpdate()
        {
            if (_groups.Count == 0)
                return;

            if (EventSystem.current == null)
            {
                CustomLogger.LogWarning("No EventSystem exists in the scene, cannot operate.", this);
                return;
            }

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
            if (EventSystem.current == null)
                return;

            if (!TryResolveActiveGroup(out NavigableGroup target))
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;
            if (current != null && current.activeInHierarchy && target.Contains(current))
                return;

            target.RestoreFocus();
        }

        private bool TryResolveActiveGroup(out NavigableGroup best)
        {
            best = null;
            bool bestFound = false;

            // Walk newest to oldest so the most recently activated group wins ties on priority.
            for (int index = _groups.Count - 1; index >= 0; index--)
            {
                NavigableGroup group = _groups[index];

                if (group == null)
                    continue;

                if (best != null && group.Priority <= best.Priority)
                    continue;

                best = group;
                bestFound = true;
            }

            return bestFound;
        }
    }
}