using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Immutable payload of a single tooltip request: the panels to show, what they explain and the screen rect to show
    /// them beside.
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

        /// <summary>
        /// What the panels explain, for a view with a header: its title, icon and eyebrow, like the card a set of
        /// keyword panels belongs to. Null when the panels speak for themselves.
        /// </summary>
        public TooltipEntry? Source { get; }

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
        /// <param name="source">What the panels explain, shown in the view's header. Null for none.</param>
        public TooltipData(IEnumerable<TooltipEntry> entries, Func<Rect> getScreenAnchor, TooltipEntry? source = null)
        {
            Entries = entries == null
                ? Array.Empty<TooltipEntry>()
                : new List<TooltipEntry>(entries);

            GetScreenAnchor = getScreenAnchor;
            Source = source;
        }

        private static Func<Rect> AnchorAt(Func<Vector2> getScreenPosition) => getScreenPosition == null
            ? null
            : () => TooltipAnchor.AtPoint(getScreenPosition());
    }
}