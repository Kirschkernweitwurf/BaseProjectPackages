using System;
using Base.AttributePackage;
using Base.CorePackage.MenuManaging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.UIPackage.Confirmation
{
    /// <summary>
    /// Represents a menu that prompts the user for confirmation before proceeding with an action.
    /// </summary>
    public sealed class ConfirmationMenu : Menu
    {
        [Space]
        [Required] [SerializeField] private Button confirmButton;
        [Required] [SerializeField] private Button cancelButton;

        [Space]
        [Required] [SerializeField] private TMP_Text messageText;
        [Required] [SerializeField] private TMP_Text confirmButtonText;
        [Required] [SerializeField] private TMP_Text cancelButtonText;

        [Space]
        [SerializeField] private string defaultConfirmText = "Confirm";
        [SerializeField] private string defaultCancelText = "Cancel";

        private Action _onConfirm;
        private Action _onCancel;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            confirmButton.onClick.AddListener(HandleConfirm);
            cancelButton.onClick.AddListener(HandleCancel);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            confirmButton.onClick.RemoveListener(HandleConfirm);
            cancelButton.onClick.RemoveListener(HandleCancel);
        }
#endregion

        /// <summary>
        /// Displays the confirmation menu with the specified message and button texts.
        /// Executes the provided actions based on the user's choice.
        /// </summary>
        /// <param name="message">The message to display in the confirmation menu.</param>
        /// <param name="confirmText">The text for the confirm button.</param>
        /// <param name="cancelText">The text for the cancel button.</param>
        /// <param name="onConfirm">The action to execute if the user confirms.</param>
        /// <param name="onCancel">The action to execute if the user cancels.</param>
        public void Show(string message, string confirmText, string cancelText, Action onConfirm, Action onCancel)
        {
            messageText.text = message;
            confirmButtonText.text = string.IsNullOrEmpty(confirmText)
                ? defaultConfirmText
                : confirmText;

            cancelButtonText.text = string.IsNullOrEmpty(cancelText)
                ? defaultCancelText
                : cancelText;

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            Open();
        }

        /// <summary>
        /// Hides the confirmation menu and clears any assigned actions.
        /// </summary>
        public void Hide()
        {
            _onConfirm = null;
            _onCancel = null;

            Close();
        }

        private void HandleConfirm() => _onConfirm?.Invoke();

        private void HandleCancel() => _onCancel?.Invoke();
    }
}