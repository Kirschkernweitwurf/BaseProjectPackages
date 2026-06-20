using System.Collections.Generic;
using Base.SystemsCorePackage.GamepadSupport.Devices;
using Base.SystemsCorePackage.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ControllerSupport.Scripts.Focus
{
    /// <summary>
    /// Global safety net that keeps a valid selection while a gamepad is in use. Re-selects the active
    /// <see cref="IFocusContext"/> whenever the current selection becomes <c>null</c> or inactive. Contexts register
    /// themselves when they are active (e.g. when their menu is open) and deregister when they are not.
    /// </summary>
    public sealed class FocusWatchdog : GameServiceBehaviour
    {
        private readonly List<IFocusContext> _entries = new();

        private InputDeviceTracker _deviceTracker;

#region Unity Callbacks
        private void LateUpdate()
        {
            if (_entries.Count == 0)
                return;

            if (EventSystem.current == null)
                return;

            if (_deviceTracker == null)
                ServiceLocator.TryGet(out _deviceTracker);

            // Only force a selection for gamepad users. Mouse users may click into empty space.
            if (_deviceTracker != null && !_deviceTracker.IsUsingGamepad)
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null && current.activeInHierarchy)
                return;

            _entries[^1].RestoreFocus();
        }
#endregion

        /// <summary>Registers an entry as the active focus context. The most recent entry wins.</summary>
        public void RegisterEntry(IFocusContext entry)
        {
            if (entry == null)
                return;

            _entries.Remove(entry);
            _entries.Add(entry);
        }

        /// <summary>Removes an entry from the active focus contexts.</summary>
        public void DeregisterEntry(IFocusContext entry)
        {
            if (entry == null)
                return;

            _entries.Remove(entry);
        }
    }
}