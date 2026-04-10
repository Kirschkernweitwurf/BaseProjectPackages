using UnityEngine;
using UnityEngine.UI;

namespace UI.Buttons
{
    /// <summary>
    /// Provides basic functionality for calling functions on click of a button
    /// </summary>
    public abstract class CustomButton : MonoBehaviour
    {
        [Tooltip("The button component to listen for clicks on")]
        [SerializeField] protected Button button;

        protected virtual void Awake() => button.onClick.AddListener(OnClick);

        protected virtual void OnDestroy() => button.onClick.RemoveListener(OnClick);

        /// <summary>
        /// Called on click of the button
        /// </summary>
        protected abstract void OnClick();
    }
}