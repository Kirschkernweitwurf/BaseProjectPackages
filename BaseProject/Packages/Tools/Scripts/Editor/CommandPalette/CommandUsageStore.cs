using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Remembers which commands were run and when, so the palette can rank the ones actually used
    /// above the rest. Lives in the library folder because this is a personal habit, not a project
    /// setting anyone else needs.
    /// </summary>
    [FilePath(StorePath, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class CommandUsageStore : ScriptableSingleton<CommandUsageStore>
    {
        private const string StorePath = "Library/CommandPaletteUsage.asset";

        [SerializeField] private List<CommandUsageRecord> records = new();

        [NonSerialized] private Dictionary<string, CommandUsageRecord> _lookup;

        private Dictionary<string, CommandUsageRecord> Lookup
        {
            get
            {
                if (_lookup != null)
                    return _lookup;

                _lookup = new Dictionary<string, CommandUsageRecord>(records.Count, StringComparer.Ordinal);

                foreach (CommandUsageRecord record in records)
                {
                    if (!string.IsNullOrEmpty(record.Id))
                        _lookup[record.Id] = record;
                }

                return _lookup;
            }
        }

        /// <summary>Returns how often a command was run.</summary>
        /// <param name="id">Id of the command.</param>
        /// <returns>The run count.</returns>
        public int CountFor(string id) => Lookup.TryGetValue(id, out CommandUsageRecord record)
            ? record.Count
            : 0;

        /// <summary>Returns the UTC tick count of the last run.</summary>
        /// <param name="id">Id of the command.</param>
        /// <returns>The tick count, or zero when the command was never run.</returns>
        public long LastUsedFor(string id) => Lookup.TryGetValue(id, out CommandUsageRecord record)
            ? record.LastUsedTicks
            : 0L;

        /// <summary>Counts one run of a command and writes the store to disk.</summary>
        /// <param name="id">Id of the command.</param>
        public void Register(string id)
        {
            if (!Lookup.TryGetValue(id, out CommandUsageRecord record))
            {
                record = new CommandUsageRecord(id);

                records.Add(record);
                Lookup[id] = record;
            }

            record.Register(DateTime.UtcNow.Ticks);
            Save(true);
        }

        /// <summary>Forgets every recorded run.</summary>
        public void Clear()
        {
            records.Clear();
            _lookup = null;

            Save(true);
        }
    }
}