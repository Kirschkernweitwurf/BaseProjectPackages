using Base.SystemsCorePackage.MenuManaging;
using Base.SystemsCorePackage.Services;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Opens the selected menu on button click.
    /// </summary>
    public class OpenMenuButton : CustomButton
    {
        [SerializeField] private MenuIdentifier menuToOpen;
        [SerializeField] private MenuIdentifier parentMenuIdentifier;

        protected override void OnClick()
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (!menuManager.IsMenuOpen(menuToOpen))
                menuManager.OpenMenu(menuToOpen, parentMenuIdentifier);
        }
    }
}