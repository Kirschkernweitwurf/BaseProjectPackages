#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Central store of all managed menu entries. Edited through the Menu Manager window.</summary>
    [FilePath(FilePathValue, FilePathAttribute.Location.ProjectFolder)]
    public sealed class MenuRegistry : ScriptableSingleton<MenuRegistry>
    {
        private const string DefaultGroupName = "Ungrouped";
        private const string FilePathValue = "ProjectSettings/MenuManagerRegistry.asset";

        [SerializeField]
        private int startPriority;

        [SerializeField]
        private int separatorGap = 11;

        [SerializeField]
        private List<MenuGroup> groups = new();

        /// <summary>Priority assigned to the first registered entry.</summary>
        public int StartPriority
        {
            get => startPriority;
            set => startPriority = value;
        }

        /// <summary>Priority gap inserted between groups. A gap of 11 or more draws a separator line.</summary>
        public int SeparatorGap
        {
            get => separatorGap;
            set => separatorGap = Mathf.Max(1, value);
        }

        /// <summary>All groups in draw order.</summary>
        public List<MenuGroup> Groups => groups;

        /// <summary>Adds newly discovered entries, flags missing ones, and refreshes kinds.</summary>
        public void Sync(IReadOnlyDictionary<string, ResolvedMenu> resolved)
        {
            HashSet<string> known = new();

            foreach (MenuGroup group in groups)
            {
                foreach (MenuEntry entry in group.Entries)
                {
                    known.Add(entry.Id);
                    bool present = resolved.TryGetValue(entry.Id, out ResolvedMenu match);
                    entry.Missing = !present;

                    if (present)
                        entry.Kind = match.Kind;
                }
            }

            foreach (KeyValuePair<string, ResolvedMenu> pair in resolved)
            {
                if (known.Contains(pair.Key))
                    continue;

                MenuEntry entry = new(pair.Key, pair.Value.DefaultPath, pair.Value.Kind);
                GetDefaultGroup().Entries.Add(entry);
            }
        }

        /// <summary>Recomputes derived priorities from group and entry order.</summary>
        public void RecalculatePriorities()
        {
            int priority = startPriority;
            bool firstEmitted = true;

            foreach (MenuGroup group in groups)
            {
                bool groupEmits = false;

                foreach (MenuEntry entry in group.Entries)
                {
                    if (!entry.Enabled || entry.Missing || string.IsNullOrWhiteSpace(entry.Path))
                    {
                        entry.Priority = int.MinValue;
                        continue;
                    }

                    if (!groupEmits && !firstEmitted)
                        priority += separatorGap;

                    groupEmits = true;
                    entry.Priority = priority;
                    priority++;
                }

                if (groupEmits)
                    firstEmitted = false;
            }
        }

        /// <summary>Writes the in-memory state to disk.</summary>
        public void Persist() => Save(true);

        private MenuGroup GetDefaultGroup()
        {
            foreach (MenuGroup group in groups)
            {
                if (group.Name == DefaultGroupName)
                    return group;
            }

            MenuGroup created = new(DefaultGroupName);
            groups.Add(created);
            return created;
        }
    }
}
#endif