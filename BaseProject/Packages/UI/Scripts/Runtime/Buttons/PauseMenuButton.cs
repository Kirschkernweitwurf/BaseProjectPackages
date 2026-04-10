using Systems.MenuManaging;
using Systems.MenuManaging.Menus;
using Systems.Services;
using UnityEngine;

namespace UI.Buttons
{
    /// <summary>
    /// Toggles the pause menu on button click.
    /// </summary>
    public class PauseMenuButton : CustomButton
    {
        [Header("Identifier")]
        [SerializeField] private EMenuIdentifier pauseMenuIdentifier = EMenuIdentifier.Pause;

        [Header("Icons")]
        [SerializeField] private Sprite pauseIcon;
        [SerializeField] private Sprite playIcon;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            SetButtonIcon(menuManager.IsMenuOpen(pauseMenuIdentifier));
            PauseMenu.OnPauseStateChanged += SetButtonIcon;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            PauseMenu.OnPauseStateChanged -= SetButtonIcon;
        }

        protected override void OnClick()
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (menuManager.IsMenuOpen(pauseMenuIdentifier))
            {
                menuManager.CloseMenu(pauseMenuIdentifier);
                SetButtonIcon(false);
            }
            else
            {
                menuManager.OpenMenu(pauseMenuIdentifier);
                SetButtonIcon(true);
            }
        }

        private void SetButtonIcon(bool isPaused) => button.image.sprite = isPaused ? pauseIcon : playIcon;
    }
}