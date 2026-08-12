using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Drag reordering for the rows the list drawer paints itself, which is what a paged list falls back
    /// to.
    /// </summary>
    /// <remarks>
    /// Unity's list draws every element or none, so a page cannot use it and the rows are drawn by hand.
    /// This puts the dragging back, and only there: a page is a contiguous window, so the row above on
    /// screen really is the element above in the array. A search filter is not contiguous, and dragging
    /// under one would move an element somewhere the pointer never went, so it stays off.
    /// <para>
    /// The state is static and keyed by list, because a drag outlives the call that started it and two
    /// inspectors can be open at once.
    /// </para>
    /// </remarks>
    internal static class PagedRowDrag
    {
        private const float GripWidth = 14f;
        private const float HandleHeight = 6f;
        private const float HandleInset = 2f;
        private const float LineThickness = 2f;

        private static GUIStyle DragHandle => _dragHandle ??= new GUIStyle("RL DragHandle");

        private static GUIStyle _dragHandle;

        private static string _activeKey;
        private static int _sourceIndex = -1;
        private static int _targetIndex = -1;

        /// <summary>Width a row leaves free on its left for the grip.</summary>
        internal static float ReservedWidth => GripWidth;

        /// <summary>Draws the grip of one row and handles the drag that may start on it.</summary>
        /// <param name="property">The array being drawn.</param>
        /// <param name="row">The rect the row occupies.</param>
        /// <param name="index">Index of the element in the array.</param>
        internal static void DrawGrip(SerializedProperty property, Rect row, int index)
        {
            Rect grip = new(row.x, row.y, GripWidth, row.height);
            string key = KeyFor(property);

            EditorGUIUtility.AddCursorRect(grip, MouseCursor.ResizeVertical);

            if (Event.current.type == EventType.Repaint)
            {
                // The style Unity's own list draws its handle with, so a paged row is gripped by the
                // same two bars as every other list rather than by something that merely resembles them.
                Rect handle = new(grip.x + HandleInset, grip.y + (grip.height - HandleHeight) * 0.5f,
                    grip.width - HandleInset * 2f, HandleHeight);

                DragHandle.Draw(handle, GUIContent.none, false, false, false, false);
            }

            Handle(key, grip, row, index, property);
        }

        /// <summary>Applies a finished drag. Call once after every row of the page has been drawn.</summary>
        /// <param name="property">The array being drawn.</param>
        internal static void Apply(SerializedProperty property)
        {
            if (_activeKey != KeyFor(property) || Event.current.type != EventType.MouseUp)
                return;

            if (_sourceIndex >= 0 && _targetIndex >= 0 && _sourceIndex != _targetIndex)
                property.MoveArrayElement(_sourceIndex, _targetIndex);

            Reset();
            Event.current.Use();
        }

        private static string KeyFor(SerializedProperty property)
            => property.serializedObject.targetObject.GetInstanceID() + property.propertyPath;

        private static void Handle(string key, Rect grip, Rect row, int index, SerializedProperty property)
        {
            Event current = Event.current;

            switch (current.type)
            {
                case EventType.MouseDown when current.button == 0 && grip.Contains(current.mousePosition):
                    _activeKey = key;
                    _sourceIndex = index;
                    _targetIndex = index;
                    current.Use();
                    break;

                // The pointer decides the target by which row it is over, so the drag follows the rows
                // rather than a pixel count, and a row of any height behaves the same.
                case EventType.MouseDrag when _activeKey == key && row.Contains(current.mousePosition):
                    _targetIndex = index;
                    current.Use();
                    break;

                case EventType.Repaint when _activeKey == key && _targetIndex == index:
                    DrawInsertionLine(row);
                    break;
            }
        }

        private static void DrawInsertionLine(Rect row)
        {
            float y = _targetIndex > _sourceIndex
                ? row.yMax
                : row.y;

            EditorGUI.DrawRect(new Rect(row.x, y - LineThickness * 0.5f, row.width, LineThickness),
                Color.white);
        }

        private static void Reset()
        {
            _activeKey = null;
            _sourceIndex = -1;
            _targetIndex = -1;
        }
    }
}