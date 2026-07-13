using Base.AttributePackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Provides basic functionality for calling functions on click of a button
    /// </summary>
    [RequireComponent(typeof(Button))]
    public abstract class CustomButton : MonoBehaviour
    {
        /// <summary>
        /// The button component that will be used to call the OnClick method
        /// </summary>
        [Tooltip("The button component that will be used to call the OnClick method."
            + " It automatically gets assigned if possible.")]
        [GetComponent] [SerializeField] protected Button button;

#region Unity Callbacks
        protected virtual void Awake() => button.onClick.AddListener(OnClick);

        protected virtual void OnDestroy() => button.onClick.RemoveListener(OnClick);
#endregion

        /// <summary>
        /// Called on click of the button
        /// </summary>
        protected abstract void OnClick();
    }
}