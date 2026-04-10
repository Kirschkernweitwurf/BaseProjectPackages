using Systems.Services;
using Tracking;

namespace Systems.Tooltip
{
    /// <summary>
    /// Manages Tooltips shown on screen, ensuring the highest priority tooltip is displayed.
    /// Uses a <see cref="PriorityTracker{T}"/> to handle multiple tooltip requests.
    /// </summary>
    public class TooltipService : GameServiceBehaviour
    {
        private readonly PriorityTracker<TooltipData> _tracker = new();
        private TooltipView _view;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _tracker.OnCurrentActiveItemChanged -= OnTooltipChanged;
        }

        /// <summary>
        /// Sets the view responsible for displaying tooltips.
        /// </summary>
        /// <param name="view"></param>
        public void SetView(TooltipView view)
        {
            _view = view;
            _tracker.OnCurrentActiveItemChanged += OnTooltipChanged;
        }

        /// <summary>
        /// Adds a tooltip with the specified priority and caller.
        /// If a tooltip from the same caller already exists, it will be updated.
        /// </summary>
        /// <param name="data">The tooltip data to display></param>
        /// <param name="priority">The priority of the tooltip (higher values take precedence)</param>
        /// <param name="caller">The object requesting the tooltip (used for tracking/removal)</param>
        public void AddTooltip(TooltipData data, uint priority, object caller) => _tracker.Add(data, priority, caller);

        /// <summary>
        /// Removes the tooltip associated with the specified caller.
        /// If the caller has no associated tooltip, this does nothing.
        /// </summary>
        /// <param name="caller">The object that requested the tooltip</param>
        public void RemoveTooltip(object caller) => _tracker.Remove(caller);

        /// <summary>
        /// Checks if there is a tooltip currently registered from the specified caller.
        /// </summary>
        /// <param name="caller">The object to check for an associated tooltip</param>
        /// <returns><c>true</c> if a tooltip from the caller exists; otherwise, <c>false</c></returns>
        public bool HasTooltipFromCaller(object caller) => _tracker.HasCaller(caller);

        /// <summary>
        /// Called when the currently active tooltip changes.
        /// Updates the view to show or hide the tooltip as needed.
        /// </summary>
        private void OnTooltipChanged(TrackedItem<TooltipData> item)
        {
            if (_view == null)
                return;

            if (item == null)
                _view.Hide();
            else
                _view.Show(item.Item);
        }
    }
}