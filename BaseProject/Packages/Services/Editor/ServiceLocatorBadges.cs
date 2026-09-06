using Base.EditorUIPackage.Editor;
using UnityEngine;

namespace Base.ServicesPackage.Editor
{
    /// <summary>
    /// What a registration's state looks like in the table: the word, the tooltip behind it, the color
    /// of the pill it sits in and the icon that carries the same meaning without color.
    /// <para>
    /// The three states are the whole point of the window, so the wording that explains each of them
    /// is kept together rather than spread across the places that draw it. The report reads the same
    /// text as the pill for that reason.
    /// </para>
    /// </summary>
    internal static class ServiceLocatorBadges
    {
        private static readonly GUIContent AliveContent = new("Alive",
            "The instance is usable and implements the type it is filed under.");
        private static readonly GUIContent DestroyedContent = new("Destroyed",
            "The instance was destroyed without deregistering. The next lookup logs an error and drops "
            + "the entry.");
        private static readonly GUIContent MismatchContent = new("Mismatch",
            "The instance does not implement the type it is filed under, so every lookup for that type "
            + "fails.");

        // The badge column is measured from these rather than from the rows, so its width cannot
        // depend on how many services happen to be registered. Declared after the three it holds,
        // because static field initializers run in the order they are written.
        /// <summary>Every badge the state column can show, for measuring the column it sits in.</summary>
        internal static readonly GUIContent[] StateBadges =
        {
            AliveContent,
            DestroyedContent,
            MismatchContent
        };

        /// <summary>The badge shown for a registration in the given state.</summary>
        /// <param name="state">The state of the registration.</param>
        /// <returns>The badge text and the tooltip that explains it.</returns>
        internal static GUIContent StateContent(EServiceState state) => state switch
        {
            EServiceState.Destroyed => DestroyedContent,
            EServiceState.Mismatch => MismatchContent,
            _ => AliveContent
        };

        /// <summary>The color the pill behind the badge is tinted with.</summary>
        /// <param name="state">The state of the registration.</param>
        /// <returns>The fill color of the pill.</returns>
        internal static Color StateColor(EServiceState state) => state switch
        {
            EServiceState.Destroyed => ServiceLocatorStyles.DestroyedBadgeColor,
            EServiceState.Mismatch => ServiceLocatorStyles.MismatchBadgeColor,
            _ => ServiceLocatorStyles.AliveBadgeColor
        };

        /// <summary>
        /// The icon a problem row carries, so it stays recognizable for anyone who cannot separate the
        /// row tints by color alone. A healthy row has none.
        /// </summary>
        /// <param name="state">The state of the registration.</param>
        /// <returns>The icon, or null when the registration is healthy.</returns>
        internal static Texture StateIcon(EServiceState state) => state switch
        {
            EServiceState.Destroyed => EditorIcons.Error,
            EServiceState.Mismatch => EditorIcons.Warning,
            _ => null
        };
    }
}