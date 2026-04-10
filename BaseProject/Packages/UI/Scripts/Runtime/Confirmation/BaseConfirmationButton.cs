using System;
using Systems.Services;
using UI.Buttons;
using UnityEngine;
using Utility.Logging;

namespace UI.Confirmation
{
    /// <summary>
    /// Provides generic usage to request a confirmation of the player on button click.
    /// </summary>
    public abstract class BaseConfirmationButton : CustomButton
    {
        [TextArea] [SerializeField] private string warningText;

        [SerializeField] private string confirmText;
        [SerializeField] private string cancelText;

        /// <summary>
        /// Displays the confirmation menu to the player prompting them with the given message and actions.
        /// Depending on their answer, will call <see cref="OnConfirm"/> or <see cref="OnCancel"/>
        /// </summary>
        protected async void ShowConfirmationBox()
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
                CustomLogger.LogError("An error occurred while requesting confirmation: " + e.Message, this);
            }
        }

        /// <summary>
        /// Called when the player confirms the given prompt.
        /// </summary>
        protected virtual void OnConfirm() { }

        /// <summary>
        /// Called when the player cancels the given prompts.
        /// </summary>
        protected virtual void OnCancel() { }
    }
}