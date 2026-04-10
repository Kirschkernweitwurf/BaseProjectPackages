using Systems.MenuManaging;
using Systems.Services;
using UnityEngine;

namespace UI.Buttons
{
    /// <summary>
    /// Opens the selected menu on button click.
    /// </summary>
    public class OpenMenuButton : CustomButton
    {
        [SerializeField] private EMenuIdentifier menuToOpen;
        [SerializeField] private EMenuIdentifier parentMenuIdentifier;

        protected override void OnClick()
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (!menuManager.IsMenuOpen(menuToOpen))
                menuManager.OpenMenu(menuToOpen, parentMenuIdentifier);
        }
    }
}