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
        protected Button button;

#region Unity Callbacks
        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        protected virtual void OnDestroy() => button.onClick.RemoveListener(OnClick);
#endregion

        /// <summary>
        /// Called on click of the button
        /// </summary>
        protected abstract void OnClick();
    }
}