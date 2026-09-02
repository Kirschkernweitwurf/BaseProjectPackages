using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.TodoOverview.Settings
{
    /// <summary>
    /// Per project settings for the todo overview, kept in <c>ProjectSettings</c> so they are version
    /// controlled and the whole project searches for the same keywords in the same notation.
    /// <para>
    /// Everything the scan looks for is declared here rather than baked in, because a comment
    /// convention is a team decision: which words mark an item, how the responsible person and the
    /// date are written, and how far an item reaches past its own line.
    /// </para>
    /// </summary>
    [FilePath("ProjectSettings/TodoSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class TodoSettings : ScriptableSingleton<TodoSettings>
    {
        /// <summary>The serialized name of the case sensitivity flag, for the settings page.</summary>
        internal const string CaseSensitivePropertyName = nameof(caseSensitive);

        /// <summary>The serialized name of the continuation mode, for the settings page.</summary>
        internal const string ContinuationPropertyName = nameof(continuation);

        /// <summary>The serialized name of the date notation choice, for the settings page.</summary>
        internal const string DateDisplayPropertyName = nameof(dateDisplay);

        /// <summary>The serialized name of the date format list, for the settings page.</summary>
        internal const string DateFormatsPropertyName = nameof(dateFormats);

        /// <summary>The serialized name of the file extension list, for the settings page.</summary>
        internal const string ExtensionsPropertyName = nameof(fileExtensions);

        /// <summary>The serialized name of the ignore list, for the settings page.</summary>
        internal const string IgnoredPropertyName = nameof(ignoredPathFragments);

        /// <summary>The serialized name of the metadata pattern list, for the settings page.</summary>
        internal const string MetadataPropertyName = nameof(metadataPatterns);

        /// <summary>The serialized name of the tag list, for the settings page.</summary>
        internal const string TagsPropertyName = nameof(tags);

        [SerializeField] private bool seeded;
        [SerializeField] private List<TodoTag> tags = new();
        [SerializeField] private List<string> fileExtensions = new();
        [SerializeField] private List<string> ignoredPathFragments = new();
        [SerializeField] private List<string> metadataPatterns = new();
        [SerializeField] private List<string> dateFormats = new();
        [SerializeField] private ETodoContinuation continuation;
        [SerializeField] private ETodoDateDisplay dateDisplay;
        [SerializeField] private bool caseSensitive;
        [SerializeField] private bool includePackages;

        /// <summary>The keywords the scan looks for, in the order they are listed in.</summary>
        internal IReadOnlyList<TodoTag> Tags
        {
            get
            {
                EnsureSeeded();
                return tags;
            }
        }

        /// <summary>The patterns that read the responsible person and the date out of an item.</summary>
        internal IReadOnlyList<string> MetadataPatterns
        {
            get
            {
                EnsureSeeded();
                return metadataPatterns;
            }
        }

        /// <summary>The date formats a date in an item is read with.</summary>
        internal IReadOnlyList<string> DateFormats
        {
            get
            {
                EnsureSeeded();
                return dateFormats;
            }
        }

        /// <summary>The notation every date is shown in, whatever notation it was written in.</summary>
        internal ETodoDateDisplay DateDisplay
        {
            get
            {
                EnsureSeeded();
                return dateDisplay;
            }
        }

        /// <summary>How far an item reaches past the line its keyword sits on.</summary>
        internal ETodoContinuation Continuation
        {
            get
            {
                EnsureSeeded();
                return continuation;
            }
        }

        /// <summary>Whether a keyword has to be written in the exact casing of its tag.</summary>
        internal bool CaseSensitive
        {
            get
            {
                EnsureSeeded();
                return caseSensitive;
            }
        }

        /// <summary>Whether files under <c>Packages</c> are scanned as well.</summary>
        internal bool IncludePackages
        {
            get
            {
                EnsureSeeded();
                return includePackages;
            }
        }

        /// <summary>Writes the settings back to disk after edits.</summary>
        internal void Persist() => Save(true);

        /// <summary>Switches package scanning on or off and saves right away.</summary>
        /// <param name="value">Whether files under <c>Packages</c> are scanned.</param>
        internal void SetIncludePackages(bool value)
        {
            EnsureSeeded();

            if (includePackages == value)
                return;

            includePackages = value;
            Save(true);
        }

        /// <summary>Whether a file is one of the types that get read at all.</summary>
        /// <param name="assetPath">Project relative path of the file.</param>
        /// <returns><c>true</c> when the file's extension is on the list.</returns>
        internal bool IsScannable(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;

            EnsureSeeded();

            foreach (string extension in fileExtensions)
            {
                if (string.IsNullOrWhiteSpace(extension))
                    continue;

                if (assetPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>Whether a file sits under something the project declared out of scope.</summary>
        /// <param name="assetPath">Project relative path of the file.</param>
        /// <returns><c>true</c> when items in it are not reported.</returns>
        internal bool IsIgnored(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;

            EnsureSeeded();

            foreach (string fragment in ignoredPathFragments)
            {
                if (string.IsNullOrWhiteSpace(fragment))
                    continue;

                if (assetPath.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private void EnsureSeeded()
        {
            if (seeded)
                return;

            // Only empty lists are filled, so a project that deliberately cleared one keeps it cleared
            // once the seeding flag is set.
            if (tags.Count == 0)
                tags.AddRange(TodoDefaults.CreateTags());

            if (fileExtensions.Count == 0)
                fileExtensions.AddRange(TodoDefaults.CreateExtensions());

            if (metadataPatterns.Count == 0)
                metadataPatterns.AddRange(TodoDefaults.CreateMetadataPatterns());

            if (dateFormats.Count == 0)
                dateFormats.AddRange(TodoDefaults.CreateDateFormats());

            continuation = TodoDefaults.Continuation;
            dateDisplay = TodoDefaults.DateDisplay;
            seeded = true;

            Save(true);
        }
    }
}