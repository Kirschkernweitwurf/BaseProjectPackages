using Base.DataHubPackage.Editor.Table;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Lists
{
    /// <summary>
    /// One line of the Contents and Used In tabs: the asset with its icon, an optional detail after it,
    /// the count stepper and, where it makes sense, a button that removes every copy.
    /// </summary>
    internal static class DataListRowGui
    {
        private const string EmptySlotLabel = "Empty slot";
        private const string NameTooltip = "Click to find it in the Project window.";
        private const string RemoveTooltip = "Take every copy out of this list.";
        private const float RowGap = 2f;
        private const float RowInset = 8f;
        private const float StepperInset = 1f;

        /// <summary>Height of one line, the gap under it included.</summary>
        internal static float Height => EditorTableStyles.RowHeight + RowGap;

        /// <summary>Takes the space of one line from the layout and draws it.</summary>
        /// <param name="asset">The asset of the line, or null for empty slots.</param>
        /// <param name="detail">Dimmed text after the name, or null.</param>
        /// <param name="count">How often the asset is in the list.</param>
        /// <param name="isMixed">True when the selected assets are in the list a different number of times.</param>
        /// <param name="canRemove">True to offer the button that removes every copy.</param>
        /// <param name="styles">The built window styles.</param>
        /// <param name="isStep">True when minus or plus was pressed, see <see cref="DataHubGui.Stepper"/>.</param>
        /// <returns>The count after this pass, the same value when nothing changed.</returns>
        internal static int DrawLayout(Object asset, string detail, int count, bool isMixed, bool canRemove,
            DataHubStyles styles, out bool isStep)
        {
            Rect line = GUILayoutUtility.GetRect(0f, Height, GUILayout.ExpandWidth(true));
            Rect row = new(line.x, line.y, line.width, EditorTableStyles.RowHeight);

            if (row.Contains(Event.current.mousePosition))
                DataHubGui.Highlight(row, EditorPalette.Hover, styles);

            Rect content = new(row.x + RowInset, row.y, Mathf.Max(0f, row.width - RowInset * 2f), row.height);
            float right = content.xMax;

            if (canRemove)
            {
                Rect remove = new(right - DataHubStyles.IconButtonWidth, content.y, DataHubStyles.IconButtonWidth,
                    content.height);

                if (DataHubGui.IconButton(remove, DataHubIcons.Delete, RemoveTooltip, styles))
                {
                    isStep = false;
                    return 0;
                }

                right = remove.x - EditorMetrics.TightGap;
            }

            Rect stepper = new(right - DataHubStyles.StepperWidth, content.y + StepperInset,
                DataHubStyles.StepperWidth, content.height - StepperInset * 2f);

            int result = DataHubGui.Stepper(stepper, count, isMixed, styles, out isStep);

            Rect name = new(content.x, content.y, Mathf.Max(0f, stepper.x - EditorMetrics.ItemGap - content.x),
                content.height);

            DrawName(name, asset, detail, styles);

            return Mathf.Max(0, result);
        }

        private static void DrawName(Rect area, Object asset, string detail, DataHubStyles styles)
        {
            if (asset == null)
            {
                GUI.Label(area, EmptySlotLabel, styles.Detail);
                return;
            }

            // Measured, so the detail sits right after the name instead of at a fixed column. Measured with
            // the plain label, because a style that clips with an ellipsis measures as nothing.
            float nameWidth = EditorTableStyles.IconSize + EditorTableStyles.IconGap
                + EditorStyles.label.CalcSize(new GUIContent(asset.name)).x;

            Rect name = new(area.x, area.y, Mathf.Min(area.width, nameWidth), area.height);
            DataCell.DrawAssetName(name, asset, styles.LinkName, null);

            // The whole name is the button, so it is easy to hit.
            if (GUI.Button(name, new GUIContent(string.Empty, NameTooltip), GUIStyle.none))
                EditorGUIUtility.PingObject(asset);

            EditorGUIUtility.AddCursorRect(name, MouseCursor.Link);

            if (string.IsNullOrEmpty(detail)
                || name.xMax >= area.xMax)
                return;

            Rect rest = new(name.xMax + EditorMetrics.ItemGap, area.y,
                Mathf.Max(0f, area.xMax - name.xMax - EditorMetrics.ItemGap), area.height);

            GUI.Label(rest, detail, styles.Detail);
        }
    }
}