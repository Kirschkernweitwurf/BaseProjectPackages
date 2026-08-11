using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a toolbar that breaks onto further rows when its entries do not fit the available width.
    /// </summary>
    /// <remarks>
    /// Unity's own toolbar divides the width evenly and lets the labels truncate, so a narrow inspector
    /// turns a row of readable tabs into a row of two-letter stubs with no way to tell them apart. This
    /// measures each entry, packs as many as fit onto a row, and starts a new one when the next would
    /// not. Entries on a row still share its width evenly, which keeps the buttons aligned.
    /// <para>
    /// A single entry wider than the inspector still truncates. Nothing can be done about that short of
    /// scrolling, and a tab bar that scrolls sideways is worse than one that clips.
    /// </para>
    /// </remarks>
    internal static class WrappedToolbar
    {
        private const float ButtonPadding = 10f;
        private const float RowGap = 2f;

        // Reused between draws so packing a bar does not allocate per repaint. A tab bar is drawn on
        // every repaint of every inspector showing one, which is often enough for it to matter.
        private static readonly List<int> RowStarts = new();

        private static string[] _rowBuffer = Array.Empty<string>();

        /// <summary>Draws the toolbar and returns the selected index.</summary>
        /// <param name="selected">The currently selected index.</param>
        /// <param name="labels">The entry labels.</param>
        /// <param name="width">The width available to the bar.</param>
        /// <returns>The index the user picked.</returns>
        public static int Draw(int selected, string[] labels, float width)
        {
            if (labels.Length == 0)
                return selected;

            Pack(labels, width);

            if (RowStarts.Count <= 1)
                return GUILayout.Toolbar(selected, labels);

            int result = selected;

            for (int row = 0; row < RowStarts.Count; row++)
            {
                int start = RowStarts[row];
                int end = row + 1 < RowStarts.Count
                    ? RowStarts[row + 1]
                    : labels.Length;

                int picked = DrawRow(selected, labels, start, end);
                if (picked >= 0)
                    result = picked;

                if (row + 1 < RowStarts.Count)
                    GUILayout.Space(RowGap);
            }

            return result;
        }

        // A row is its own toolbar, so the selection has to be translated into and out of row space. An
        // index of minus one means the selected entry is not on this row, which Unity's toolbar accepts.
        private static int DrawRow(int selected, string[] labels, int start, int end)
        {
            int count = end - start;

            // Unity's toolbar takes an array, so the row is copied into a buffer that grows once and is
            // then reused rather than allocated per row per repaint.
            if (_rowBuffer.Length != count)
                _rowBuffer = new string[count];

            for (int i = 0; i < count; i++)
                _rowBuffer[i] = labels[start + i];

            int local = selected >= start && selected < end
                ? selected - start
                : -1;

            int picked = GUILayout.Toolbar(local, _rowBuffer);

            return picked >= 0 && picked != local
                ? start + picked
                : -1;
        }

        private static void Pack(string[] labels, float width)
        {
            RowStarts.Clear();
            RowStarts.Add(0);

            float used = 0f;

            for (int i = 0; i < labels.Length; i++)
            {
                float entry = EditorStyles.miniButton.CalcSize(ScratchContent.For(labels[i])).x + ButtonPadding;

                // The first entry of a row always goes on it, even when it is wider than the row itself,
                // because moving it to the next one would leave an empty row and not help.
                if (used > 0f && used + entry > width)
                {
                    RowStarts.Add(i);
                    used = entry;
                    continue;
                }

                used += entry;
            }
        }
    }
}