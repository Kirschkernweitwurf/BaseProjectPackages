using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Immutable payload of a single tooltip request: the panels to show and the screen rect to show them beside.
    /// </summary>
    public readonly struct TooltipData
    {
        /// <summary>Panels shown side by side, in this order. Never null.</summary>
        public IReadOnlyList<TooltipEntry> Entries { get; }

        /// <summary>
        /// Screen rect the tooltip is placed beside, queried every frame while visible. A zero-size rect is a
        /// point, like the cursor.
        /// </summary>
        public Func<Rect> GetScreenAnchor { get; }

        /// <summary>Creates a tooltip payload with a single text panel next to a point.</summary>
        /// <param name="message">Text to show.</param>
        /// <param name="getScreenPosition">Supplies the screen point to follow.</param>
        public TooltipData(string message, Func<Vector2> getScreenPosition)
            : this(new[] { new TooltipEntry(message) }, getScreenPosition)
        {
        }

        /// <summary>Creates a tooltip payload with several panels next to a point.</summary>
        /// <param name="entries">Panels to show. Copied, so the caller may reuse its list.</param>
        /// <param name="getScreenPosition">Supplies the screen point to follow.</param>
        public TooltipData(IEnumerable<TooltipEntry> entries, Func<Vector2> getScreenPosition)
            : this(entries, AnchorAt(getScreenPosition))
        {
        }

        /// <summary>Creates a tooltip payload with several panels beside a screen rect.</summary>
        /// <param name="entries">Panels to show. Copied, so the caller may reuse its list.</param>
        /// <param name="getScreenAnchor">Supplies the screen rect to place the tooltip beside.</param>
        public TooltipData(IEnumerable<TooltipEntry> entries, Func<Rect> getScreenAnchor)
        {
            Entries = entries == null
                ? Array.Empty<TooltipEntry>()
                : new List<TooltipEntry>(entries);

            GetScreenAnchor = getScreenAnchor;
        }

        private static Func<Rect> AnchorAt(Func<Vector2> getScreenPosition) => getScreenPosition == null
            ? null
            : () => TooltipAnchor.AtPoint(getScreenPosition());
    }
}