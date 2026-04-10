using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Tweening.Components.System
{
    /// <summary>
    /// Triggers a TweenGroup based on UI events (hover and click).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Tooltip("The type of UI event to listen for.")]
        [SerializeField] private UIEventType eventType;

        [Tooltip("The group of tweens to play when the event is triggered.")]
        [SerializeField] private TweenGroup tweenGroup;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventType == UIEventType.OnHover && tweenGroup != null)
                tweenGroup.Play();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventType == UIEventType.OnHover && tweenGroup != null)
                tweenGroup.Reverse();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventType == UIEventType.OnClick && tweenGroup != null)
                tweenGroup.Play();
        }
    }
}