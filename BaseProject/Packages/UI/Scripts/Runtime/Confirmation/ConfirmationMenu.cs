using System;
using Systems.MenuManaging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Confirmation
{
    /// <summary>
    /// Represents a menu that prompts the user for confirmation before proceeding with an action.
    /// </summary>
    public sealed class ConfirmationMenu : Menu
    {
        [Space]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [Space]
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text confirmButtonText;
        [SerializeField] private TMP_Text cancelButtonText;

        [Space]
        [SerializeField] private string defaultConfirmText = "Confirm";
        [SerializeField] private string defaultCancelText = "Cancel";

        private Action _onConfirm;
        private Action _onCancel;

        protected override void Awake()
        {
            base.Awake();

            confirmButton.onClick.AddListener(() => _onConfirm?.Invoke());
            cancelButton.onClick.AddListener(() => _onCancel?.Invoke());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
        }

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
            confirmButtonText.text = string.IsNullOrEmpty(confirmText) ? defaultConfirmText : confirmText;
            cancelButtonText.text = string.IsNullOrEmpty(cancelText) ? defaultCancelText : cancelText;

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
    }
}