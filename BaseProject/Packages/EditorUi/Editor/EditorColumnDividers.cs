using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The draggable lines between the columns of a table: the line itself, the few pixels around it
    /// that can be grabbed, the resize cursor, and the press, drag and release that resize a column.
    /// </summary>
    /// <remarks>
    /// One instance per table, because only one of its dividers can be dragged at a time and that is
    /// the whole state this keeps. A divider is identified by any integer the table picks, usually the
    /// index of the column left of it, so a table needs no enum of its own for them.
    /// <para>
    /// What a width means is left to the caller. A column anchored to the left edge grows as the
    /// divider moves right, one anchored to the right edge grows as it moves left, and no shared
    /// class can know which without being told. So this reports where the mouse is and the table
    /// works out its own widths from that.
    /// </para>
    /// </remarks>
    public sealed class EditorColumnDividers
    {
        private const int NoDivider = -1;

        private int _dragging = NoDivider;

        /// <summary>
        /// Whether a point is close enough to a divider to grab it.
        /// </summary>
        /// <remarks>
        /// A header that is also a button has to ask this before treating a press as a click, or the
        /// few pixels of the divider sitting on top of the title become unusable.
        /// </remarks>
        /// <param name="pointX">The x to test, usually the mouse position.</param>
        /// <param name="dividerX">The x the divider is drawn at.</param>
        /// <returns>True when a press there would start a resize.</returns>
        public static bool IsOver(float pointX, float dividerX)
            => Mathf.Abs(pointX - dividerX) <= EditorMetrics.DividerHitWidth * 0.5f;

        /// <summary>
        /// Draws one divider and processes the event for it. Call once per divider per GUI pass.
        /// </summary>
        /// <param name="id">Any number identifying this divider within the table.</param>
        /// <param name="x">The x to draw it at.</param>
        /// <param name="area">The area it spans, usually the whole table so it can be grabbed at any row.</param>
        /// <param name="mouseX">Where the mouse is, meaningful only when the divider moved.</param>
        /// <returns>What happened, so the caller knows whether to resize and whether to save.</returns>
        public EEditorDividerAction Handle(int id, float x, Rect area, out float mouseX)
        {
            mouseX = 0f;

            // NoDivider is the idle marker, so a caller handing it out as an identifier would make
            // every divider look like it was already being dragged.
            if (id == NoDivider)
                return EEditorDividerAction.None;

            Rect hit = new(x - EditorMetrics.DividerHitWidth * 0.5f, area.y, EditorMetrics.DividerHitWidth,
                area.height);

            EditorGUIUtility.AddCursorRect(hit, MouseCursor.ResizeHorizontal);

            Event current = Event.current;

            switch (current.type)
            {
                case EventType.Repaint:
                    DrawLine(x, area, _dragging == id);
                    break;

                case EventType.MouseDown when hit.Contains(current.mousePosition):
                    _dragging = id;
                    current.Use();
                    break;

                case EventType.MouseDrag when _dragging == id:
                    mouseX = current.mousePosition.x;
                    current.Use();

                    return EEditorDividerAction.Dragged;

                case EventType.MouseUp when _dragging == id:
                    mouseX = current.mousePosition.x;
                    _dragging = NoDivider;
                    current.Use();

                    return EEditorDividerAction.Released;
            }

            return EEditorDividerAction.None;
        }

        /// <summary>
        /// Drops a drag in progress.
        /// </summary>
        /// <remarks>
        /// A button released outside the window never reports back, which leaves the table thinking
        /// the drag is still on and resizing on the next mouse move. A window can call this when it
        /// loses focus to get out of that.
        /// </remarks>
        public void Cancel() => _dragging = NoDivider;

        private static void DrawLine(float x, Rect area, bool isActive)
        {
            Rect line = new(x - EditorMetrics.DividerThickness * 0.5f, area.y, EditorMetrics.DividerThickness,
                area.height);

            Color color = isActive
                ? EditorPalette.Accent
                : EditorPalette.Divider;

            EditorGUI.DrawRect(line, color);
        }
    }
}