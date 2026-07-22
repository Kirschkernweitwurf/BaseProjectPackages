using Base.AttributePackage;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.MenuManaging.Identifier;
using Base.CorePackage.MenuManaging.Menus;
using Base.CorePackage.Services;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Toggles the pause menu on button click.
    /// </summary>
    public class PauseMenuButton : CustomButton
    {
        [Header("Identifier")]

        [Required] [SerializeField] private MenuIdentifier pauseMenuIdentifier;

        [Header("Icons")]

        [Required] [SerializeField] private Sprite pauseIcon;
        [Required] [SerializeField] private Sprite playIcon;

#region Unity Callbacks
        private void Start()
        {
            PauseMenu.OnPauseStateChanged += SetButtonIcon;

            if (ServiceLocator.TryGet(out MenuManager menuManager))
                SetButtonIcon(menuManager.IsMenuOpen(pauseMenuIdentifier));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            PauseMenu.OnPauseStateChanged -= SetButtonIcon;
        }
#endregion

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

        private void SetButtonIcon(bool isPaused) => button.image.sprite = isPaused
            ? pauseIcon
            : playIcon;
    }
}