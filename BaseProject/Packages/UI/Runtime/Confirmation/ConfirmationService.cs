using System.Threading.Tasks;
using Base.AttributesPackage;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.MenuManaging.Identifier;
using Base.ServicesPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UIPackage.Confirmation
{
    /// <summary>
    /// A globally accessible service for showing confirmation prompts.
    /// Works asynchronously and can be awaited.
    /// </summary>
    public sealed class ConfirmationService : GameServiceBehaviour
    {
        [Required] [SerializeField] private MenuIdentifier confirmationMenuIdentifier;

        private ConfirmationMenu _menu;
        private TaskCompletionSource<bool> _activeRequest;

#region Unity Callbacks
        private void Start()
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (!menuManager.TryGetMenu(confirmationMenuIdentifier, out Menu foundMenu))
                return;

            if (foundMenu is not ConfirmationMenu confirmationMenu)
            {
                CustomLogger.LogError($"The registered confirmation menu is not of type {nameof(ConfirmationMenu)}. "
                    + "Ensure it is registered correctly.", this);

                return;
            }

            _menu = confirmationMenu;
        }
#endregion

        /// <summary>
        /// Shows a confirmation popup and awaits the user's response.
        /// Only one confirmation can be active at a time; concurrent requests are denied.
        /// </summary>
        /// <param name="request">The message and the optional button labels.</param>
        /// <returns><c>true</c> if the user confirmed, otherwise <c>false</c>.</returns>
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

            // Kept local so a late callback of a closed prompt cannot touch the next request
            TaskCompletionSource<bool> completionSource = new();
            _activeRequest = completionSource;

            try
            {
                _menu.Show(request,
                    onConfirm: () => completionSource.TrySetResult(true),
                    onCancel: () => completionSource.TrySetResult(false));

                return await completionSource.Task;
            }
            finally
            {
                _menu.Hide();
                _activeRequest = null;
            }
        }
    }
}