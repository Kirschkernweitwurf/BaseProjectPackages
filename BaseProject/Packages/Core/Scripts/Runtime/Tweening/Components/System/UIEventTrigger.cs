using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.CorePackage.Tweening.Components.System
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

        public void OnDeselect(BaseEventData eventData)
        {
            if (eventType == EUIEventType.OnSelect && tweenGroup != null)
                tweenGroup.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventType == EUIEventType.OnClick && tweenGroup != null)
                tweenGroup.Show();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventType == EUIEventType.OnHover && tweenGroup != null)
                tweenGroup.Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventType == EUIEventType.OnHover && tweenGroup != null)
                tweenGroup.Hide();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (eventType == EUIEventType.OnSelect && tweenGroup != null)
                tweenGroup.Show();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (eventType == EUIEventType.OnSubmit && tweenGroup != null)
                tweenGroup.Show();
        }
    }
}