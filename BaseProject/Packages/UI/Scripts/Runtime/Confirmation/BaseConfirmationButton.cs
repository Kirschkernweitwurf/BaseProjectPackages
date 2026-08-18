using System;
using System.Threading.Tasks;
using Base.AttributePackage;
using Base.ServicePackage;
using Base.UIPackage.Buttons;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UIPackage.Confirmation
{
    /// <summary>
    /// Provides generic usage to request a confirmation of the player on button click.
    /// </summary>
    public abstract class BaseConfirmationButton : CustomButton
    {
        [TextArea] [NotNullOrEmpty] [SerializeField] private string warningText;

        [Tooltip("Optional. Empty uses the default text of the confirmation menu.")]
        [SerializeField] private string confirmText;

        [Tooltip("Optional. Empty uses the default text of the confirmation menu.")]
        [SerializeField] private string cancelText;

        /// <summary>
        /// Displays the confirmation menu to the player prompting them with the given message and actions.
        /// Depending on their answer, will call <see cref="OnConfirm"/> or <see cref="OnCancel"/>.
        /// </summary>
        protected void ShowConfirmationBox() => _ = ShowConfirmationBoxAsync();

        /// <summary>
        /// Called when the player confirms the given prompt.
        /// </summary>
        protected virtual void OnConfirm() { }

        /// <summary>
        /// Called when the player cancels the given prompt.
        /// </summary>
        protected virtual void OnCancel() { }

        private async Task ShowConfirmationBoxAsync()
        {
            try
            {
                if (!ServiceLocator.TryGet(out ConfirmationService confirmationService))
                    return;

                ConfirmationRequest confirmationRequest = new(warningText, confirmText, cancelText);
                if (await confirmationService.ShowConfirmationAsync(confirmationRequest))
                    OnConfirm();
                else
                    OnCancel();
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"An error occurred while requesting confirmation: {e}", this);
            }
        }
    }
}