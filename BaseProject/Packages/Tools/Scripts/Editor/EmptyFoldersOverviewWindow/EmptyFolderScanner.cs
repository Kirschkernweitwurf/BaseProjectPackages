using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Base.EmptyFoldersPackage
{
    /// <summary>
    /// Finds empty folders under Assets. A folder counts as empty when it holds no assets
    /// and every subfolder is empty too. Only the top-most empty folders are returned, since
    /// deleting one also removes its nested empty folders.
    /// </summary>
    public static class EmptyFolderScanner
    {
        private const string RootFolder = "Assets";

        public static List<EmptyFolderEntry> Scan()
        {
            List<string> allFolders = new List<string>();
            CollectFolders(RootFolder, allFolders);

            string projectPath = Application.dataPath
                .Substring(0, Application.dataPath.Length - RootFolder.Length);

            HashSet<string> emptyFolders = new HashSet<string>();

            foreach (string folder in allFolders)
            {
                if (IsRecursivelyEmpty(projectPath + folder))
                {
                    emptyFolders.Add(folder);
                }
            }

            List<EmptyFolderEntry> results = new List<EmptyFolderEntry>();

            foreach (string folder in emptyFolders)
            {
                string parent = GetParentFolder(folder);

                // Skip folders that live under an empty ancestor, that ancestor covers them.
                if (parent != null && emptyFolders.Contains(parent))
                {
                    continue;
                }

                int nested = CountSubtree(folder, allFolders);
                results.Add(new EmptyFolderEntry(folder, nested));
            }

            results.Sort((first, second) => string.Compare(first.Path, second.Path, StringComparison.Ordinal));
            return results;
        }

        private static void CollectFolders(string folder, List<string> output)
        {
            foreach (string sub in AssetDatabase.GetSubFolders(folder))
            {
                output.Add(sub);
                CollectFolders(sub, output);
            }
        }

        private static bool IsRecursivelyEmpty(string absolutePath)
        {
            foreach (string file in Directory.GetFiles(absolutePath))
            {
                if (IsMeaningfulFile(file))
                {
                    return false;
                }
            }

            foreach (string directory in Directory.GetDirectories(absolutePath))
            {
                if (IsHidden(Path.GetFileName(directory)))
                {
                    continue;
                }

                if (!IsRecursivelyEmpty(directory))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsMeaningfulFile(string file)
        {
            string name = Path.GetFileName(file);

            if (name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !IsHidden(name);
        }

        private static bool IsHidden(string name)
        {
            return name.StartsWith(".") || name.EndsWith("~");
        }

        private static string GetParentFolder(string folder)
        {
            int slash = folder.LastIndexOf('/');
            return slash <= 0 ? null : folder.Substring(0, slash);
        }

        private static int CountSubtree(string folder, List<string> allFolders)
        {
            string prefix = folder + "/";
            return allFolders.Count(candidate => candidate == folder || candidate.StartsWith(prefix));
        }
    }
}
