using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>
    /// Column widths of a table, with draggable dividers in the header. Widths are stored per
    /// column in EditorPrefs, so a layout survives restarts, and both tables of the window use the
    /// same class so they look and behave identically.
    /// </summary>
    internal sealed class AssetNamingColumnLayout
    {
        private const float DividerGrip = 6f;
        private const float FieldInset = 2f;
        private const float MinimumWidth = 24f;

        /// <summary>Horizontal offset right behind the last column, including padding.</summary>
        internal float TotalWidth
        {
            get
            {
                float total = AssetNamingGui.Padding;

                foreach (float width in Widths)
                    total += width + AssetNamingGui.Padding;

                return total;
            }
        }

        /// <summary>
        /// Stored widths, loaded on first use. Reading EditorPrefs in the constructor is not
        /// allowed, because a layout can be built from a static field of an EditorWindow.
        /// </summary>
        private float[] Widths
        {
            get
            {
                if (_widths != null)
                    return _widths;

                _widths = new float[_defaultWidths.Length];

                for (int index = 0; index < _defaultWidths.Length; index++)
                    _widths[index] = EditorPrefs.GetFloat(KeyOf(index), _defaultWidths[index]);

                return _widths;
            }
        }

        private readonly float[] _defaultWidths;
        private readonly string _prefsKey;

        private float[] _widths;

        /// <summary>Creates a layout with the widths it falls back to on first use.</summary>
        public AssetNamingColumnLayout(string prefsKey, params float[] defaultWidths)
        {
            _prefsKey = prefsKey;
            _defaultWidths = defaultWidths;
        }

        /// <summary>Full height rectangle of one cell, used for labels.</summary>
        internal Rect Cell(Rect row, int index)
        {
            float x = row.x + AssetNamingGui.Padding;

            for (int current = 0; current < index; current++)
                x += Widths[current] + AssetNamingGui.Padding;

            return new Rect(x, row.y, Widths[index], row.height);
        }

        /// <summary>Slightly inset rectangle of one cell, used for fields and buttons.</summary>
        internal Rect Field(Rect row, int index)
        {
            Rect cell = Cell(row, index);

            return new Rect(cell.x, cell.y + FieldInset, cell.width, cell.height - FieldInset * 2f);
        }

        /// <summary>Draws the titles and the drag handles. Returns true when a width changed.</summary>
        internal bool DrawHeader(Rect rect, GUIContent[] titles)
        {
            bool isChanged = false;

            for (int index = 0; index < Widths.Length; index++)
            {
                Rect cell = Cell(rect, index);

                GUI.Label(cell, titles[index], EditorStyles.miniBoldLabel);

                if (DrawDivider(rect, cell.xMax + AssetNamingGui.Padding * 0.5f, index))
                    isChanged = true;
            }

            return isChanged;
        }

        private string KeyOf(int index) => _prefsKey + index;

        /// <summary>Drag handle that resizes the column left of it.</summary>
        private bool DrawDivider(Rect headerRect, float x, int index)
        {
            Rect handle = new(x - DividerGrip * 0.5f, headerRect.y, DividerGrip, headerRect.height);
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            EventType type = Event.current.GetTypeForControl(controlId);

            EditorGUIUtility.AddCursorRect(handle, MouseCursor.ResizeHorizontal);

            if (type == EventType.Repaint)
            {
                EditorGUI.DrawRect(new Rect(x, headerRect.y + 3f, 1f, headerRect.height - 6f),
                    AssetNamingGui.DividerColor);

                return false;
            }

            if (type == EventType.MouseDown
                && handle.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = controlId;
                Event.current.Use();

                return false;
            }

            if (GUIUtility.hotControl != controlId)
                return false;

            if (type == EventType.MouseDrag)
            {
                Widths[index] = Mathf.Max(MinimumWidth, Widths[index] + Event.current.delta.x);
                Event.current.Use();

                return true;
            }

            if (type != EventType.MouseUp)
                return false;

            GUIUtility.hotControl = 0;
            EditorPrefs.SetFloat(KeyOf(index), Widths[index]);
            Event.current.Use();

            return true;
        }
    }
}