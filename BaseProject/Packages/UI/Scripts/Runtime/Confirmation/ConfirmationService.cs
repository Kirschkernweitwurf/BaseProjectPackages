using System.Threading.Tasks;
using Base.AttributePackage;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.MenuManaging.Identifier;
using Base.CorePackage.Services;
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
        [Required] [SerializeField] private MenuIdentifier confirmationMenuIdentifier;

        private ConfirmationMenu _menu;
        private TaskCompletionSource<bool> _activeRequest;

#region Unity Callbacks
        private void Start()
        {
            if (!ServiceLocator.TryGet(out MenuManager manager))
                return;

            if (!manager.TryGetMenu(confirmationMenuIdentifier, out Menu foundMenu))
                return;

            if (foundMenu is ConfirmationMenu confirmationMenu)
                _menu = confirmationMenu;
            else
                CustomLogger.LogError($"Menu with Confirmation identifier is not of type {nameof(ConfirmationMenu)}. "
                    + "Ensure it is registered correctly", this);
        }
#endregion

        /// <summary>
        /// Shows a confirmation popup and awaits the user's response.
        /// Only one confirmation can be active at a time; concurrent requests are denied.
        /// </summary>
        public async Task<bool> ShowConfirmationAsync(ConfirmationRequest request)
        {
            if (_menu == null)
            {
                CustomLogger.LogError("Confirmation menu not found.", this);
                return false;
            }

            if (_activeRequest != null)
            {
                CustomLogger.LogWarning("A confirmation is already being shown. Concurrent request denied.", this);
                return false;
            }

            _activeRequest = new TaskCompletionSource<bool>();

            _menu.Show(request.Message, request.ConfirmText, request.CancelText,
                onConfirm: () => _activeRequest.TrySetResult(true),
                onCancel: () => _activeRequest.TrySetResult(false));

            // Await until user confirms or cancels
            bool result = await _activeRequest.Task;

            _menu.Hide();
            _activeRequest = null;

            return result;
        }
    }
}
