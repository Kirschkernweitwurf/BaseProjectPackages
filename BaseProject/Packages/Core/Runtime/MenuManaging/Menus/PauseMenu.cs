using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Base.CorePackage.MenuManaging.Menus
{
    /// <summary>
    /// Represents the pause menu in the game. A pause menu that goes away while open, like with a scene that unloads
    /// behind it, still ends the pause, so <see cref="IsPaused"/> never stays set with no pause menu left.
    /// </summary>
    public class PauseMenu : Menu
    {
        /// <summary>
        /// Event invoked when the pause state changes.
        /// The bool parameter indicates whether the game is paused (true) or unpaused (false).
        /// </summary>
        public static event Action<bool> OnPauseStateChanged;

        /// <summary>
        /// Event invoked when the pause menu starts its close animation. The game stays paused until
        /// <see cref="OnPauseStateChanged"/> reports <c>false</c>, so use it only to start a return early, like a
        /// HUD fading back in while the menu animates out.
        /// </summary>
        public static event Action Unpausing;

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

        /// <inheritdoc/>
        public override void Shutdown()
        {
            bool wasPausing = IsOpen
                && !HasShutDown;

            base.Shutdown();

            // A destroyed menu never closes, so without this the game would count as paused for good.
            if (wasPausing)
                Unpause();
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            Unpausing?.Invoke();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            Unpause();
        }

        private static void Unpause()
        {
            IsPaused = false;
            OnPauseStateChanged?.Invoke(false);
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics()
        {
            IsPaused = false;
            Unpausing = null;
        }
#endif
    }
}