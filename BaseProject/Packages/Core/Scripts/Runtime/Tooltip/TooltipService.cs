using Base.CorePackage.Services;
using Base.CorePackage.Tracking;
using Base.UtilityPackage.Logging;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Shows the highest priority tooltip that is currently requested.
    /// Backed by a <see cref="PriorityTracker{T}"/>, so overlapping requests resolve by priority.
    /// </summary>
    public class TooltipService : GameServiceBehaviour
    {
        private readonly PriorityTracker<TooltipData> _tracker = new();

        private TooltipView _view;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            _tracker.OnCurrentActiveItemChanged += OnTooltipChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _tracker.OnCurrentActiveItemChanged -= OnTooltipChanged;
        }
#endregion

        /// <summary>
        /// Registers the view that draws the tooltips. Called by the <see cref="TooltipView"/> itself.
        /// </summary>
        /// <param name="view">The view to draw with.</param>
        public void SetView(TooltipView view)
        {
            if (view == null)
            {
                CustomLogger.LogError($"{nameof(SetView)} was called with a null view.", this);
                return;
            }

            _view = view;
        }

        /// <summary>
        /// Adds a tooltip request. A caller can only hold one request at a time.
        /// </summary>
        /// <param name="data">The tooltip to display.</param>
        /// <param name="priority">Higher values win over lower ones.</param>
        /// <param name="caller">The object requesting the tooltip, used for tracking and removal.</param>
        public void AddTooltip(TooltipData data, uint priority, object caller) => _tracker.Add(data, priority, caller);

        /// <summary>
        /// Removes the request of the given caller.
        /// </summary>
        /// <param name="caller">The object that requested the tooltip.</param>
        public void RemoveTooltip(object caller) => _tracker.Remove(caller);

        /// <summary>
        /// Checks whether a caller currently holds a tooltip request.
        /// </summary>
        /// <param name="caller">The object to check.</param>
        /// <returns><c>true</c> when a request from that caller exists; otherwise <c>false</c>.</returns>
        public bool HasTooltipFromCaller(object caller) => _tracker.HasCaller(caller);

        /// <summary>
        /// Pushes the new top request into the view. Stays silent while no view is registered,
        /// which is the normal state before the tooltip canvas loads and after it is unloaded.
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