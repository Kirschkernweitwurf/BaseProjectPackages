using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.UtilityPackage.Editor.Documentation
{
    /// <summary>
    /// Mirrors the <c>Documentation~</c> folder of every installed Base package into the project's
    /// <c>Docs/Packages</c> folder, so one Markdown vault (for example Obsidian on <c>Docs</c>) shows the
    /// project docs and the package docs together.
    /// </summary>
    /// <remarks>
    /// Runs once per editor session, whenever packages change, and from the Sync button of the
    /// <see cref="DocumentationWindow"/>. Does nothing in a project without <c>Docs/index.md</c>, so
    /// projects that do not keep docs are never touched. Unchanged files are skipped, which keeps a sync
    /// cheap enough to run on every package change. The decisions live in
    /// <see cref="DocumentationSyncRules"/> so they can be tested without the Package Manager.
    /// </remarks>
    [InitializeOnLoad]
    internal static class DocumentationSync
    {
        private const string LogPrefix = "[Documentation Sync] ";
        private const string MarkdownExtension = ".md";
        private const string SessionKey = "Base.UtilityPackage.DocumentationSync.Done";

        private static readonly UTF8Encoding Utf8WithoutBom = new(false);

        static DocumentationSync()
        {
            if (Application.isBatchMode)
                return;

            Events.registeredPackages += OnRegisteredPackages;

            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);
            EditorApplication.delayCall += () => Sync();
        }

        /// <summary>
        /// Mirrors the docs of every installed Base package into <c>Docs/Packages</c>.
        /// </summary>
        /// <returns>
        /// The number of files and folders written or deleted, or <c>-1</c> when the project has no
        /// <c>Docs/index.md</c> and nothing was done.
        /// </returns>
        internal static int Sync()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);

            if (!DocumentationSource.TryFindFolder(projectRoot, DocumentationSource.ProjectDocsFolder,
                    out string docsRoot))
                return -1;

            try
            {
                return SyncInto(projectRoot, docsRoot);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Debug.LogWarning(LogPrefix + "Stopped: " + exception.Message);

                return 0;
            }
        }

        private static void OnRegisteredPackages(PackageRegistrationEventArgs args) => Sync();

        private static int SyncInto(string projectRoot, string docsRoot)
        {
            string packagesRoot = Path.Combine(docsRoot, DocumentationSyncRules.PackagesFolder);
            List<DocumentationSource> packages = FindPackages();
            Dictionary<string, string> mirrorByFolder = new(StringComparer.OrdinalIgnoreCase);

            foreach (DocumentationSource package in packages)
            {
                if (DocumentationSyncRules.TryGetRepositoryFolder(package.OnlineUrl, out string folder))
                    mirrorByFolder[folder] = DocumentationSyncRules.MirrorName(package.DisplayName);
            }

            Directory.CreateDirectory(packagesRoot);
            int changes = 0;

            foreach (DocumentationSource package in packages)
            {
                DocumentationSyncRules.TryGetRepositoryFolder(package.OnlineUrl, out string ownFolder);
                string target = Path.Combine(packagesRoot, DocumentationSyncRules.MirrorName(package.DisplayName));
                changes += MirrorFolder(package.Folder, target, ownFolder, mirrorByFolder);
            }

            IEnumerable<string> existing = Directory.GetDirectories(packagesRoot).Select(Path.GetFileName);
            IEnumerable<string> mirrorNames =
                packages.Select(package => DocumentationSyncRules.MirrorName(package.DisplayName));

            foreach (string stale in DocumentationSyncRules.FoldersToDelete(existing, mirrorNames))
            {
                Directory.Delete(Path.Combine(packagesRoot, stale), true);
                changes++;
            }

            string indexPath = Path.Combine(packagesRoot, DocumentationSource.StartPage);

            if (WriteIfChanged(indexPath, DocumentationSyncRules.BuildIndex(packages)))
                changes++;

            if (EnsureIgnored(projectRoot))
                changes++;

            if (changes > 0)
                Debug.Log($"{LogPrefix}Docs/Packages updated ({changes} changes, {packages.Count} packages).");

            return changes;
        }

        private static List<DocumentationSource> FindPackages()
        {
            List<DocumentationSource> packages = new();
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

            foreach (PackageInfo package in PackageInfo.GetAllRegisteredPackages())
            {
                if (!package.name.StartsWith(DocumentationSyncRules.PackageNamePrefix, StringComparison.Ordinal)
                    || !DocumentationSource.TryFindFolder(package.resolvedPath,
                        DocumentationSource.PackageDocsFolder, out string folder))
                    continue;

                string mirrorName = DocumentationSyncRules.MirrorName(package.displayName);

                if (mirrorName.Length == 0 || !names.Add(mirrorName))
                {
                    Debug.LogWarning($"{LogPrefix}Skipped {package.name}: its folder name is empty or taken.");

                    continue;
                }

                packages.Add(new DocumentationSource(package.displayName, package.version, folder,
                    package.documentationUrl));
            }

            return packages;
        }

        private static int MirrorFolder(string source, string target, string ownFolder,
            IReadOnlyDictionary<string, string> mirrorByFolder)
        {
            int changes = 0;
            HashSet<string> wanted = new(StringComparer.OrdinalIgnoreCase);

            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(source, file).Replace('\\', '/');

                if (IsHidden(relative))
                    continue;

                wanted.Add(relative);
                string destination = Path.Combine(target, relative);

                bool isPage = Path.GetExtension(file).Equals(MarkdownExtension, StringComparison.OrdinalIgnoreCase);
                bool written = isPage
                    ? WriteIfChanged(destination, DocumentationSyncRules.RewriteLinks(File.ReadAllText(file),
                        relative, ownFolder, mirrorByFolder))
                    : CopyIfChanged(file, destination);

                if (written)
                    changes++;
            }

            if (!Directory.Exists(target))
                return changes;

            foreach (string file in Directory.GetFiles(target, "*", SearchOption.AllDirectories))
            {
                if (wanted.Contains(Path.GetRelativePath(target, file).Replace('\\', '/')))
                    continue;

                File.Delete(file);
                changes++;
            }

            DeleteEmptyFolders(target);

            return changes;
        }

        private static bool IsHidden(string relative) =>
            relative.Split('/').Any(part => part.Length > 0 && part[0] == '.');

        private static bool WriteIfChanged(string path, string content)
        {
            if (File.Exists(path) && File.ReadAllText(path) == content)
                return false;

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content, Utf8WithoutBom);

            return true;
        }

        private static bool CopyIfChanged(string source, string destination)
        {
            if (File.Exists(destination) && SameBytes(source, destination))
                return false;

            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(source, destination, true);

            return true;
        }

        private static bool SameBytes(string a, string b)
        {
            if (new FileInfo(a).Length != new FileInfo(b).Length)
                return false;

            byte[] first = File.ReadAllBytes(a);
            byte[] second = File.ReadAllBytes(b);

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                    return false;
            }

            return true;
        }

        private static void DeleteEmptyFolders(string root)
        {
            foreach (string folder in Directory.GetDirectories(root))
            {
                DeleteEmptyFolders(folder);

                if (!Directory.EnumerateFileSystemEntries(folder).Any())
                    Directory.Delete(folder);
            }
        }

        private static bool EnsureIgnored(string projectRoot)
        {
            string path = Path.Combine(projectRoot, DocumentationSyncRules.IgnoreFile);

            if (!File.Exists(path))
                return false;

            string content = File.ReadAllText(path);
            string updated = DocumentationSyncRules.EnsureIgnoreLine(content, out bool changed);

            if (!changed)
                return false;

            File.WriteAllText(path, updated, Utf8WithoutBom);
            Debug.Log($"{LogPrefix}Added \"{DocumentationSyncRules.IgnoreLine}\" to "
                + $"{DocumentationSyncRules.IgnoreFile}.");

            return true;
        }
    }
}