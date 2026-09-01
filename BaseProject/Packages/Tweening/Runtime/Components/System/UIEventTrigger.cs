using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.TweeningPackage.Components.System
{
    /// <summary>
    /// Triggers a TweenGroup based on UI events (hover and click).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [SerializeField] [Tooltip("The type of UI event to listen for.")]
        private EUIEventType eventType;

        [SerializeField] [Tooltip("The group of tweens to play when the event is triggered.")]
        private TweenGroup tweenGroup;

        /// <summary>Hides the group when it is set to react to selection. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnDeselect(BaseEventData eventData)
        {
            if (eventType == EUIEventType.OnSelect && tweenGroup != null)
                tweenGroup.Hide();
        }

        /// <summary>Shows the group when it is set to react to a click. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventType == EUIEventType.OnClick && tweenGroup != null)
                tweenGroup.Show();
        }

        /// <summary>Shows the group when it is set to react to hover. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventType == EUIEventType.OnHover && tweenGroup != null)
                tweenGroup.Show();
        }

        /// <summary>Hides the group when it is set to react to hover. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventType == EUIEventType.OnHover && tweenGroup != null)
                tweenGroup.Hide();
        }

        /// <summary>Shows the group when it is set to react to selection. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnSelect(BaseEventData eventData)
        {
            if (eventType == EUIEventType.OnSelect && tweenGroup != null)
                tweenGroup.Show();
        }

        /// <summary>Shows the group when it is set to react to submit. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnSubmit(BaseEventData eventData)
        {
            if (eventType == EUIEventType.OnSubmit && tweenGroup != null)
                tweenGroup.Show();
        }
    }
}