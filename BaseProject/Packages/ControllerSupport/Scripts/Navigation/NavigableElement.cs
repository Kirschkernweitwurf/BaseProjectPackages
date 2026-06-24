using System;
using Base.SystemsCorePackage.Tweening.Components.System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupport.Navigation
{
    /// <summary>
    /// Makes any sibling <see cref="Selectable"/> drive a focus <see cref="TweenGroup"/> and raise a
    /// submit event, without subclassing it. Because the EventSystem dispatches select, deselect and
    /// submit to every matching component on the selected object, this works as a plain sibling. The
    /// Selectable can be a bare Selectable, a Button, a Toggle or any other, so the one-selectable-per-
    /// object limit never gets in the way.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public sealed class NavigableElement : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        /// <summary>Raised when the element is confirmed (Submit action).</summary>
        public event Action OnSubmitted;

        [Tooltip("Optional tween group shown on focus and hidden on blur.")]
        [SerializeField] private TweenGroup focusTweenGroup;

        /// <summary>The sibling selectable that makes this element navigable.</summary>
        public Selectable Selectable { get; private set; }

#region Unity Callbacks
        private void Awake() => Selectable = GetComponent<Selectable>();
#endregion

        public void OnDeselect(BaseEventData eventData)
        {
            if (focusTweenGroup != null)
                focusTweenGroup.Hide();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (focusTweenGroup != null)
                focusTweenGroup.Show();
        }

        public void OnSubmit(BaseEventData eventData) => OnSubmitted?.Invoke();
    }
}