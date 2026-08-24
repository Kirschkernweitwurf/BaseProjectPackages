using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// A toolbar that wraps onto further rows instead of squeezing its buttons, drawn into a rect the
    /// caller reserves.
    /// </summary>
    /// <remarks>
    /// Split into packing and drawing so the caller can reserve the bar's space, draw something else,
    /// and then draw the bar last. A tab group needs exactly that: its block has to be painted before
    /// the bar so the bar ends up on top of it.
    /// </remarks>
    internal static class WrappedToolbar
    {
        private const float ButtonPadding = 10f;

        // Reused between draws so packing a bar does not allocate per repaint. A tab bar is drawn on
        // every repaint of every inspector showing one, which is often enough for it to matter.
        private static readonly List<int> RowStarts = new();

        private static string[] _rowBuffer = Array.Empty<string>();

        /// <summary>Works out how many rows the labels need at the given width.</summary>
        /// <param name="labels">The tab labels.</param>
        /// <param name="width">The width the bar has to fit into.</param>
        /// <returns>The number of rows, at least one.</returns>
        public static int Rows(string[] labels, float width)
        {
            Pack(labels, width);

            return Mathf.Max(RowStarts.Count, 1);
        }

        /// <summary>Draws the bar into an already reserved rect, using the last packing.</summary>
        /// <param name="area">The rect reserved for the whole bar.</param>
        /// <param name="selected">The index currently selected.</param>
        /// <param name="labels">The tab labels.</param>
        /// <returns>The index after any click, which is the given one when nothing was clicked.</returns>
        public static int DrawAt(Rect area, int selected, string[] labels)
        {
            if (labels.Length == 0 || RowStarts.Count == 0)
                return selected;

            float height = area.height / RowStarts.Count;
            int result = selected;

            for (int row = 0; row < RowStarts.Count; row++)
            {
                int start = RowStarts[row];
                int end = row + 1 < RowStarts.Count
                    ? RowStarts[row + 1]
                    : labels.Length;

                Rect rect = new(area.x, area.y + row * height, area.width, height);

                int picked = DrawRow(rect, selected, labels, start, end);
                if (picked >= 0)
                    result = picked;
            }

            return result;
        }

        // A row is its own toolbar, so the selection has to be translated into and out of row space. An
        // index of minus one means the selected entry is not on this row, which Unity's toolbar accepts.
        private static int DrawRow(Rect rect, int selected, string[] labels, int start, int end)
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

            int picked = GUI.Toolbar(rect, local, _rowBuffer);

            return picked >= 0 && picked != local
                ? start + picked
                : -1;
        }

        // Greedy packing: a label goes on the current row while it still fits, and opens a new one when
        // it does not. Good enough for a handful of tabs, and it keeps the reading order intact.
        private static void Pack(string[] labels, float width)
        {
            RowStarts.Clear();

            if (labels.Length == 0)
                return;

            RowStarts.Add(0);

            float used = 0f;

            for (int i = 0; i < labels.Length; i++)
            {
                float needed = EditorStyles.toolbarButton.CalcSize(ScratchContent.For(labels[i])).x
                    + ButtonPadding;

                if (used > 0f && used + needed > width)
                {
                    RowStarts.Add(i);
                    used = 0f;
                }

                used += needed;
            }
        }
    }
}