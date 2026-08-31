using Base.EditorUiPackage;
using UnityEngine;

namespace Base.ServicePackage.Editor
{
    /// <summary>
    /// What the <see cref="ServiceLocatorWindow"/> needs on top of the shared list window look: the
    /// widths its own columns start at, its minimum size, and the fills of the badges that only mean
    /// something for a registered service.
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
    internal sealed class ServiceLocatorStyles : EditorTableStyles
    {
        /// <summary>Width the Instance column starts at before the user drags it.</summary>
        internal const float DefaultInstanceWidth = 170f;

        /// <summary>Width the Service column starts at before the user drags it.</summary>
        internal const float DefaultServiceWidth = 210f;

        /// <summary>Smallest height of the window.</summary>
        internal const float MinWindowHeight = 260f;

        /// <summary>Smallest width of the window, enough for every column plus the button.</summary>
        internal const float MinWindowWidth = 660f;

        private const float ProblemRowAlpha = 0.06f;

        /// <summary>Fill of the badge on an entry that is fine.</summary>
        internal static Color AliveBadgeColor => OkBadgeColor;

        /// <summary>Fill of the badge on an entry whose instance was destroyed.</summary>
        internal static Color DestroyedBadgeColor => DangerBadgeColor;

        /// <summary>Fill of the badge on an entry filed under a type its instance does not implement.</summary>
        internal static Color MismatchBadgeColor => WarningBadgeColor;

        /// <summary>
        /// Tint laid over a row that reports a problem. Fainter than a badge, because it covers the
        /// whole row and still has to let the striping and the selection through.
        /// </summary>
        internal static Color ProblemRowColor => EditorPalette.WithAlpha(EditorPalette.Danger, ProblemRowAlpha);
    }
}