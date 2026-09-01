using System;
using System.Collections.Generic;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// One group of the window: an event type together with the subscribers currently combined into
    /// its multicast delegate, in the order the bus would invoke them.
    /// </summary>
    internal sealed class EventTypeEntry
    {
        private const string MissingValue = "-";

        /// <summary>The event type this group was built for.</summary>
        internal Type EventType { get; }

        /// <summary>Short name of <see cref="EventType"/>.</summary>
        internal string TypeName { get; }

        /// <summary>Namespace of <see cref="EventType"/>, or a dash when it has none.</summary>
        internal string NamespaceName { get; }

        /// <summary>The subscribers, in invocation order.</summary>
        internal IReadOnlyList<HandlerEntry> Handlers { get; }

        /// <summary>How many subscribers run on an object that was already destroyed.</summary>
        internal int LeakCount { get; }

        /// <summary>True when at least one subscription outlived its subscriber.</summary>
        internal bool HasLeaks => LeakCount > 0;

        /// <summary>Creates the group for a single event type.</summary>
        /// <param name="eventType">The type the handlers are filed under.</param>
        /// <param name="combined">The multicast delegate every subscriber was combined into.</param>
        internal EventTypeEntry(Type eventType, Delegate combined)
        {
            EventType = eventType;
            TypeName = eventType.Name;

            NamespaceName = string.IsNullOrEmpty(eventType.Namespace)
                ? MissingValue
                : eventType.Namespace;

            List<HandlerEntry> handlers = new();
            int leaks = 0;

            // The bus removes an entry as soon as its last handler goes, so a null delegate should
            // never reach this. Tolerated rather than trusted, because the window reads live state.
            if (combined != null)
                foreach (Delegate handler in combined.GetInvocationList())
                {
                    HandlerEntry entry = new(handler);

                    handlers.Add(entry);

                    if (entry.IsLeak)
                        leaks++;
                }

            Handlers = handlers;
            LeakCount = leaks;
        }

        /// <summary>
        /// Reports whether this group survives the given search term. A group is kept when the event
        /// itself matches or when any of its subscribers does, so a search for a class name finds
        /// every event that class listens to.
        /// </summary>
        /// <param name="search">The term typed into the toolbar. An empty term matches everything.</param>
        /// <returns><c>true</c> when the group should stay in the list.</returns>
        internal bool Matches(string search)
        {
            if (string.IsNullOrEmpty(search))
                return true;

            if (Contains(TypeName, search)
                || Contains(NamespaceName, search))
                return true;

            foreach (HandlerEntry handler in Handlers)
            {
                if (handler.Matches(search))
                    return true;
            }

            return false;
        }

        private static bool Contains(string value, string search)
            => value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}