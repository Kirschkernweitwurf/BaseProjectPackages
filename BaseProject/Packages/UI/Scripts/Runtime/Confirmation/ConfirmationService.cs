using System.Threading.Tasks;
using Systems.MenuManaging;
using Systems.Services;
using Utility.Logging;

namespace UI.Confirmation
{
    /// <summary>
    /// A globally accessible service for showing confirmation prompts.
    /// Works asynchronously and can be awaited.
    /// </summary>
    public class ConfirmationService : GameServiceBehaviour
    {
        private ConfirmationMenu _menu;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out MenuManager manager))
                return;

            if (!manager.TryGetMenu(EMenuIdentifier.Confirmation, out Menu foundMenu))
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