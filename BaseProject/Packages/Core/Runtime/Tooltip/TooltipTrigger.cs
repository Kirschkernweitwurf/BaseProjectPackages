using System.Collections.Generic;
using Base.ServicesPackage;
using Base.ServicesPackage.Tracking;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Shows a tooltip while the pointer hovers this GameObject: the text set in the inspector, followed by the
    /// panels of every <see cref="ITooltipProvider"/> on the same GameObject. Requests go through the
    /// <see cref="TooltipService"/>, so overlapping triggers resolve by priority.
    /// </summary>
    [DisallowMultipleComponent]
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Header above the text. Optional.")]
        [SerializeField] private string tooltipTitle;

        [Tooltip("Text shown while this GameObject is hovered. Optional when a provider on it adds the content.")]
        [TextArea]
        [SerializeField] private string tooltipText;

        [Tooltip("Higher wins when several tooltips are requested at the same time.")]
        [SerializeField] private EPriority priority;

        private readonly List<TooltipEntry> _entries = new();

        private ITooltipProvider[] _providers;
        private TooltipService _service;

#region Unity Callbacks
        private void Awake()
        {
            _providers = GetComponents<ITooltipProvider>();

            // TryGet logs on its own. Disabling keeps a missing service from repeating that error on every hover.
            if (!ServiceLocator.TryGet(out _service))
                enabled = false;
        }

        private void OnDisable() => HideTooltip();
#endregion

        /// <inheritdoc/>
        public void OnPointerEnter(PointerEventData eventData) => ShowTooltip();

        /// <inheritdoc/>
        public void OnPointerExit(PointerEventData eventData) => HideTooltip();

        /// <summary>
        /// Replaces the tooltip text and refreshes the tooltip when it is currently visible.
        /// </summary>
        /// <param name="newText">The new text. Must not be empty.</param>
        public void SetText(string newText)
        {
            if (string.IsNullOrEmpty(newText))
            {
                CustomLogger.LogError($"{nameof(SetText)} was called with empty text.", this);
                return;
            }

            tooltipText = newText;
            Refresh();
        }

        /// <summary>
        /// Builds the visible tooltip again, for providers whose content changed while it is hovered. Does nothing
        /// while the tooltip is hidden, since it is built fresh on the next hover anyway.
        /// </summary>
        public void Refresh()
        {
            if (!HasActiveTooltip())
                return;

            HideTooltip();
            ShowTooltip();
        }

        /// <summary>
        /// Requests the tooltip. Returns quietly when the service is already gone, which happens on teardown.
        /// </summary>
        private void ShowTooltip()
        {
            if (_service == null)
                return;

            CollectEntries();

            if (_entries.Count == 0)
            {
                CustomLogger.LogWarning($"{nameof(TooltipTrigger)} on '{gameObject.name}' has no tooltip content.",
                    this);

                return;
            }

            if (Mouse.current == null)
            {
                CustomLogger.LogWarning("Cannot show the tooltip, no mouse is available.", this);
                return;
            }

            TooltipData data = new(_entries, getScreenPosition: () => Mouse.current.position.ReadValue());
            _service.AddTooltip(data, (uint)priority, this);
        }

        // The inspector text comes first, so a provider can add details below a fixed description.
        private void CollectEntries()
        {
            _entries.Clear();

            if (!string.IsNullOrEmpty(tooltipText))
                _entries.Add(new TooltipEntry(tooltipTitle, tooltipText));

            foreach (ITooltipProvider provider in _providers)
                provider.CollectEntries(_entries);
        }

        /// <summary>
        /// Drops the request again. Silent when there is nothing to remove, so a double release does not spam.
        /// </summary>
        private void HideTooltip()
        {
            if (!HasActiveTooltip())
                return;

            _service.RemoveTooltip(this);
        }

        /// <summary>
        /// Checks whether this trigger currently holds a request in the service.
        /// </summary>
        private bool HasActiveTooltip() => _service != null && _service.HasTooltipFromCaller(this);
    }
}