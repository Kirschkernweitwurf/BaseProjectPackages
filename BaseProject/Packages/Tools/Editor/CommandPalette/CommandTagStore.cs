using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Stores the custom tags and pins of the palette. Lives in the project settings folder so it
    /// can be version controlled and shared with everyone working on the project.
    /// </summary>
    [FilePath(StorePath, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class CommandTagStore : ScriptableSingleton<CommandTagStore>
    {
        private const string StorePath = "ProjectSettings/CommandPaletteTags.asset";

        private static readonly string[] NoTags = Array.Empty<string>();

        [SerializeField] private List<CommandTagRecord> records = new();

        private Dictionary<string, CommandTagRecord> Lookup
        {
            get
            {
                if (_lookup != null)
                    return _lookup;

                _lookup = new Dictionary<string, CommandTagRecord>(records.Count, StringComparer.Ordinal);

                foreach (CommandTagRecord record in records)
                {
                    if (!string.IsNullOrEmpty(record.Id))
                        _lookup[record.Id] = record;
                }

                return _lookup;
            }
        }

        [NonSerialized] private Dictionary<string, CommandTagRecord> _lookup;

        /// <summary>Every tag used anywhere, sorted and without duplicates.</summary>
        /// <returns>The known tags.</returns>
        internal IReadOnlyList<string> KnownTags()
        {
            SortedSet<string> distinct = new(StringComparer.Ordinal);

            foreach (CommandTagRecord record in records)
            {
                foreach (string tag in record.Tags)
                    distinct.Add(tag);
            }

            string[] result = new string[distinct.Count];
            distinct.CopyTo(result);

            return result;
        }

        /// <summary>Returns the tags of a command, or an empty list.</summary>
        /// <param name="id">Id of the command.</param>
        /// <returns>The assigned tags.</returns>
        internal IReadOnlyList<string> TagsFor(string id) => Lookup.TryGetValue(id, out CommandTagRecord record)
            ? record.Tags
            : NoTags;

        /// <summary>Returns whether a command is pinned.</summary>
        /// <param name="id">Id of the command.</param>
        /// <returns><c>true</c> when the command is pinned.</returns>
        internal bool IsPinned(string id) => Lookup.TryGetValue(id, out CommandTagRecord record) && record.Pinned;

        /// <summary>Replaces the tags of a command and writes the store to disk.</summary>
        /// <param name="id">Id of the command.</param>
        /// <param name="tags">The new tags. They are trimmed, lowercased and deduplicated.</param>
        internal void SetTags(string id, IEnumerable<string> tags)
        {
            CommandTagRecord record = Require(id);
            record.SetTags(Normalize(tags));

            Prune(record);
            Persist();
        }

        /// <summary>Pins or unpins a command and writes the store to disk.</summary>
        /// <param name="id">Id of the command.</param>
        internal void TogglePinned(string id)
        {
            CommandTagRecord record = Require(id);
            record.Pinned = !record.Pinned;

            Prune(record);
            Persist();
        }

        private static string[] Normalize(IEnumerable<string> tags)
        {
            SortedSet<string> distinct = new(StringComparer.Ordinal);

            foreach (string tag in tags)
            {
                string trimmed = tag.Trim().ToLowerInvariant();

                if (trimmed.Length > 0)
                    distinct.Add(trimmed);
            }

            if (distinct.Count == 0)
                return NoTags;

            string[] result = new string[distinct.Count];
            distinct.CopyTo(result);

            return result;
        }

        private void Persist() => Save(true);

        private void Prune(CommandTagRecord record)
        {
            if (!record.IsEmpty)
                return;

            records.Remove(record);
            _lookup = null;
        }

        private CommandTagRecord Require(string id)
        {
            if (Lookup.TryGetValue(id, out CommandTagRecord existing))
                return existing;

            CommandTagRecord created = new(id);

            records.Add(created);
            Lookup[id] = created;

            return created;
        }
    }
}