using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.TodoOverview.Model;
using Base.ToolPackage.Editor.TodoOverview.Settings;
using UnityEditor;

namespace Base.ToolPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// Runs one pass over the project and collects every item. Files are taken from the asset database
    /// rather than from disk, so a package that only exists virtually is covered too, and each file is
    /// dismissed on a plain text check before it is lexed.
    /// </summary>
    internal static class TodoScanner
    {
        private const string AssetsPrefix = "Assets/";
        private const string PackagesPrefix = "Packages/";
        private const int ProgressInterval = 64;
        private const string ProgressLabel = "Reading {0} of {1} files";
        private const string ProgressTitle = "Todo Overview";

        /// <summary>Scans the project for items.</summary>
        /// <param name="settings">The settings that say what to look for and where.</param>
        /// <returns>Every item that was found, in file order.</returns>
        internal static List<TodoEntry> Scan(TodoSettings settings)
        {
            List<TodoEntry> entries = new();
            TodoPatterns patterns = TodoPatterns.Create(settings);

            if (!patterns.HasKeywords)
                return entries;

            List<string> files = CollectFiles(settings);

            try
            {
                for (int i = 0; i < files.Count; i++)
                {
                    if (i % ProgressInterval == 0
                        && IsCanceled(i, files.Count))
                        break;

                    string source = TodoSourceReader.Read(files[i]);

                    if (!patterns.ContainsKeyword(source))
                        continue;

                    TodoCommentParser.Parse(files[i], source, patterns, entries);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return entries;
        }

        private static bool IsCanceled(int done, int total)
        {
            float progress = total == 0
                ? 1f
                : done / (float)total;

            return EditorUtility.DisplayCancelableProgressBar(ProgressTitle,
                string.Format(ProgressLabel, done, total), progress);
        }

        private static List<string> CollectFiles(TodoSettings settings)
        {
            List<string> files = new();

            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (!IsInScope(path, settings))
                    continue;

                if (!settings.IsScannable(path) || settings.IsIgnored(path))
                    continue;

                files.Add(path);
            }

            files.Sort(StringComparer.Ordinal);

            return files;
        }

        private static bool IsInScope(string path, TodoSettings settings)
        {
            if (path.StartsWith(AssetsPrefix, StringComparison.Ordinal))
                return true;

            return settings.IncludePackages
                && path.StartsWith(PackagesPrefix, StringComparison.Ordinal);
        }
    }
}