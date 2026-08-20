using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Per project settings for the Codebase Graph, kept in <c>ProjectSettings</c> so they are version
    /// controlled and shared rather than living in one machine's preferences.
    /// <para>
    /// The only thing here is the ignore list, and it exists because whose code a finding is about is
    /// the one question the scan cannot answer. An asset bought from the store compiles into the same
    /// assembly as everything else under Assets, references the same packages, and carries no marker
    /// saying where it came from. Nothing can detect it, so it is declared.
    /// </para>
    /// </summary>
    [FilePath("ProjectSettings/CodebaseGraphSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class CodebaseGraphSettings : ScriptableSingleton<CodebaseGraphSettings>
    {
        /// <summary>The serialized name of the ignore list, for the settings page to bind against.</summary>
        internal const string FragmentsPropertyName = nameof(ignoredPathFragments);

        [SerializeField] private List<string> ignoredPathFragments = new();

        /// <summary>Writes the settings back to disk after edits.</summary>
        internal void Persist() => Save(true);

        /// <summary>
        /// Whether a script path sits under something the project has declared out of scope.
        /// </summary>
        /// <param name="scriptPath">The project relative path of the script.</param>
        /// <returns><c>true</c> when findings on it should not be reported.</returns>
        internal bool IsIgnored(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath))
                return false;

            foreach (string fragment in ignoredPathFragments)
            {
                if (string.IsNullOrWhiteSpace(fragment))
                    continue;

                if (scriptPath.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}