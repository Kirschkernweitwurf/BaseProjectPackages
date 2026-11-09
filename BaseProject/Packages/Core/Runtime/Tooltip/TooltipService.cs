using System;
using System.Collections.Generic;
using Base.ServicesPackage;
using Base.ServicesPackage.Tracking;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Shows the highest priority tooltip that is currently requested.
    /// Backed by a <see cref="PriorityTracker{T}"/>, so overlapping requests resolve by priority. Areas kept clear,
    /// like a marker that points at the target from the side, push a tooltip beside its target past them.
    /// </summary>
    public class TooltipService : GameServiceBehaviour
    {
        private readonly PriorityTracker<TooltipData> _tracker = new();
        private readonly Dictionary<object, Func<Rect>> _clearAreas = new();

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
        /// Keeps a tooltip that opens beside its target clear of an area running alongside the target, like a
        /// marker that points at the target from the side. The tooltip opens past the area instead of covering it.
        /// The area is read each time a tooltip is placed, so it may move. One area per owner; a second call replaces
        /// the first.
        /// </summary>
        /// <param name="owner">The object that keeps the area clear, used for release.</param>
        /// <param name="area">Returns the area in screen pixels.</param>
        public void KeepClear(object owner, Func<Rect> area)
        {
            if (owner == null
                || area == null)
            {
                CustomLogger.LogError($"{nameof(KeepClear)} was called without an owner or an area.", this);
                return;
            }

            _clearAreas[owner] = area;
        }

        /// <summary>
        /// Stops keeping the area of the given owner clear. Silent when it kept none, so a double release does not
        /// spam.
        /// </summary>
        /// <param name="owner">The object that called <see cref="KeepClear"/>.</param>
        public void ReleaseClear(object owner)
        {
            if (owner == null)
                return;

            _clearAreas.Remove(owner);
        }

        /// <summary>
        /// Widens the rect a tooltip opens beside over every kept-clear area that runs alongside it, so the tooltip
        /// opens past those areas.
        /// </summary>
        internal Rect WidenOverClearAreas(Rect anchor)
        {
            foreach (Func<Rect> getArea in _clearAreas.Values)
            {
                Rect area = getArea();
                if (area.yMax <= anchor.yMin
                    || area.yMin >= anchor.yMax)
                    continue;

                anchor.xMin = Mathf.Min(anchor.xMin, area.xMin);
                anchor.xMax = Mathf.Max(anchor.xMax, area.xMax);
            }

            return anchor;
        }

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