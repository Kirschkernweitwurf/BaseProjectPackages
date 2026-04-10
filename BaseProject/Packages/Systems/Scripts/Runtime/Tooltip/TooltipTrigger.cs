using Systems.Services;
using Tracking;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utility.Logging;

namespace Systems.Tooltip
{
    /// <summary>
    /// The purpose of this class is to show a tooltip when hovering a GameObject with this component.
    /// It uses the <see cref="TooltipService"/> to manage the display of the tooltip based on priority.
    /// </summary>
    [DisallowMultipleComponent]
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea]
        [SerializeField] private string tooltipText;

        [Tooltip("Higher = more important (used when overlapping tooltips).")]
        [SerializeField] private EPriority priority;

        private TooltipService _service;

        private void Awake() => ServiceLocator.TryGet(out _service);

        private void OnDisable()
        {
            if (_service == null)
                return;

            if (_service.HasTooltipFromCaller(this))
                _service.RemoveTooltip(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_service == null)
                return;

            if (string.IsNullOrEmpty(tooltipText))
            {
                CustomLogger.LogWarning($"{nameof(TooltipTrigger)} on GameObject '{gameObject.name}' " +
                                        "has empty tooltip text.", this);
                return;
            }

            if (Mouse.current == null)
            {
                CustomLogger.LogWarning("Cannot show tooltip: Mouse input is not available.", this);
                return;
            }

            TooltipData data = new(tooltipText, () => Mouse.current.position.ReadValue());

            _service.AddTooltip(data, (uint)priority, this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_service == null)
                return;

            if (_service.HasTooltipFromCaller(this))
                _service.RemoveTooltip(this);
        }

        /// <summary>
        /// Updates the tooltip text displayed by this trigger.
        /// </summary>
        public void SetText(string newText)
        {
            if (_service == null)
                return;

            tooltipText = newText;

            // If the tooltip is currently visible, refresh it
            if (!_service.HasTooltipFromCaller(this))
                return;

            if (Mouse.current == null)
            {
                CustomLogger.LogWarning("Cannot refresh tooltip: Mouse input is not available.", this);
                return;
            }

            _service.RemoveTooltip(this);

            TooltipData data = new(tooltipText, () => Mouse.current.position.ReadValue());
            _service.AddTooltip(data, (uint)priority, this);
        }
    }
}