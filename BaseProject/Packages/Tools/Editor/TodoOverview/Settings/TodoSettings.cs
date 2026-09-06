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
        /// <summary>The serialized name of the aging threshold, for the settings page.</summary>
        internal const string AgingPropertyName = nameof(agingAfterDays);

        /// <summary>The serialized name of the case sensitivity flag, for the settings page.</summary>
        internal const string CaseSensitivePropertyName = nameof(caseSensitive);

        /// <summary>The serialized name of the continuation mode, for the settings page.</summary>
        internal const string ContinuationPropertyName = nameof(continuation);

        /// <summary>The serialized name of the date notation choice, for the settings page.</summary>
        internal const string DateDisplayPropertyName = nameof(dateDisplay);

        /// <summary>The serialized name of the date format list, for the settings page.</summary>
        internal const string DateFormatsPropertyName = nameof(dateFormats);

        /// <summary>The serialized name of the date reading, for the settings page.</summary>
        internal const string DateMeaningPropertyName = nameof(dateMeaning);

        /// <summary>The serialized name of the file extension list, for the settings page.</summary>
        internal const string ExtensionsPropertyName = nameof(fileExtensions);

        /// <summary>The serialized name of the ignore list, for the settings page.</summary>
        internal const string IgnoredPropertyName = nameof(ignoredPathFragments);

        /// <summary>The serialized name of the metadata pattern list, for the settings page.</summary>
        internal const string MetadataPropertyName = nameof(metadataPatterns);

        /// <summary>The serialized name of the stale threshold, for the settings page.</summary>
        internal const string StalePropertyName = nameof(staleAfterDays);

        /// <summary>The serialized name of the tag list, for the settings page.</summary>
        internal const string TagsPropertyName = nameof(tags);

        /// <summary>
        /// What the stored settings were last brought up to date with. A project that predates a
        /// step gets that step run once; a fresh one is written at the current number and skips them.
        /// </summary>
        private const int CurrentVersion = 1;

        /// <summary>The prefix a named group carries inside a pattern, used to look for one.</summary>
        private const string GroupPrefix = "?<";

        /// <summary>The suffix a named group carries inside a pattern, used to look for one.</summary>
        private const string GroupSuffix = ">";

        [SerializeField] private bool seeded;
        [SerializeField] private List<TodoTag> tags = new();
        [SerializeField] private List<string> fileExtensions = new();
        [SerializeField] private List<string> ignoredPathFragments = new();
        [SerializeField] private List<string> metadataPatterns = new();
        [SerializeField] private List<string> dateFormats = new();
        [SerializeField] private ETodoContinuation continuation;
        [SerializeField] private ETodoDateDisplay dateDisplay;
        [SerializeField] private ETodoDateMeaning dateMeaning;
        [SerializeField] private int agingAfterDays;
        [SerializeField] private int staleAfterDays;
        [SerializeField] private int settingsVersion;
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

        /// <summary>
        /// What a date that does not say for itself means: a deadline, or the day the note was
        /// written. An item can still override it by putting its date in a due or written group.
        /// </summary>
        internal ETodoDateMeaning DateMeaning
        {
            get
            {
                EnsureSeeded();
                return dateMeaning;
            }
        }

        /// <summary>Days a written date ages before the item is worth a look.</summary>
        internal int AgingAfterDays
        {
            get
            {
                EnsureSeeded();
                return agingAfterDays;
            }
        }

        /// <summary>Days a written date ages before the item counts as stale.</summary>
        internal int StaleAfterDays
        {
            get
            {
                EnsureSeeded();
                return staleAfterDays;
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
            if (!seeded)
                Seed();

            if (settingsVersion < CurrentVersion)
                Upgrade();
        }

        private void Seed()
        {
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
            dateMeaning = TodoDefaults.DateMeaning;
            agingAfterDays = TodoDefaults.AgingAfterDays;
            staleAfterDays = TodoDefaults.StaleAfterDays;
            seeded = true;
            settingsVersion = CurrentVersion;

            Save(true);
        }

        /// <summary>
        /// Brings a project that was configured before a step existed up to date. Only ever adds what
        /// is missing, so nothing a project decided for itself is written over.
        /// </summary>
        private void Upgrade()
        {
            if (agingAfterDays <= 0)
                agingAfterDays = TodoDefaults.AgingAfterDays;

            if (staleAfterDays <= 0)
                staleAfterDays = TodoDefaults.StaleAfterDays;

            AddMeaningPatterns();

            settingsVersion = CurrentVersion;

            Save(true);
        }

        // Only added when the project has nothing that reads a marked date yet, and only once: after
        // the version has moved past this step a project that deleted them keeps them deleted.
        private void AddMeaningPatterns()
        {
            if (HasGroup(TodoGroupNames.Due) || HasGroup(TodoGroupNames.Written))
                return;

            metadataPatterns.InsertRange(0, TodoDefaults.CreateMeaningPatterns());
        }

        private bool HasGroup(string groupName)
        {
            string marker = GroupPrefix + groupName + GroupSuffix;

            foreach (string pattern in metadataPatterns)
            {
                if (pattern != null && pattern.Contains(marker, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}