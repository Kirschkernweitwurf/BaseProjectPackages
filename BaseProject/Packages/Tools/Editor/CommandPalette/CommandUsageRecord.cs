using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>How often and how recently a single command was run.</summary>
    [Serializable]
    internal sealed class CommandUsageRecord
    {
        [SerializeField] private string id;
        [SerializeField] private int count;
        [SerializeField] private long lastUsedTicks;

        /// <summary>Id of the command this record belongs to.</summary>
        internal string Id => id;

        /// <summary>How often the command was run.</summary>
        internal int Count => count;

        /// <summary>UTC tick count of the last run.</summary>
        internal long LastUsedTicks => lastUsedTicks;

        /// <summary>Required by serialization.</summary>
        public CommandUsageRecord() { }

        /// <summary>Creates an empty record for a command.</summary>
        /// <param name="id">Id of the command.</param>
        public CommandUsageRecord(string id) => this.id = id;

        /// <summary>Counts one more run at the given moment.</summary>
        /// <param name="ticks">UTC tick count of the run.</param>
        internal void Register(long ticks)
        {
            count++;
            lastUsedTicks = ticks;
        }
    }
}