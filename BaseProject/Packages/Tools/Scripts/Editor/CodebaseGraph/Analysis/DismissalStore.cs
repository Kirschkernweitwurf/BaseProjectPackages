using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Remembers which findings have been looked at and dismissed, so a first pass over a few thousand
    /// candidates can be worked through in sittings. Kept in ProjectSettings as plain text, so it
    /// survives recompiles and reviews cleanly in a diff.
    /// </summary>
    internal static class DismissalStore
    {
        private const int CurrentVersion = 2;
        private const string FilePath = "ProjectSettings/CodebaseGraphDismissed.json";

        /// <summary>Raised whenever the set of dismissals changes, so open windows can refresh.</summary>
        public static event Action Changed;

        /// <summary>True when nothing has been dismissed, so lookups can skip the work entirely.</summary>
        public static bool IsEmpty
        {
            get
            {
                Load();
                return Own.Count == 0 && Tree.Count == 0;
            }
        }

        /// <summary>How many entries have been dismissed.</summary>
        public static int Count
        {
            get
            {
                Load();
                return Own.Count + Tree.Count;
            }
        }

        /// <summary>The ids that were dismissed on their own, without their contents.</summary>
        public static IReadOnlyCollection<string> DismissedAlone
        {
            get
            {
                Load();
                return Own;
            }
        }

        /// <summary>The ids that were dismissed together with everything inside them.</summary>
        public static IReadOnlyCollection<string> DismissedWithContents
        {
            get
            {
                Load();
                return Tree;
            }
        }

        private static readonly HashSet<string> Own = new(StringComparer.Ordinal);
        private static readonly HashSet<string> Tree = new(StringComparer.Ordinal);

        private static DateTime _lastWriteTimeUtc;
        private static bool _isLoaded;

        /// <summary>
        /// Rereads the file when something outside the window changed it. The findings report tells
        /// people they may edit the file by hand, so writing over their edits would be rude.
        /// </summary>
        public static void Refresh()
        {
            if (!File.Exists(FilePath))
                return;

            if (File.GetLastWriteTimeUtc(FilePath) == _lastWriteTimeUtc)
                return;

            _isLoaded = false;
            Own.Clear();
            Tree.Clear();
            Load();
        }

        /// <summary>True when the findings on this id itself are hidden.</summary>
        /// <param name="id">Stable id of the entry.</param>
        /// <returns>True when hidden.</returns>
        public static bool Contains(string id)
        {
            Load();
            return Own.Contains(id) || Tree.Contains(id);
        }

        /// <summary>True when this id was dismissed together with everything inside it.</summary>
        /// <param name="id">Stable id of the entry.</param>
        /// <returns>True when the whole subtree is hidden.</returns>
        public static bool ContainsTree(string id)
        {
            Load();
            return Tree.Contains(id);
        }

        /// <summary>Sets an entry aside.</summary>
        /// <param name="id">Stable id of the entry.</param>
        /// <param name="includeContents">True to hide everything inside it as well.</param>
        public static void Dismiss(string id, bool includeContents)
        {
            Refresh();
            Load();

            if (includeContents)
                Tree.Add(id);
            else
                Own.Add(id);

            Save();
        }

        /// <summary>Brings one dismissed entry back, whichever way it was dismissed.</summary>
        /// <param name="id">Stable id of the entry.</param>
        /// <returns>True when the entry had been dismissed and is now showing again.</returns>
        public static bool Restore(string id)
        {
            Refresh();
            Load();

            bool removed = Own.Remove(id) | Tree.Remove(id);

            if (removed)
                Save();

            return removed;
        }

        /// <summary>
        /// Brings an entry back together with everything inside it. A namespace holds its types, a type
        /// holds its members, so this is the exact reverse of dismissing with contents.
        /// </summary>
        /// <param name="id">Stable id of the entry.</param>
        /// <returns>How many entries came back, including the one named.</returns>
        public static int RestoreWithContents(string id)
        {
            Refresh();
            Load();

            List<string> doomed = new();

            foreach (string candidate in Own)
            {
                if (candidate == id || GraphIdentity.IsNested(id, candidate))
                    doomed.Add(candidate);
            }

            foreach (string candidate in Tree)
            {
                if (candidate == id || GraphIdentity.IsNested(id, candidate))
                    doomed.Add(candidate);
            }

            foreach (string candidate in doomed)
            {
                Own.Remove(candidate);
                Tree.Remove(candidate);
            }

            if (doomed.Count > 0)
                Save();

            return doomed.Count;
        }

        /// <summary>Lists every dismissal, sorted for reading rather than for the file.</summary>
        /// <returns>The entries, grouped by kind and then by name.</returns>
        public static List<DismissalEntry> Collect()
        {
            Load();

            List<DismissalEntry> entries = new();

            foreach (string id in Own)
                AppendEntry(entries, id, false);

            foreach (string id in Tree)
                AppendEntry(entries, id, true);

            entries.Sort(CompareEntries);
            return entries;
        }

        /// <summary>Brings every dismissed entry back.</summary>
        public static void RestoreAll()
        {
            Load();
            Own.Clear();
            Tree.Clear();
            Save();
        }

        private static void AppendEntry(List<DismissalEntry> entries, string id, bool includesContents)
        {
            if (!GraphIdentity.TryRead(id, out EDismissalKind kind, out string name))
                return;

            GraphIdentity.ReadEntry(id, out EFinding finding);

            entries.Add(new DismissalEntry(id, kind, name, includesContents)
            {
                Finding = finding
            });
        }

        private static int CompareEntries(DismissalEntry left, DismissalEntry right)
        {
            int byKind = left.Kind.CompareTo(right.Kind);

            return byKind != 0
                ? byKind
                : string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Says once that an older file was read. Those ids name no finding, so each of them now
        /// silences everything on its entry, including findings a later scan raises for the first time.
        /// Nothing is changed for you, and nothing is remembered: this is a line to read, not a state
        /// to manage.
        /// </summary>
        private static void ReportWidened()
        {
            int widened = 0;

            foreach (string id in Own)
                widened += CountEntryWide(id);

            foreach (string id in Tree)
                widened += CountEntryWide(id);

            Save();

            if (widened == 0)
                return;

            CustomLogger.LogWarning($"{widened} dismissals came from a file written before an id could "
                + "name a single finding, so each one now silences everything on its entry. Re-apply "
                + "them from a findings report to narrow them.",
                null);
        }

        private static int CountEntryWide(string id)
        {
            GraphIdentity.ReadEntry(id, out EFinding finding);

            return finding == EFinding.None
                ? 1
                : 0;
        }

        private static void Load()
        {
            if (_isLoaded)
                return;

            _isLoaded = true;

            if (!File.Exists(FilePath))
                return;

            try
            {
                DismissalData data = JsonUtility.FromJson<DismissalData>(File.ReadAllText(FilePath));
                if (data == null)
                    return;

                Own.UnionWith(data.own);
                Tree.UnionWith(data.tree);
                _lastWriteTimeUtc = File.GetLastWriteTimeUtc(FilePath);

                if (data.version < CurrentVersion)
                    ReportWidened();
            }
            catch (Exception exception)
            {
                CustomLogger.LogWarning($"Could not read {FilePath}: {exception.Message}", null);
            }
        }

        private static void Save()
        {
            DismissalData data = new()
            {
                version = CurrentVersion
            };

            data.own.AddRange(Own);
            data.tree.AddRange(Tree);

            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
                _lastWriteTimeUtc = File.GetLastWriteTimeUtc(FilePath);
                Changed?.Invoke();
            }
            catch (Exception exception)
            {
                // A read only file under source control throws UnauthorizedAccessException rather than
                // IOException, and letting that escape would take the dismiss action down with it.
                CustomLogger.LogError($"Could not write {FilePath}: {exception.Message}", null);
            }
        }
    }
}