using System.Collections.Generic;
using Base.ControllerSupport.Devices;
using Base.SystemsCorePackage.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.ControllerSupport.Focus
{
    /// <summary>
    /// Global safety net that keeps a valid selection while a gamepad is in use. Re-selects the active
    /// context's focus target whenever the current selection becomes null or inactive, so the UI never
    /// goes "dead" for a gamepad user. Mouse and keyboard users may have nothing selected.
    /// </summary>
    public sealed class FocusWatchdog : GameServiceBehaviour
    {
        private readonly List<IFocusContext> _contexts = new();

        private InputDeviceTracker _deviceTracker;

#region Unity Callbacks
        private void LateUpdate()
        {
            if (_contexts.Count == 0)
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

            _contexts[^1].RestoreFocus();
        }
#endregion

        /// <summary>Registers a focus context as active. The most recently registered context wins.</summary>
        public void RegisterContext(IFocusContext context)
        {
            if (context == null)
                return;

            _contexts.Remove(context);
            _contexts.Add(context);
        }

        /// <summary>Removes a focus context from the active set.</summary>
        public void DeregisterContext(IFocusContext context)
        {
            if (context == null)
                return;

            _contexts.Remove(context);
        }
    }
}