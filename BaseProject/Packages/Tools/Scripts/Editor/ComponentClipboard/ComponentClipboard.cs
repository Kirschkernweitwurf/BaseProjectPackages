using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.ComponentClipboard
{
    /// <summary>
    /// Multi component clipboard. Unity's internal clipboard only holds a single component, so this
    /// singleton keeps its own list of serialized snapshots. Persisted outside of Assets so it is
    /// never committed to version control.
    /// </summary>
    [FilePath(FilePathConstant, FilePathAttribute.Location.ProjectFolder)]
    public class ComponentClipboard : ScriptableSingleton<ComponentClipboard>
    {
        private const string FilePathConstant = "UserSettings/Base/ComponentClipboard.asset";

        /// <summary>Raised whenever the clipboard content changes.</summary>
        public static event Action Changed;

        [SerializeField] private List<ComponentClipboardEntry> entries = new();

        /// <summary>Snapshots currently held by the clipboard, in copy order.</summary>
        public IReadOnlyList<ComponentClipboardEntry> Entries => entries;

        /// <summary>Returns true when at least one snapshot is stored.</summary>
        public bool HasEntries => entries.Count > 0;

        /// <summary>Replaces the clipboard content with snapshots of the given components.</summary>
        /// <param name="components">Components to capture. Null entries and transforms are skipped.</param>
        public void Copy(IEnumerable<Component> components)
        {
            if (components == null)
                return;

            entries.Clear();

            foreach (Component component in components)
            {
                if (!ComponentOperations.CanCopy(component))
                    continue;

                entries.Add(new ComponentClipboardEntry(component));
            }

            Persist();
        }

        /// <summary>Appends snapshots of the given components without clearing existing entries.</summary>
        /// <param name="components">Components to capture. Null entries and transforms are skipped.</param>
        public void Append(IEnumerable<Component> components)
        {
            if (components == null)
                return;

            foreach (Component component in components)
            {
                if (!ComponentOperations.CanCopy(component))
                    continue;

                entries.Add(new ComponentClipboardEntry(component));
            }

            Persist();
        }

        /// <summary>Removes the entry at the given index.</summary>
        /// <param name="index">Index into <see cref="Entries"/>.</param>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= entries.Count)
                return;

            entries.RemoveAt(index);
            Persist();
        }

        /// <summary>Removes all stored snapshots.</summary>
        public void Clear()
        {
            entries.Clear();
            Persist();
        }

        private void Persist()
        {
            Save(true);
            Changed?.Invoke();
        }
    }
}