using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>The tags and the pinned state stored for a single command.</summary>
    [Serializable]
    internal sealed class CommandTagRecord
    {
        private static readonly string[] NoTags = Array.Empty<string>();

        [SerializeField] private string id;
        [SerializeField] private string[] tags;
        [SerializeField] private bool pinned;

        /// <summary>Whether the command is pinned to the top of the results.</summary>
        public bool Pinned
        {
            get => pinned;
            set => pinned = value;
        }

        /// <summary>Id of the command this record belongs to.</summary>
        internal string Id => id;

        /// <summary>Tags assigned by hand, always lowercase.</summary>
        internal IReadOnlyList<string> Tags => tags ?? NoTags;

        /// <summary>True when the record holds nothing worth saving anymore.</summary>
        internal bool IsEmpty => !pinned && Tags.Count == 0;

        /// <summary>Required by serialization.</summary>
        public CommandTagRecord() { }

        /// <summary>Creates an empty record for a command.</summary>
        /// <param name="id">Id of the command.</param>
        public CommandTagRecord(string id) => this.id = id;

        /// <summary>Replaces the assigned tags.</summary>
        /// <param name="value">The new tags, already normalized.</param>
        internal void SetTags(string[] value) => tags = value;
    }
}