using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// Owns the table's column widths and the draggable lines between them, and hands back one
    /// rectangle per column for a given row, so the header, the event rows and the subscriber rows
    /// under them share the exact same x positions.
    /// <para>
    /// Event and Handler are resizable and remembered between sessions. Target takes whatever is
    /// left. The badge and the ping button are pinned to the right. An event row fills
    /// <see cref="Content"/> with its name instead of splitting the three text columns.
    /// </para>
    /// </summary>
    internal sealed class EventBusColumns
    {
        private const string HandlerWidthKey = "Base.CorePackage.EventBus.Columns.Handler";
        private const float MinTextColumnWidth = 70f;
        private const string SubscriberWidthKey = "Base.CorePackage.EventBus.Columns.Subscriber";
        private const float TextPadding = 4f;

        // What the user dragged to, kept apart from what is actually drawn: a narrow window clamps
        // the drawn widths, and folding that back into the saved ones would lose the real widths.
        private float _savedHandlerWidth;
        private float _savedSubscriberWidth;

        private float _badgeWidth;
        private float _handlerWidth;
        private float _subscriberWidth;
        private float _targetWidth;

        private EEventDivider _dragging = EEventDivider.None;

        /// <summary>Restores the column widths the user last dragged to.</summary>
        internal EventBusColumns()
        {
            _savedSubscriberWidth = EditorPrefs.GetFloat(SubscriberWidthKey, EventBusStyles.DefaultSubscriberWidth);
            _savedHandlerWidth = EditorPrefs.GetFloat(HandlerWidthKey, EventBusStyles.DefaultHandlerWidth);
        }

        /// <summary>
        /// Recomputes the widths to fill the space available. Call once per frame before any
        /// rectangle is asked for.
        /// </summary>
        /// <param name="row">A full row rectangle, used for its width.</param>
        /// <param name="badgeWidth">The width the window measured for the badge column.</param>
        internal void Recalculate(Rect row, float badgeWidth)
        {
            _badgeWidth = badgeWidth;

            float flexible = Mathf.Max(0f, FlexibleWidth(row));

            _subscriberWidth = Mathf.Max(_savedSubscriberWidth, MinTextColumnWidth);
            _handlerWidth = Mathf.Max(_savedHandlerWidth, MinTextColumnWidth);
            _targetWidth = flexible - _subscriberWidth - _handlerWidth;

            if (_targetWidth >= MinTextColumnWidth)
                return;

            // Not enough room for Target: take it back from Handler first, then Event.
            float deficit = MinTextColumnWidth - _targetWidth;

            deficit -= Reclaim(ref _handlerWidth, deficit);
            deficit -= Reclaim(ref _subscriberWidth, deficit);

            _targetWidth = Mathf.Max(MinTextColumnWidth, flexible - _subscriberWidth - _handlerWidth);
        }

        /// <summary>Everything left of the badge, used whole by an event row.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The content cell.</returns>
        internal Rect Content(Rect row) => Cell(row, Left(row, 0f),
            Mathf.Max(0f, Badge(row).x - EventBusStyles.BadgeGap - Left(row, 0f)));

        /// <summary>The cell holding the event type, or the subscribing type one level in.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <param name="indent">How far the text columns are pushed in for a nested row.</param>
        /// <returns>The subscriber cell.</returns>
        internal Rect Subscriber(Rect row, float indent)
            => Cell(row, Left(row, indent), Mathf.Max(0f, _subscriberWidth - indent));

        /// <summary>The cell holding the subscribed method.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The handler cell.</returns>
        /// <remarks>Takes no indent: only the first column steps in, the rest stay in their column.</remarks>
        internal Rect Method(Rect row) => Cell(row, SubscriberEdge(row), _handlerWidth);

        /// <summary>The cell holding the object a handler runs on.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The target cell.</returns>
        internal Rect Target(Rect row) => Cell(row, MethodEdge(row), _targetWidth);

        /// <summary>The cell holding the state pill or the handler count.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The badge cell.</returns>
        internal Rect Badge(Rect row) => new(Ping(row).x - EventBusStyles.BadgeGap - _badgeWidth, row.y,
            _badgeWidth, row.height);

        /// <summary>The cell holding the ping button.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The ping cell.</returns>
        internal Rect Ping(Rect row) => new(row.xMax - EventBusStyles.RowInset - EventBusStyles.PingButtonWidth,
            row.y, EventBusStyles.PingButtonWidth, row.height);

        /// <summary>
        /// Whether a point is close enough to a divider to grab it. The header uses this to leave
        /// those few pixels to the drag instead of treating the press as a click on the title.
        /// </summary>
        /// <param name="point">The point to test, usually the mouse position.</param>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns><c>true</c> when a press there would start a resize.</returns>
        internal bool IsOverDivider(Vector2 point, Rect row)
            => IsNear(point.x, SubscriberEdge(row)) || IsNear(point.x, MethodEdge(row));

        /// <summary>
        /// Draws the divider lines across the whole table and processes any resize drag.
        /// </summary>
        /// <param name="area">The full table area the lines span.</param>
        internal void DrawAndProcessDividers(Rect area)
        {
            HandleDivider(EEventDivider.SubscriberHandler, SubscriberEdge(area), area);
            HandleDivider(EEventDivider.HandlerTarget, MethodEdge(area), area);
        }

        private static bool IsNear(float x, float dividerX)
            => Mathf.Abs(x - dividerX) <= EditorMetrics.DividerHitWidth * 0.5f;

        private static float Left(Rect row, float indent) => row.x + EventBusStyles.RowInset + indent;

        // Text starts a few pixels after the boundary rather than against it, so a column never
        // looks like it is touching the divider line in front of it.
        private static Rect Cell(Rect row, float start, float width)
            => new(start + TextPadding, row.y, Mathf.Max(0f, width - TextPadding * 2f), row.height);

        // Takes as much of the deficit out of one column as its minimum allows.
        private static float Reclaim(ref float width, float deficit)
        {
            if (deficit <= 0f)
                return 0f;

            float available = Mathf.Min(deficit, width - MinTextColumnWidth);

            if (available <= 0f)
                return 0f;

            width -= available;

            return available;
        }

        private float SubscriberEdge(Rect row) => Left(row, 0f) + _subscriberWidth;

        private float MethodEdge(Rect row) => SubscriberEdge(row) + _handlerWidth;

        private float FlexibleWidth(Rect row) => row.xMax - EventBusStyles.RowInset
            - EventBusStyles.PingButtonWidth - EventBusStyles.BadgeGap - _badgeWidth - EventBusStyles.BadgeGap
            - Left(row, 0f);

        private void HandleDivider(EEventDivider divider, float x, Rect area)
        {
            Rect line = new(x - EditorMetrics.DividerThickness * 0.5f, area.y, EditorMetrics.DividerThickness,
                area.height);
            Rect hit = new(x - EditorMetrics.DividerHitWidth * 0.5f, area.y, EditorMetrics.DividerHitWidth,
                area.height);

            EditorGUIUtility.AddCursorRect(hit, MouseCursor.ResizeHorizontal);

            Event current = Event.current;

            switch (current.type)
            {
                case EventType.Repaint:
                    EditorGUI.DrawRect(line, _dragging == divider
                        ? EditorPalette.Accent
                        : EditorPalette.Divider);

                    break;

                case EventType.MouseDown when hit.Contains(current.mousePosition):
                    _dragging = divider;
                    current.Use();

                    break;

                case EventType.MouseDrag when _dragging == divider:
                    Resize(divider, current.mousePosition.x, area);
                    current.Use();

                    break;

                case EventType.MouseUp when _dragging == divider:
                    _dragging = EEventDivider.None;
                    Save();
                    current.Use();

                    break;
            }
        }

        private void Resize(EEventDivider divider, float mouseX, Rect area)
        {
            if (divider == EEventDivider.SubscriberHandler)
            {
                _savedSubscriberWidth = Mathf.Max(MinTextColumnWidth, mouseX - Left(area, 0f));
                return;
            }

            _savedHandlerWidth = Mathf.Max(MinTextColumnWidth, mouseX - SubscriberEdge(area));
        }

        private void Save()
        {
            EditorPrefs.SetFloat(SubscriberWidthKey, _savedSubscriberWidth);
            EditorPrefs.SetFloat(HandlerWidthKey, _savedHandlerWidth);
        }
    }
}