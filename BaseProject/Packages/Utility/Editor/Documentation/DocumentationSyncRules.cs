using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Base.UtilityPackage.Editor.Documentation
{
    /// <summary>
    /// The decisions behind <see cref="DocumentationSync"/>, kept free of the Package Manager and the file
    /// system so they can be tested on their own: which folder a package is mirrored to, how links are
    /// rewritten, which mirrored folders are stale and what the generated pages look like.
    /// </summary>
    internal static class DocumentationSyncRules
    {
        /// <summary>Folder below the project's <c>Docs</c> folder that holds the mirrored package docs.</summary>
        internal const string PackagesFolder = "Packages";

        /// <summary>Line in Plastic's <c>ignore.conf</c> that keeps the mirrored docs out of version control.</summary>
        internal const string IgnoreLine = "Docs/Packages";

        /// <summary>Name of the Plastic ignore file in the project root.</summary>
        internal const string IgnoreFile = "ignore.conf";

        /// <summary>Package name prefix of every Base package. Only these are mirrored.</summary>
        internal const string PackageNamePrefix = "com.baseprojectpackages.";

        /// <summary>Where every Base package lives on GitHub, up to and including the packages folder.</summary>
        internal const string RepositoryPackagesUrl =
            "https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/";

        private const string DisplayNamePrefix = "Base ";
        private const string DocsSegment = DocumentationSource.PackageDocsFolder + "/";
        private const string NewLine = "\r\n";
        private const string ParentSegment = "..";
        private const string Space = " ";
        private const string EncodedSpace = "%20";

        private static readonly char[] InvalidNameCharacters = Path.GetInvalidFileNameChars();
        private static readonly Regex LinkTarget = new(@"\]\(([^)\s]+)", RegexOptions.Compiled);
        private static readonly Regex Scheme = new(@"^[a-zA-Z][a-zA-Z0-9+.-]*:", RegexOptions.Compiled);

        /// <summary>
        /// Returns the folder name a package is mirrored to: its display name without the leading
        /// <c>Base </c>, so <c>Base Utility</c> becomes <c>Utility</c>.
        /// </summary>
        /// <param name="displayName">The package's display name.</param>
        /// <returns>The mirror folder name, or empty when nothing usable is left.</returns>
        internal static string MirrorName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return string.Empty;

            string name = displayName.Trim();

            if (name.StartsWith(DisplayNamePrefix, StringComparison.Ordinal))
                name = name.Substring(DisplayNamePrefix.Length).Trim();

            StringBuilder builder = new(name.Length);

            foreach (char character in name)
                builder.Append(Array.IndexOf(InvalidNameCharacters, character) >= 0
                    ? '_'
                    : character);

            return builder.ToString();
        }

        /// <summary>
        /// Reads the package's folder in the repository from its documentation URL, which every Base
        /// package points at <c>.../Packages/&lt;Folder&gt;/Documentation~/index.md</c>.
        /// </summary>
        /// <param name="documentationUrl">The <c>documentationUrl</c> from the package manifest.</param>
        /// <param name="folder">The repository folder, for example <c>Utility</c>, or <c>null</c>.</param>
        /// <returns><c>true</c> when the URL points into the repository's packages folder.</returns>
        internal static bool TryGetRepositoryFolder(string documentationUrl, out string folder)
        {
            folder = null;

            if (string.IsNullOrEmpty(documentationUrl)
                || !documentationUrl.StartsWith(RepositoryPackagesUrl, StringComparison.OrdinalIgnoreCase))
                return false;

            string rest = documentationUrl.Substring(RepositoryPackagesUrl.Length);
            int slash = rest.IndexOf('/');

            if (slash <= 0)
                return false;

            folder = Uri.UnescapeDataString(rest.Substring(0, slash));

            return true;
        }

        /// <summary>
        /// Rewrites the link targets of one mirrored page. Links to another installed package's docs on
        /// GitHub become relative links to its mirrored copy; links to packages that are not installed
        /// stay as they are. Relative links that leave the docs folder (a README, a CHANGELOG) cannot
        /// work in the mirror, so they become GitHub links when the page's own package is known.
        /// </summary>
        /// <param name="markdown">The page's content.</param>
        /// <param name="pagePath">The page's path inside its docs folder, separated by <c>/</c>.</param>
        /// <param name="ownRepositoryFolder">The page's own package folder in the repository, or <c>null</c>.</param>
        /// <param name="mirrorByRepositoryFolder">
        /// Mirror folder name of every installed package, keyed by its repository folder.
        /// </param>
        /// <returns>The page with its links rewritten.</returns>
        internal static string RewriteLinks(string markdown, string pagePath, string ownRepositoryFolder,
            IReadOnlyDictionary<string, string> mirrorByRepositoryFolder)
        {
            if (string.IsNullOrEmpty(markdown))
                return markdown ?? string.Empty;

            string pageFolder = ParentOf(pagePath ?? string.Empty);
            int depth = pageFolder.Length == 0 ? 1 : pageFolder.Split('/').Length + 1;

            return LinkTarget.Replace(markdown, match =>
            {
                string target = match.Groups[1].Value;
                string rewritten = RewriteAbsolute(target, depth, mirrorByRepositoryFolder)
                    ?? RewriteEscaping(target, pageFolder, depth, ownRepositoryFolder, mirrorByRepositoryFolder)
                    ?? target;

                return "](" + rewritten;
            });
        }

        /// <summary>
        /// Returns the mirrored folders that no longer belong to an installed package and should be deleted.
        /// </summary>
        /// <param name="existingFolders">Names of the folders currently in <c>Docs/Packages</c>.</param>
        /// <param name="mirrorNames">Mirror folder names of the installed packages.</param>
        /// <returns>The folder names to delete, in the order they were given.</returns>
        internal static List<string> FoldersToDelete(IEnumerable<string> existingFolders,
            IEnumerable<string> mirrorNames)
        {
            HashSet<string> keep = new(mirrorNames ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            List<string> stale = new();

            if (existingFolders == null)
                return stale;

            foreach (string folder in existingFolders)
            {
                if (!string.IsNullOrEmpty(folder) && !keep.Contains(folder))
                    stale.Add(folder);
            }

            return stale;
        }

        /// <summary>
        /// Builds the generated <c>Docs/Packages/index.md</c> that links every mirrored package.
        /// </summary>
        /// <param name="packages">The mirrored packages. Display names are turned into mirror names here.</param>
        /// <returns>The page, with CRLF line endings and no final newline like every other docs page.</returns>
        internal static string BuildIndex(IEnumerable<DocumentationSource> packages)
        {
            List<DocumentationSource> sorted = new(packages ?? Array.Empty<DocumentationSource>());
            sorted.Sort((a, b) => string.Compare(MirrorName(a.DisplayName), MirrorName(b.DisplayName),
                StringComparison.OrdinalIgnoreCase));

            StringBuilder builder = new();
            builder.Append("# Packages").Append(NewLine).Append(NewLine);
            builder.Append("📦 **Packages** · docs of every Base package installed in this project")
                .Append(NewLine).Append(NewLine);
            builder.Append("Generated by the Documentation Sync in the Utility package. Do not edit by hand, "
                + "the next sync overwrites it.").Append(NewLine).Append(NewLine);

            if (sorted.Count == 0)
            {
                builder.Append("No Base package with docs is installed.");

                return builder.ToString();
            }

            builder.Append("| Package | Version |").Append(NewLine);
            builder.Append("| --- | --- |");

            foreach (DocumentationSource package in sorted)
            {
                string name = MirrorName(package.DisplayName);
                builder.Append(NewLine).Append("| [").Append(name).Append("](").Append(Encode(name)).Append('/')
                    .Append(DocumentationSource.StartPage).Append(") | ").Append(package.Version).Append(" |");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Adds <see cref="IgnoreLine"/> to the content of an <c>ignore.conf</c> unless it is already there.
        /// </summary>
        /// <param name="content">The current content of the file.</param>
        /// <param name="changed"><c>true</c> when the line had to be added.</param>
        /// <returns>The content to write back.</returns>
        internal static string EnsureIgnoreLine(string content, out bool changed)
        {
            content ??= string.Empty;

            foreach (string line in content.Split('\n'))
            {
                string trimmed = line.Trim().TrimStart('/').TrimEnd('/');

                if (string.Equals(trimmed, IgnoreLine, StringComparison.OrdinalIgnoreCase))
                {
                    changed = false;

                    return content;
                }
            }

            changed = true;
            string newLine = content.Contains("\r\n") || content.Length == 0 ? NewLine : "\n";
            bool endsWithNewLine = content.Length == 0 || content.EndsWith("\n", StringComparison.Ordinal);

            return content + (endsWithNewLine ? string.Empty : newLine) + IgnoreLine + newLine;
        }

        private static string RewriteAbsolute(string target, int depth,
            IReadOnlyDictionary<string, string> mirrorByRepositoryFolder)
        {
            if (mirrorByRepositoryFolder == null
                || !target.StartsWith(RepositoryPackagesUrl, StringComparison.OrdinalIgnoreCase))
                return null;

            string rest = target.Substring(RepositoryPackagesUrl.Length);
            int slash = rest.IndexOf('/');

            if (slash <= 0)
                return null;

            string folder = Uri.UnescapeDataString(rest.Substring(0, slash));
            string inside = rest.Substring(slash + 1);

            if (!inside.StartsWith(DocsSegment, StringComparison.Ordinal)
                || !mirrorByRepositoryFolder.TryGetValue(folder, out string mirror))
                return null;

            string path = inside.Substring(DocsSegment.Length);

            if (path.Length == 0 || path[0] == '#')
                path = DocumentationSource.StartPage + path;

            StringBuilder builder = new();

            for (int i = 0; i < depth; i++)
                builder.Append(ParentSegment).Append('/');

            return builder.Append(Encode(mirror)).Append('/').Append(path).ToString();
        }

        private static string RewriteEscaping(string target, string pageFolder, int depth,
            string ownRepositoryFolder, IReadOnlyDictionary<string, string> mirrorByRepositoryFolder)
        {
            if (string.IsNullOrEmpty(ownRepositoryFolder) || target.Length == 0 || target[0] == '#'
                || target[0] == '/' || Scheme.IsMatch(target))
                return null;

            List<string> segments = new() { ownRepositoryFolder, DocumentationSource.PackageDocsFolder };

            if (pageFolder.Length > 0)
                segments.AddRange(pageFolder.Split('/'));

            foreach (string segment in target.Split('/'))
            {
                if (segment == ParentSegment)
                {
                    if (segments.Count == 0)
                        return null;

                    segments.RemoveAt(segments.Count - 1);
                }
                else if (segment != "." && segment.Length > 0)
                    segments.Add(segment);
            }

            bool insideOwnDocs = segments.Count >= 2
                && string.Equals(segments[0], ownRepositoryFolder, StringComparison.OrdinalIgnoreCase)
                && segments[1] == DocumentationSource.PackageDocsFolder;

            if (segments.Count == 0 || insideOwnDocs)
                return null;

            string absolute = RepositoryPackagesUrl + string.Join("/", segments);

            return RewriteAbsolute(absolute, depth, mirrorByRepositoryFolder) ?? absolute;
        }

        private static string ParentOf(string path)
        {
            int slash = path.LastIndexOf('/');

            return slash < 0 ? string.Empty : path.Substring(0, slash);
        }

        private static string Encode(string name) => name.Replace(Space, EncodedSpace);
    }
}