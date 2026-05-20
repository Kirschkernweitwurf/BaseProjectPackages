using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.SystemsCorePackage.Tweening.Components.System
{
    /// <summary>
    /// Triggers a TweenGroup based on UI events (hover and click).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField, Tooltip("The type of UI event to listen for.")]
        private EUIEventType eventType;

        [SerializeField, Tooltip("The group of tweens to play when the event is triggered.")]
        private TweenGroup tweenGroup;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventType == EUIEventType.OnHover && tweenGroup != null)
                tweenGroup.Play();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventType == EUIEventType.OnHover && tweenGroup != null)
                tweenGroup.Reverse();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventType == EUIEventType.OnClick && tweenGroup != null)
                tweenGroup.Play();
        }
    }
}