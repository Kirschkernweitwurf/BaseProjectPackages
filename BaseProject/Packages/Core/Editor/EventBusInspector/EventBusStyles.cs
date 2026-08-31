using Base.EditorUiPackage;
using UnityEngine;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// What the <see cref="EventBusWindow"/> needs on top of the shared list window look: the widths
    /// its own columns start at, its minimum size, the nesting offset of a subscriber row, and the
    /// fills of the badges that only mean something for a subscription.
    /// </summary>
    /// <remarks>
    /// Everything else, from the card and the striped rows to the ping button and the empty state,
    /// comes from <see cref="EditorTableStyles"/>, which reads it all from the theme assigned in the
    /// Editor UI Theme settings page.
    /// <para>
    /// Building and releasing are inherited: the window calls <c>EnsureBuilt</c> at the top of
    /// <c>OnGUI</c> and <c>Dispose</c> from <c>OnDisable</c>.
    /// </para>
    /// </remarks>
    internal sealed class EventBusStyles : EditorTableStyles
    {
        /// <summary>Width of the toolbar dropdown that picks between several buses.</summary>
        internal const float BusPopupWidth = 200f;

        /// <summary>Width the Handler column starts at before the user drags it.</summary>
        internal const float DefaultHandlerWidth = 180f;

        /// <summary>Width the Event column starts at before the user drags it.</summary>
        internal const float DefaultSubscriberWidth = 230f;

        private const float GuideAlpha = 0.16f;
        private const float LeakRowAlpha = 0.07f;

        /// <summary>Smallest height of the window.</summary>
        internal const float MinWindowHeight = 280f;

        /// <summary>Smallest width of the window, enough for every column plus the button.</summary>
        internal const float MinWindowWidth = 700f;

        /// <summary>Fill of the badge carrying an event's subscriber count.</summary>
        internal static Color CountBadgeColor => BadgeFill(EditorPalette.Accent);

        /// <summary>Fill of the badge on a subscription whose object was destroyed.</summary>
        internal static Color DestroyedBadgeColor => DangerBadgeColor;

        /// <summary>Background of an event row, which reads as a header for the rows under it.</summary>
        internal static Color GroupColor => HeaderColor;

        /// <summary>The vertical line that ties a subscriber row back to its event.</summary>
        internal static Color GuideColor => EditorPalette.Tint(GuideAlpha);

        /// <summary>
        /// Horizontal offset a subscriber row sits at under its event, and the width the expand
        /// arrow takes on the event row above it. They are one number on purpose: it is what lines
        /// an event name up with the column header and with the subscriber names below it.
        /// </summary>
        internal static float Indent => EditorMetrics.Indent;

        /// <summary>Tint laid over a row that holds or contains a leaked subscription.</summary>
        internal static Color LeakRowColor => EditorPalette.WithAlpha(EditorPalette.Danger, LeakRowAlpha);

        /// <summary>Fill of the badge on a subscription whose object is still alive.</summary>
        internal static Color LiveBadgeColor => OkBadgeColor;
    }
}