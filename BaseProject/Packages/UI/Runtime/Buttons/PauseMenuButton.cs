using Base.AttributesPackage;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.MenuManaging.Identifier;
using Base.CorePackage.MenuManaging.Menus;
using Base.ServicesPackage;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Toggles the pause menu on button click and keeps the button icon in sync with the pause state.
    /// </summary>
    public sealed class PauseMenuButton : CustomButton
    {
        [Title("Identifier")]
        [Required] [SerializeField] private MenuIdentifier pauseMenuIdentifier;

        [Title("Icons")]
        [Required] [SerializeField] private Sprite pauseIcon;
        [Required] [SerializeField] private Sprite playIcon;

#region Unity Callbacks
        private void Start()
        {
            PauseMenu.OnPauseStateChanged += SetButtonIcon;

            SetButtonIcon(PauseMenu.IsPaused);
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

            // The icon follows PauseMenu.OnPauseStateChanged, so it is not set here
            if (menuManager.IsMenuOpen(pauseMenuIdentifier))
                menuManager.CloseMenu(pauseMenuIdentifier);
            else
                menuManager.OpenMenu(pauseMenuIdentifier);
        }

        private void SetButtonIcon(bool isPaused) => button.image.sprite = isPaused
            ? pauseIcon
            : playIcon;
    }
}