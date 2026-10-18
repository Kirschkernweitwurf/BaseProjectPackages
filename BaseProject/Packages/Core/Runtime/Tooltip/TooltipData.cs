using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Immutable payload of a single tooltip request: the panels to show and where to draw them.
    /// </summary>
    public readonly struct TooltipData
    {
        /// <summary>Panels shown side by side, in this order. Never null.</summary>
        public IReadOnlyList<TooltipEntry> Entries { get; }

        /// <summary>Screen position the tooltip follows. Queried every frame while visible.</summary>
        public Func<Vector2> GetScreenPosition { get; }

        /// <summary>Creates a tooltip payload with a single text panel.</summary>
        /// <param name="message">Text to show.</param>
        /// <param name="getScreenPosition">Supplies the screen position to follow.</param>
        public TooltipData(string message, Func<Vector2> getScreenPosition)
            : this(new[] { new TooltipEntry(message) }, getScreenPosition)
        {
        }

        /// <summary>Creates a tooltip payload with several panels.</summary>
        /// <param name="entries">Panels to show. Copied, so the caller may reuse its list.</param>
        /// <param name="getScreenPosition">Supplies the screen position to follow.</param>
        public TooltipData(IEnumerable<TooltipEntry> entries, Func<Vector2> getScreenPosition)
        {
            Entries = entries == null
                ? Array.Empty<TooltipEntry>()
                : new List<TooltipEntry>(entries);

            GetScreenPosition = getScreenPosition;
        }
    }
}