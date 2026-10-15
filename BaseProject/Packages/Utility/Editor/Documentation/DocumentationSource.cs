using System;
using System.Collections.Generic;
using System.IO;

namespace Base.UtilityPackage.Editor.Documentation
{
    /// <summary>
    /// One place documentation can be opened from: a package's <c>Documentation~</c> folder or the
    /// project's own <c>Docs</c> folder, each with a start page in its root.
    /// </summary>
    /// <remarks>
    /// The start page is the page whose properties say <c>type: Start Page</c>, so it can carry a
    /// descriptive file name such as <c>Tweening Package.md</c>. Folders without one fall back to
    /// <see cref="FallbackStartPage"/>, which keeps older packages and projects working.
    /// </remarks>
    internal sealed class DocumentationSource
    {
        /// <summary>Start page of folders that do not mark one in their properties.</summary>
        internal const string FallbackStartPage = "index.md";

        /// <summary>Folder name Unity skips on import, so package docs never get meta files.</summary>
        internal const string PackageDocsFolder = "Documentation~";

        /// <summary>Folder next to <c>Assets</c> that holds the project's own docs.</summary>
        internal const string ProjectDocsFolder = "Docs";

        /// <summary>Value of the <c>type</c> property that marks a page as the start page.</summary>
        internal const string StartPageType = "Start Page";

        private const string FrontmatterFence = "---";
        private const string MarkdownPattern = "*.md";
        private const string TypeKey = "type:";

        private static readonly char[] Quotes = { '"', '\'' };

        /// <summary>Creates a source for a documentation folder that is known to exist.</summary>
        /// <param name="displayName">Name shown in the window.</param>
        /// <param name="version">Package version, or empty for the project's own docs.</param>
        /// <param name="folder">Absolute path of the documentation folder.</param>
        /// <param name="startPage">File name of the start page inside <paramref name="folder"/>.</param>
        /// <param name="onlineUrl">Where the docs can be read in a browser, or empty.</param>
        internal DocumentationSource(string displayName, string version, string folder, string startPage,
            string onlineUrl)
        {
            DisplayName = displayName ?? string.Empty;
            Version = version ?? string.Empty;
            Folder = folder ?? throw new ArgumentNullException(nameof(folder));
            StartPage = string.IsNullOrEmpty(startPage)
                ? FallbackStartPage
                : startPage;
            OnlineUrl = onlineUrl ?? string.Empty;
        }

        /// <summary>Name shown in the window.</summary>
        internal string DisplayName { get; }

        /// <summary>Package version, or empty for the project's own docs.</summary>
        internal string Version { get; }

        /// <summary>Absolute path of the documentation folder.</summary>
        internal string Folder { get; }

        /// <summary>File name of the start page, for example <c>Tweening Package.md</c>.</summary>
        internal string StartPage { get; }

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
        /// <param name="startPage">File name of its start page, or <c>null</c> when there is none.</param>
        /// <returns><c>true</c> when a folder with a start page was found.</returns>
        internal static bool TryFindFolder(string root, string folderName, out string folder, out string startPage)
        {
            folder = null;
            startPage = null;

            if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(folderName))
                return false;

            string candidate = Path.Combine(root, folderName);

            if (!TryFindStartPage(candidate, out startPage))
                return false;

            folder = candidate;

            return true;
        }

        /// <summary>
        /// Finds the start page in the root of a documentation folder: the first page, by file name, whose
        /// properties say <c>type: Start Page</c>, otherwise <see cref="FallbackStartPage"/>.
        /// </summary>
        /// <param name="folder">Absolute path of the documentation folder.</param>
        /// <param name="startPage">File name of the start page, or <c>null</c> when there is none.</param>
        /// <returns><c>true</c> when the folder has a start page.</returns>
        internal static bool TryFindStartPage(string folder, out string startPage)
        {
            startPage = null;

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                return false;

            string[] pages = Directory.GetFiles(folder, MarkdownPattern, SearchOption.TopDirectoryOnly);
            Array.Sort(pages, StringComparer.OrdinalIgnoreCase);

            foreach (string page in pages)
            {
                if (!IsStartPage(File.ReadAllText(page)))
                    continue;

                startPage = Path.GetFileName(page);

                return true;
            }

            if (!File.Exists(Path.Combine(folder, FallbackStartPage)))
                return false;

            startPage = FallbackStartPage;

            return true;
        }

        /// <summary>
        /// Tells whether a page's frontmatter marks it as the start page with <c>type: Start Page</c>.
        /// </summary>
        /// <param name="markdown">The page's content.</param>
        /// <returns><c>true</c> when the page is a start page.</returns>
        internal static bool IsStartPage(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return false;

            using StringReader reader = new(markdown);

            if (reader.ReadLine()?.Trim() != FrontmatterFence)
                return false;

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string trimmed = line.Trim();

                if (trimmed == FrontmatterFence)
                    return false;

                if (!trimmed.StartsWith(TypeKey, StringComparison.OrdinalIgnoreCase))
                    continue;

                string value = trimmed.Substring(TypeKey.Length).Trim().Trim(Quotes);

                return string.Equals(value, StartPageType, StringComparison.OrdinalIgnoreCase);
            }

            return false;
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