using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// Maps a keyword onto the color it is drawn in and onto the position of its tag, which is the
    /// order keywords are sorted and listed in. Built once per scan from the configured tags.
    /// </summary>
    internal sealed class TodoPalette
    {
        private const int Unknown = int.MaxValue;

        private readonly Dictionary<string, Color> _colors = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _order = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<TodoTag> _tags = new();

        /// <summary>The tags this palette was built from, in their configured order.</summary>
        internal IReadOnlyList<TodoTag> Tags => _tags;

        /// <summary>Fallback for a keyword whose tag was removed after the scan.</summary>
        private static Color Fallback => Color.gray;

        /// <summary>Builds a palette from the enabled tags.</summary>
        /// <param name="tags">The configured tags.</param>
        internal TodoPalette(IReadOnlyList<TodoTag> tags)
        {
            foreach (TodoTag tag in tags)
            {
                if (!tag.Enabled || string.IsNullOrWhiteSpace(tag.Keyword))
                    continue;

                _tags.Add(tag);
                _colors[tag.Keyword] = tag.Color;
                _order[tag.Keyword] = _order.Count;
            }
        }

        /// <summary>The color a keyword is drawn in.</summary>
        /// <param name="keyword">The keyword to look up.</param>
        /// <returns>The configured color, or gray when the keyword is unknown.</returns>
        internal Color Of(string keyword) => _colors.TryGetValue(keyword, out Color color)
            ? color
            : Fallback;

        /// <summary>The position of a keyword's tag, used to sort by keyword.</summary>
        /// <param name="keyword">The keyword to look up.</param>
        /// <returns>The zero based position, or a value that sorts unknown keywords last.</returns>
        internal int OrderOf(string keyword) => _order.TryGetValue(keyword, out int order)
            ? order
            : Unknown;
    }
}