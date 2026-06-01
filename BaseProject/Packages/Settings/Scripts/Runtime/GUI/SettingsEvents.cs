using System;

namespace Base.SettingsPackage.GUI
{
    /// <summary>
    /// Broadcast hub for settings UI commands, raised by the project (typically from input) and consumed by
    /// the settings elements. Replaces the previous direct dependency on a game-specific settings menu.
    /// </summary>
    public static class SettingsEvents
    {
        /// <summary>Raised to reset whichever setting element currently holds UI focus.</summary>
        public static event Action OnResetSelected;

        /// <summary>Raised when the active settings sub-menu changes, so transient UI can clear itself.</summary>
        public static event Action OnSubMenuChanged;

        /// <summary>Requests a reset of the focused setting element.</summary>
        public static void RaiseResetSelected() => OnResetSelected?.Invoke();

        /// <summary>Signals that the active settings sub-menu changed.</summary>
        public static void RaiseSubMenuChanged() => OnSubMenuChanged?.Invoke();
    }
}