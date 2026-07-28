using Base.AttributePackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Base for components that run their own logic when the attached <see cref="Button"/> is clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public abstract class CustomButton : MonoBehaviour
    {
        [Tooltip("The button this component listens to. Auto-assigned from the same GameObject when empty.")]
        [GetComponent] [Required] [SerializeField] protected Button button;

#region Unity Callbacks
        protected virtual void Awake() => button.onClick.AddListener(OnClick);

        protected virtual void OnDestroy() => button.onClick.RemoveListener(OnClick);
#endregion

        /// <summary>
        /// Called on click of the button.
        /// </summary>
        protected abstract void OnClick();
    }
}