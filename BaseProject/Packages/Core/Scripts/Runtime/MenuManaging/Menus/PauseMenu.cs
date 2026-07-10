using System;
using UnityEditor;

namespace Base.CorePackage.MenuManaging.Menus
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

        /// <summary>
        /// Gets whether the game is currently paused.
        /// </summary>
        public static bool IsPaused { get; private set; }

        protected override void OnOpened()
        {
            base.OnOpened();

            IsPaused = true;
            OnPauseStateChanged?.Invoke(true);
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            IsPaused = false;
            OnPauseStateChanged?.Invoke(false);
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics() => IsPaused = false;
#endif
    }
}