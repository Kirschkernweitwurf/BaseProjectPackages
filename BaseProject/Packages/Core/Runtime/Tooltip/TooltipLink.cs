using System;
using System.Collections.Generic;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Something a tooltip's text names, shown beside the tooltip as a picture of its own, with its own panels in a
    /// card next to the picture, like the card a rule deals: the text says its name, the link shows what it is and
    /// explains its terms. A <see cref="TooltipSubjectView"/> in the view draws the picture.
    /// </summary>
    public readonly struct TooltipLink
    {
        /// <summary>
        /// What the link shows, handed to the view's <see cref="TooltipSubjectView"/>. Null for no picture.
        /// </summary>
        public object Subject { get; }

        /// <summary>Panels in the card beside the picture, in this order. Never null; empty hides the card.</summary>
        public IReadOnlyList<TooltipEntry> Entries { get; }

        /// <summary>Creates a link with a picture and the panels beside it.</summary>
        /// <param name="subject">What the picture shows. Null for none.</param>
        /// <param name="entries">Panels beside the picture. Copied, so the caller may reuse it. Null for none.</param>
        public TooltipLink(object subject, IEnumerable<TooltipEntry> entries = null)
        {
            Subject = subject;
            Entries = entries == null
                ? Array.Empty<TooltipEntry>()
                : new List<TooltipEntry>(entries);
        }
    }
}