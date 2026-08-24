using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.ServicePackage.Editor
{
    /// <summary>
    /// Owns the table's column widths and the draggable lines between them, and hands back one
    /// rectangle per column for a given row, so the header and every row below it share the exact
    /// same x positions.
    /// <para>
    /// Service and Instance are resizable and remembered between sessions. Location takes whatever
    /// is left. The state pill and the ping button are pinned to the right at a width the window
    /// measures from its own content.
    /// </para>
    /// </summary>
    internal sealed class ServiceLocatorColumns
    {
        private const string InstanceWidthKey = "Base.ServicePackage.Columns.Instance";
        private const float MinTextColumnWidth = 70f;
        private const string ServiceWidthKey = "Base.ServicePackage.Columns.Service";
        private const float TextPadding = 4f;

        // What the user dragged to, kept apart from what is actually drawn: a narrow window clamps
        // the drawn widths, and folding that back into the saved ones would lose the real widths.
        private float _savedInstanceWidth;
        private float _savedServiceWidth;

        private float _badgeWidth;
        private float _instanceWidth;
        private float _locationWidth;
        private float _serviceWidth;

        private EServiceDivider _dragging = EServiceDivider.None;

        /// <summary>Restores the column widths the user last dragged to.</summary>
        internal ServiceLocatorColumns()
        {
            _savedServiceWidth = EditorPrefs.GetFloat(ServiceWidthKey, ServiceLocatorStyles.DefaultServiceWidth);
            _savedInstanceWidth = EditorPrefs.GetFloat(InstanceWidthKey, ServiceLocatorStyles.DefaultInstanceWidth);
        }

        /// <summary>
        /// Recomputes the widths to fill the space available. Call once per frame before any
        /// rectangle is asked for.
        /// </summary>
        /// <param name="row">A full row rectangle, used for its width.</param>
        /// <param name="badgeWidth">The width the window measured for the state column.</param>
        internal void Recalculate(Rect row, float badgeWidth)
        {
            _badgeWidth = badgeWidth;

            float flexible = Mathf.Max(0f, FlexibleWidth(row));

            _serviceWidth = Mathf.Max(_savedServiceWidth, MinTextColumnWidth);
            _instanceWidth = Mathf.Max(_savedInstanceWidth, MinTextColumnWidth);
            _locationWidth = flexible - _serviceWidth - _instanceWidth;

            if (_locationWidth >= MinTextColumnWidth)
                return;

            // Not enough room for Location: take it back from Instance first, then Service.
            float deficit = MinTextColumnWidth - _locationWidth;

            deficit -= Reclaim(ref _instanceWidth, deficit);
            deficit -= Reclaim(ref _serviceWidth, deficit);

            _locationWidth = Mathf.Max(MinTextColumnWidth,
                flexible - _serviceWidth - _instanceWidth);
        }

        /// <summary>The cell holding the type the service is filed under.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The service cell.</returns>
        internal Rect Service(Rect row) => Cell(row, Left(row), _serviceWidth);

        /// <summary>The cell holding the type the instance actually is.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The instance cell.</returns>
        internal Rect Instance(Rect row) => Cell(row, ServiceEdge(row), _instanceWidth);

        /// <summary>The cell holding the game object and scene.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The location cell.</returns>
        internal Rect Location(Rect row) => Cell(row, InstanceEdge(row), _locationWidth);

        /// <summary>The cell holding the state pill.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The state cell.</returns>
        internal Rect State(Rect row) => new(Ping(row).x - ServiceLocatorStyles.BadgeGap - _badgeWidth, row.y,
            _badgeWidth, row.height);

        /// <summary>The cell holding the ping button.</summary>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns>The ping cell.</returns>
        internal Rect Ping(Rect row) => new(row.xMax - ServiceLocatorStyles.RowInset
            - ServiceLocatorStyles.PingButtonWidth, row.y, ServiceLocatorStyles.PingButtonWidth, row.height);

        /// <summary>
        /// Whether a point is close enough to a divider to grab it. The header uses this to leave
        /// those few pixels to the drag instead of treating the press as a click on the title.
        /// </summary>
        /// <param name="point">The point to test, usually the mouse position.</param>
        /// <param name="row">The row the columns are laid out in.</param>
        /// <returns><c>true</c> when a press there would start a resize.</returns>
        internal bool IsOverDivider(Vector2 point, Rect row)
            => IsNear(point.x, ServiceEdge(row)) || IsNear(point.x, InstanceEdge(row));

        /// <summary>
        /// Draws the divider lines across the whole table and processes any resize drag.
        /// </summary>
        /// <param name="area">The full table area the lines span.</param>
        internal void DrawAndProcessDividers(Rect area)
        {
            HandleDivider(EServiceDivider.ServiceInstance, ServiceEdge(area), area);
            HandleDivider(EServiceDivider.InstanceLocation, InstanceEdge(area), area);
        }

        private static bool IsNear(float x, float dividerX)
            => Mathf.Abs(x - dividerX) <= EditorMetrics.DividerHitWidth * 0.5f;

        private static float Left(Rect row) => row.x + ServiceLocatorStyles.RowInset;

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

        private float ServiceEdge(Rect row) => Left(row) + _serviceWidth;

        private float InstanceEdge(Rect row) => ServiceEdge(row) + _instanceWidth;

        private float FlexibleWidth(Rect row) => row.xMax - ServiceLocatorStyles.RowInset
            - ServiceLocatorStyles.PingButtonWidth - ServiceLocatorStyles.BadgeGap - _badgeWidth
            - ServiceLocatorStyles.BadgeGap - Left(row);

        private void HandleDivider(EServiceDivider divider, float x, Rect area)
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
                    _dragging = EServiceDivider.None;
                    Save();
                    current.Use();

                    break;
            }
        }

        private void Resize(EServiceDivider divider, float mouseX, Rect area)
        {
            if (divider == EServiceDivider.ServiceInstance)
            {
                _savedServiceWidth = Mathf.Max(MinTextColumnWidth, mouseX - Left(area));
                return;
            }

            _savedInstanceWidth = Mathf.Max(MinTextColumnWidth, mouseX - ServiceEdge(area));
        }

        private void Save()
        {
            EditorPrefs.SetFloat(ServiceWidthKey, _savedServiceWidth);
            EditorPrefs.SetFloat(InstanceWidthKey, _savedInstanceWidth);
        }
    }
}