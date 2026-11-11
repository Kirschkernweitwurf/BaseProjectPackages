using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.TweeningPackage.Components.System
{
    /// <summary>
    /// Triggers a TweenGroup based on UI events (hover and click). An element whose <see cref="Selectable"/> is not
    /// interactable does not show the group, so a disabled button does not light up as if it could be used. Leaving
    /// it still hides the group, so a button that gets disabled while hovered lets go.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [SerializeField] [Tooltip("The type of UI event to listen for.")]
        private EUIEventType eventType;

        [SerializeField] [Tooltip("The group of tweens to play when the event is triggered.")]
        private TweenGroup tweenGroup;

        private Selectable _selectable;

#region Unity Callbacks
        // Optional: an element without a Selectable, like a plain image, always reacts.
        private void Awake() => TryGetComponent(out _selectable);
#endregion

        /// <summary>Hides the group when it is set to react to selection. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnDeselect(BaseEventData eventData) => Hide(EUIEventType.OnSelect);

        /// <summary>Shows the group when it is set to react to a click. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnPointerClick(PointerEventData eventData) => Show(EUIEventType.OnClick);

        /// <summary>Shows the group when it is set to react to hover. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnPointerEnter(PointerEventData eventData) => Show(EUIEventType.OnHover);

        /// <summary>Hides the group when it is set to react to hover. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnPointerExit(PointerEventData eventData) => Hide(EUIEventType.OnHover);

        /// <summary>Shows the group when it is set to react to selection. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnSelect(BaseEventData eventData) => Show(EUIEventType.OnSelect);

        /// <summary>Shows the group when it is set to react to submit. Any other event type is ignored.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event kind matters.</param>
        public void OnSubmit(BaseEventData eventData) => Show(EUIEventType.OnSubmit);

        // The event system still sends pointer and select events to a disabled Selectable.
        private void Show(EUIEventType type)
        {
            if (type != eventType
                || tweenGroup == null
                || !IsInteractable())
                return;

            tweenGroup.Show();
        }

        private void Hide(EUIEventType type)
        {
            if (type != eventType
                || tweenGroup == null)
                return;

            tweenGroup.Hide();
        }

        // Also false while a parent CanvasGroup is not interactable, like a menu that is still fading in.
        private bool IsInteractable() => _selectable == null || _selectable.IsInteractable();
    }
}