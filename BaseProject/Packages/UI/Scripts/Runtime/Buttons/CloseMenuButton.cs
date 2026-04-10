using Systems.MenuManaging;
using Systems.Services;
using UnityEngine;

namespace UI.Buttons
{
    /// <summary>
    /// Closes the selected menu on button click.
    /// </summary>
    public class CloseMenuButton : CustomButton
    {
        [SerializeField] private EMenuIdentifier menuToClose;

        protected override void OnClick()
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (menuManager.IsMenuOpen(menuToClose))
                menuManager.CloseMenu(menuToClose);
        }
    }
}