using System;

namespace Systems.MenuManaging.Menus
{
    /// <summary>
    /// Represents the pause menu in the game.
    /// </summary>
    public class PauseMenu : Menu
    {
        /// <summary>
        /// Event invoked when the pause state changes.
        /// The bool parameter indicates whether the game is paused (true) or unpaused (false).
        /// </summary>
        public static event Action<bool> OnPauseStateChanged;

        protected override void OnOpened()
        {
            base.OnOpened();

            OnPauseStateChanged?.Invoke(true);
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            OnPauseStateChanged?.Invoke(false);
        }
    }
}