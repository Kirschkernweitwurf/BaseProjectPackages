using Base.SystemsCorePackage.Tweening.Components.System;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SettingsPackage.GUI
{
    /// <summary>
    /// A button paired with an <see cref="TweenGroup"/> that shows or hides
    /// the group based on whether this button's option is currently selected.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class SelectionIndicatorButton : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TweenGroup tweenGroup;
        [SerializeField] private Button button;

        private Action _onClick;

        private void OnDestroy() => button.onClick.RemoveListener(HandleClick);

        /// <summary>Wires the click action and sets the initial selected state.</summary>
        public void Initialize(bool isSelected, Action onClick)
        {
            _onClick = onClick;
            button.onClick.AddListener(HandleClick);
            SetSelected(isSelected);
        }

        /// <summary>Shows or hides the indicator based on the selected state.</summary>
        public void SetSelected(bool isSelected) => tweenGroup.SetVisibility(isSelected);

        /// <summary>
        /// Unwires the click action and clears the reference to the click callback.
        /// </summary>
        public void Cleanup()
        {
            button.onClick.RemoveListener(HandleClick);
            _onClick = null;
        }

        private void HandleClick() => _onClick?.Invoke();
    }
}