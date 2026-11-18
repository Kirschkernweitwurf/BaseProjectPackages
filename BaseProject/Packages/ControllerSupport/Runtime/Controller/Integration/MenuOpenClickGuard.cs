using Base.CorePackage.MenuManaging.Modules;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.ControllerSupportPackage.Controller.Integration
{
    /// <summary>
    /// Ignores clicks and confirms for a moment after the menu opens. Without it the second press of a fast double
    /// click or of confirm pressed twice, lands on the screen the first press opened, like the next step
    /// starting right after the one before. The content keeps blocking raycasts, so an ignored click never
    /// falls through to the screen behind. A group wired at runtime links the buttons again on its own once they take
    /// presses.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuOpenClickGuard : MenuModule
    {
        [Min(0f)]
        [Tooltip("Seconds after the menu opens in which presses are ignored. A bit longer than a fast double click. "
            + "Runs on unscaled time, since many menus stop the game time.")]
        [SerializeField] private float guardDuration = 0.35f;

        private CanvasGroup _content;
        private float _guardEndTime;
        private bool _isGuarding;

#region Unity Callbacks
        private void Update()
        {
            if (!_isGuarding
                || Time.unscaledTime < _guardEndTime)
                return;

            Release();
        }
#endregion

        /// <summary>Stops the content from taking presses until the guard time is over.</summary>
        protected override void OnMenuOpened()
        {
            if (!TryGetContent(out _content))
                return;

            _content.interactable = false;
            _guardEndTime = Time.unscaledTime + guardDuration;
            _isGuarding = true;
        }

        /// <summary>Gives the content back its presses, so a menu closed early never stays locked.</summary>
        protected override void OnMenuClosed() => Release();

        // The menu fades its content root, which carries the canvas group of the whole screen.
        private bool TryGetContent(out CanvasGroup content)
        {
            content = OwnerMenu.ContentRoot == null
                ? null
                : OwnerMenu.ContentRoot.GetComponent<CanvasGroup>();

            if (content != null)
                return true;

            CustomLogger.LogWarning($"Menu \"{OwnerMenu.name}\" has no {nameof(CanvasGroup)} on its content root, so "
                + "it can't ignore presses after opening.", this);
            return false;
        }

        private void Release()
        {
            if (!_isGuarding)
                return;

            _isGuarding = false;
            _content.interactable = true;
        }
    }
}