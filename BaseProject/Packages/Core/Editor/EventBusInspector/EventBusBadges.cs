using UnityEngine;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// What a handler's state looks like in the table: the word, the tooltip behind it and the color
    /// of the pill it sits in.
    /// <para>
    /// The four states are the whole point of the window, so the wording that explains each of them is
    /// kept together rather than spread across the places that draw it. The report reads the same text
    /// as the pill for that reason.
    /// </para>
    /// </summary>
    internal static class EventBusBadges
    {
        private const string LeakCountFormat = "{0} of {1} leaked";

        private static readonly GUIContent DestroyedContent = new("Destroyed",
            "The object this handler runs on was destroyed but never unsubscribed. It still fires on "
            + "every publish and keeps the destroyed object alive.");
        private static readonly GUIContent LiveContent = new("Live",
            "The handler runs on a Unity object that is still alive.");
        private static readonly GUIContent PlainContent = new("Object",
            "The handler runs on a plain C# object. Unity does not manage its lifetime, so whether this "
            + "is a leak depends on who owns it.");
        private static readonly GUIContent StaticContent = new("Static",
            "The handler is a static method, so it has no instance that could outlive its subscription.");

        // The badge column is measured from these rather than from the rows, so its width cannot
        // depend on how many subscribers happen to be listed. Declared after the four it holds,
        // because static field initializers run in the order they are written.
        /// <summary>Every badge the state column can show, for measuring the column it sits in.</summary>
        internal static readonly GUIContent[] StateBadges =
        {
            DestroyedContent,
            LiveContent,
            PlainContent,
            StaticContent
        };

        /// <summary>The badge shown for a handler in the given state.</summary>
        /// <param name="state">The state of the handler.</param>
        /// <returns>The badge text and the tooltip that explains it.</returns>
        internal static GUIContent StateContent(EHandlerState state) => state switch
        {
            EHandlerState.Destroyed => DestroyedContent,
            EHandlerState.Live => LiveContent,
            EHandlerState.Static => StaticContent,
            _ => PlainContent
        };

        /// <summary>The color the pill behind the badge is tinted with.</summary>
        /// <param name="state">The state of the handler.</param>
        /// <returns>The fill color of the pill.</returns>
        internal static Color StateColor(EHandlerState state) => state switch
        {
            EHandlerState.Destroyed => EventBusStyles.DestroyedBadgeColor,
            EHandlerState.Live => EventBusStyles.LiveBadgeColor,
            _ => EventBusStyles.NeutralBadgeColor
        };

        /// <summary>
        /// The subscriber count of an event, which reads as a plain number until one of them leaked.
        /// </summary>
        /// <param name="entry">The event to count the subscribers of.</param>
        /// <returns>The count, or the leaked share of it.</returns>
        internal static string CountText(EventTypeEntry entry) => entry.HasLeaks
            ? string.Format(LeakCountFormat, entry.LeakCount, entry.Handlers.Count)
            : entry.Handlers.Count.ToString();

        /// <summary>The same count, for the summary bar that counts across every event at once.</summary>
        /// <param name="leaked">How many subscriptions leaked.</param>
        /// <param name="total">How many subscriptions there are.</param>
        /// <returns>The leaked share of the total.</returns>
        internal static string LeakText(int leaked, int total) => string.Format(LeakCountFormat, leaked, total);
    }
}