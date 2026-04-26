using System.Threading.Tasks;
using Base.SystemsCorePackage.MenuManaging;
using Base.SystemsCorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UIPackage.Confirmation
{
    /// <summary>
    /// A globally accessible service for showing confirmation prompts.
    /// Works asynchronously and can be awaited.
    /// </summary>
    public class ConfirmationService : GameServiceBehaviour
    {
        [SerializeField] private MenuIdentifier confirmationMenuIdentifier;

        private ConfirmationMenu _menu;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out MenuManager manager))
                return;

            if (!manager.TryGetMenu(confirmationMenuIdentifier, out Menu foundMenu))
                return;

            if (foundMenu is ConfirmationMenu confirmationMenu)
                _menu = confirmationMenu;
            else
                CustomLogger.LogError("Menu with Confirmation identifier is not of type ConfirmationMenu. " +
                                      "Ensure it is registered correctly", this);
        }

        /// <summary>
        /// Shows a confirmation popup and awaits the user's response.
        /// </summary>
        public async Task<bool> ShowConfirmationAsync(ConfirmationRequest request)
        {
            if (_menu == null)
            {
                CustomLogger.LogError("Confirmation menu not found.", this);
                return false;
            }

            TaskCompletionSource<bool> tcs = new();

            _menu.Show(request.Message, request.ConfirmText, request.CancelText,
                onConfirm: () => tcs.TrySetResult(true),
                onCancel: () => tcs.TrySetResult(false)
            );

            // Await until user confirms or cancels
            bool result = await tcs.Task;

            _menu.Hide();
            return result;
        }
    }
}