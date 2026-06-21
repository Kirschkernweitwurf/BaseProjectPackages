using System;
using Base.SystemsCorePackage.Tweening.Components.System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupport.Navigation
{
    /// <summary>
    /// A drop-in focusable UI element. Subclasses <see cref="Selectable"/> so anything (not just
    /// buttons) can be navigated to, drives an optional focus <see cref="TweenGroup"/> on select and
    /// deselect, and raises <see cref="OnSubmitted"/> when confirmed via gamepad or keyboard.
    /// </summary>
    public class NavigableElement : Selectable, ISubmitHandler
    {
        /// <summary>Raised when the element is confirmed (Submit action).</summary>
        public event Action OnSubmitted;

        [Tooltip("Optional tween group shown on focus and hidden on blur.")]
        [SerializeField] private TweenGroup focusTweenGroup;

        public void OnSubmit(BaseEventData eventData) => OnSubmitted?.Invoke();

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (focusTweenGroup != null)
                focusTweenGroup.Show();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            if (focusTweenGroup != null)
                focusTweenGroup.Hide();
        }
    }
}