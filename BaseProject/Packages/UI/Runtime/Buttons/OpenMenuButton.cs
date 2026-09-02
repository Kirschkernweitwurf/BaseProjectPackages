using Base.AttributesPackage;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.MenuManaging.Identifier;
using Base.ServicesPackage;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Opens the selected menu on button click.
    /// </summary>
    public sealed class OpenMenuButton : CustomButton
    {
        [Required] [SerializeField] private MenuIdentifier menuToOpen;

        [Tooltip("Optional. The menu that stays registered as parent of the opened menu.")]
        [SerializeField] private MenuIdentifier parentMenuIdentifier;

        protected override void OnClick()
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (menuManager.IsMenuOpen(menuToOpen))
                return;

            menuManager.OpenMenu(menuToOpen, parentMenuIdentifier);
        }
    }
}