using Base.CorePackage.MenuManaging;
using Base.CorePackage.MenuManaging.Identifier;
using Base.CorePackage.Services;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Closes the selected menu on button click.
    /// </summary>
    public class CloseMenuButton : CustomButton
    {
        [SerializeField] private MenuIdentifier menuToClose;

        protected override void OnClick()
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (menuManager.IsMenuOpen(menuToClose))
                menuManager.CloseMenu(menuToClose);
        }
    }
}