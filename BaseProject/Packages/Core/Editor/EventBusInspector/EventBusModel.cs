using System;
using System.Collections.Generic;
using UnityEngine;
using EventBusBehaviour = Base.CorePackage.EventBus.EventBus;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// Everything the window lists, read from the buses in the loaded scenes and reduced to the rows
    /// that get drawn.
    /// <para>
    /// Held apart from the window because it is rebuilt four times a second while play mode runs and
    /// the drawing is not. What the search box, the leak filter, the expansion set and the sort say is
    /// handed in rather than kept here, so this owns what was read and the window owns what was asked
    /// for.
    /// </para>
    /// </summary>
    internal sealed class EventBusModel
    {
        private readonly List<EventBusBehaviour> _buses = new();
        private readonly List<EventTypeEntry> _entries = new();
        private readonly List<EventTypeEntry> _filtered = new();
        private readonly List<EventBusRow> _rows = new();

        // Reused scratch list, so ordering the subscribers of every event does not allocate a list
        // per event on every rebuild.
        private readonly List<HandlerEntry> _sorted = new();

        private string[] _busLabels = Array.Empty<string>();

        /// <summary>Every event bus in the loaded scenes, in the order they were found.</summary>
        internal IReadOnlyList<EventBusBehaviour> Buses => _buses;

        /// <summary>One label per bus, naming the scene and object it sits on.</summary>
        internal string[] BusLabels => _busLabels;

        /// <summary>Every event the chosen bus currently holds handlers for.</summary>
        internal IReadOnlyList<EventTypeEntry> Entries => _entries;

        /// <summary>The events left after the search box and the leak filter.</summary>
        internal IReadOnlyList<EventTypeEntry> Filtered => _filtered;

        /// <summary>The rows as they are drawn, each an event or one of its subscribers.</summary>
        internal IReadOnlyList<EventBusRow> Rows => _rows;

        /// <summary>How many subscribers the chosen bus holds across every event.</summary>
        internal int HandlerCount { get; private set; }

        /// <summary>How many of those subscribers run on an object that was already destroyed.</summary>
        internal int LeakCount { get; private set; }

        /// <summary>Which of the buses the window is showing.</summary>
        internal int BusIndex { get; set; }

        /// <summary>Reads the buses in the loaded scenes and the events the chosen one holds.</summary>
        internal void Read()
        {
            ReadBuses();
            ReadEntries();
        }

        /// <summary>
        /// Reduces what was read to the rows that get drawn.
        /// </summary>
        /// <param name="search">The text the search box holds.</param>
        /// <param name="leaksOnly">Whether only leaked subscriptions are wanted.</param>
        /// <param name="expanded">The events whose subscribers are shown.</param>
        /// <param name="sorting">The order the table is in.</param>
        internal void Filter(string search, bool leaksOnly, HashSet<Type> expanded, EventBusSorting sorting)
        {
            _filtered.Clear();

            foreach (EventTypeEntry entry in _entries)
            {
                if (leaksOnly && !entry.HasLeaks)
                    continue;

                if (entry.Matches(search))
                    _filtered.Add(entry);
            }

            // The bus keeps a dictionary, so its order is arbitrary either way and the rows have to
            // be put in some order before they are drawn.
            _filtered.Sort(sorting.CompareEvents);

            BuildRows(leaksOnly, expanded, sorting);
        }

        /// <summary>
        /// Rebuilds only the rows, for when an event was expanded or collapsed and nothing else about
        /// what is shown changed.
        /// </summary>
        /// <param name="leaksOnly">Whether only leaked subscriptions are wanted.</param>
        /// <param name="expanded">The events whose subscribers are shown.</param>
        /// <param name="sorting">The order the table is in.</param>
        internal void BuildRows(bool leaksOnly, HashSet<Type> expanded, EventBusSorting sorting)
        {
            _rows.Clear();

            foreach (EventTypeEntry entry in _filtered)
            {
                _rows.Add(new EventBusRow(entry, null));

                if (!expanded.Contains(entry.EventType))
                    continue;

                _sorted.Clear();

                foreach (HandlerEntry handler in entry.Handlers)
                {
                    if (leaksOnly && !handler.IsLeak)
                        continue;

                    _sorted.Add(handler);
                }

                // Sorted per event rather than across the whole table, because a subscriber only
                // means anything under the event it is subscribed to.
                _sorted.Sort(sorting.CompareHandlers);

                foreach (HandlerEntry handler in _sorted)
                    _rows.Add(new EventBusRow(entry, handler));
            }
        }

        private void ReadBuses()
        {
            _buses.Clear();
            _buses.AddRange(Object.FindObjectsByType<EventBusBehaviour>(FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID));

            if (_busLabels.Length != _buses.Count)
                _busLabels = new string[_buses.Count];

            for (int i = 0; i < _buses.Count; i++)
                _busLabels[i] = SceneLabel.Describe(_buses[i]);

            BusIndex = Mathf.Clamp(BusIndex, 0, Mathf.Max(0, _buses.Count - 1));
        }

        private void ReadEntries()
        {
            _entries.Clear();

            HandlerCount = 0;
            LeakCount = 0;

            if (_buses.Count == 0)
                return;

            EventBusBehaviour bus = _buses[BusIndex];

            if (bus == null)
                return;

            foreach (KeyValuePair<Type, Delegate> pair in bus.Handlers)
            {
                EventTypeEntry entry = new(pair.Key, pair.Value);

                _entries.Add(entry);

                HandlerCount += entry.Handlers.Count;
                LeakCount += entry.LeakCount;
            }
        }
    }
}