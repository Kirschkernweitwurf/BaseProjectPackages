using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer
{
    /// <summary>
    /// Draws the row of tabs across the top of the window.
    /// </summary>
    /// <remarks>
    /// Drawn by hand rather than through the built-in toolbar, which draws four equally wide segmented
    /// buttons across the whole window and reads as a control to press rather than as a place you are.
    /// An underline under the active tab is the convention the rest of the editor uses for exactly that.
    /// </remarks>
    internal static class AttributeExplorerTabBar
    {
        private const float LabelPadding = 18f;
        private const float MinimumTabWidth = 78f;
        private const float UnderlineHeight = 2f;

        private static readonly GUIContent Content = new();

        private static readonly string[] Labels =
        {
            "Reference",
            "Showcase",
            "Troubleshoot"
        };

        /// <summary>Draws the tabs and returns the one the user is on.</summary>
        /// <param name="area">The strip the tabs are drawn in.</param>
        /// <param name="current">The tab that is active now.</param>
        /// <param name="styles">The window styles.</param>
        /// <returns>The tab after the click, which equals the current one when nothing was clicked.</returns>
        internal static EAttributeExplorerTab Draw(Rect area, EAttributeExplorerTab current,
            AttributeExplorerStyles styles)
        {
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(area, styles.TabStrip);

                EditorGUI.DrawRect(new Rect(area.x, area.yMax - 1f, area.width, 1f), styles.Divider);
            }

            EAttributeExplorerTab picked = current;
            float x = area.x;

            for (int i = 0; i < Labels.Length; i++)
            {
                Content.text = Labels[i];

                float width = Mathf.Max(styles.Tab.CalcSize(Content).x + LabelPadding, MinimumTabWidth);
                Rect tab = new(x, area.y, width, area.height);
                bool active = (int)current == i;

                if (Draw(tab, active, styles))
                    picked = (EAttributeExplorerTab)i;

                x += width;
            }

            return picked;
        }

        private static bool Draw(Rect tab, bool active, AttributeExplorerStyles styles)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (active)
                {
                    EditorGUI.DrawRect(tab, styles.TabActive);

                    EditorGUI.DrawRect(new Rect(tab.x, tab.yMax - UnderlineHeight, tab.width,
                        UnderlineHeight), styles.Selection);
                }
                else if (tab.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(tab, styles.Hover);
                }
            }

            return GUI.Button(tab, Content, active
                ? styles.TabSelected
                : styles.Tab);
        }
    }
}