using Base.AttributePackage;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.MenuManaging.Identifier;
using Base.CorePackage.Services;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Opens the selected menu on button click.
    /// </summary>
    public class OpenMenuButton : CustomButton
    {
        [Required] [SerializeField] private MenuIdentifier menuToOpen;
        [Tooltip("Optional. The menu that stays registered as parent of the opened menu.")]
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