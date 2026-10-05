using System;
using System.Collections.Generic;
using System.IO;

namespace Base.UtilityPackage.Editor.Documentation
{
    /// <summary>
    /// One place documentation can be opened from: a package's <c>Documentation~</c> folder or the
    /// project's own <c>Docs</c> folder, each with an <c>index.md</c> as its start page.
    /// </summary>
    internal sealed class DocumentationSource
    {
        /// <summary>Folder name Unity skips on import, so package docs never get meta files.</summary>
        internal const string PackageDocsFolder = "Documentation~";

        /// <summary>Folder next to <c>Assets</c> that holds the project's own docs.</summary>
        internal const string ProjectDocsFolder = "Docs";

        /// <summary>The start page every documentation folder is expected to have.</summary>
        internal const string StartPage = "index.md";

        /// <summary>Creates a source for a documentation folder that is known to exist.</summary>
        /// <param name="displayName">Name shown in the window.</param>
        /// <param name="version">Package version, or empty for the project's own docs.</param>
        /// <param name="folder">Absolute path of the documentation folder.</param>
        /// <param name="onlineUrl">Where the docs can be read in a browser, or empty.</param>
        internal DocumentationSource(string displayName, string version, string folder, string onlineUrl)
        {
            DisplayName = displayName ?? string.Empty;
            Version = version ?? string.Empty;
            Folder = folder ?? throw new ArgumentNullException(nameof(folder));
            OnlineUrl = onlineUrl ?? string.Empty;
        }

        /// <summary>Name shown in the window.</summary>
        internal string DisplayName { get; }

        /// <summary>Package version, or empty for the project's own docs.</summary>
        internal string Version { get; }

        /// <summary>Absolute path of the documentation folder.</summary>
        internal string Folder { get; }

        /// <summary>Absolute path of the start page.</summary>
        internal string StartPagePath => Path.Combine(Folder, StartPage);

        /// <summary>Where the docs can be read in a browser, or empty when there is no such place.</summary>
        internal string OnlineUrl { get; }

        /// <summary>
        /// Returns the documentation folder below <paramref name="root"/> if it has a start page.
        /// </summary>
        /// <param name="root">A package folder or the project folder.</param>
        /// <param name="folderName">
        /// <see cref="PackageDocsFolder"/> for a package, <see cref="ProjectDocsFolder"/> for the project.
        /// </param>
        /// <param name="folder">The documentation folder, or <c>null</c> when there is none.</param>
        /// <returns><c>true</c> when a folder with a start page was found.</returns>
        internal static bool TryFindFolder(string root, string folderName, out string folder)
        {
            folder = null;

            if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(folderName))
                return false;

            string candidate = Path.Combine(root, folderName);

            if (!File.Exists(Path.Combine(candidate, StartPage)))
                return false;

            folder = candidate;

            return true;
        }

        /// <summary>
        /// Orders sources for display: the project's own docs first, then packages by name.
        /// </summary>
        /// <param name="sources">The sources to sort in place.</param>
        internal static void Sort(List<DocumentationSource> sources)
        {
            sources.Sort((a, b) =>
            {
                bool aIsProject = a.Version.Length == 0;
                bool bIsProject = b.Version.Length == 0;

                if (aIsProject != bIsProject)
                    return aIsProject ? -1 : 1;

                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}