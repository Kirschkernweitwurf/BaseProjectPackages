using Base.AttributesPackage;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.MenuManaging.Identifier;
using Base.ServicesPackage;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Closes the selected menu on button click.
    /// </summary>
    public sealed class CloseMenuButton : CustomButton
    {
        [Required] [SerializeField] private MenuIdentifier menuToClose;

        protected override void OnClick()
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (!menuManager.IsMenuOpen(menuToClose))
                return;

            menuManager.CloseMenu(menuToClose);
        }
    }
}