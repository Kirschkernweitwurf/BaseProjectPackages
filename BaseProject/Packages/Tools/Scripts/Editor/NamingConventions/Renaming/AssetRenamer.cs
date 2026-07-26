using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.NamingConventions.Data;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.NamingConventions.Renaming
{
    /// <summary>
    /// Applies the suggested file names to the asset database and records every applied rename in
    /// the <see cref="AssetNamingHistoryStore"/>. Renaming an asset keeps its GUID, so references
    /// survive, which is why assets can be fixed without touching any code.
    /// </summary>
    public static class AssetRenamer
    {
        private const char PathSeparator = '/';

        /// <summary>Renames a single asset. Returns true when the rename went through.</summary>
        public static bool Rename(AssetNamingViolation violation)
        {
            if (violation == null)
            {
                CustomLogger.LogError($"Renaming needs an {nameof(AssetNamingViolation)}.", null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(violation.Suggestion))
            {
                CustomLogger.LogWarning($"Skipped {violation.AssetPath} because the new name is empty.", null);
                return false;
            }

            if (violation.Suggestion == violation.CurrentName)
                return false;

            string error = AssetDatabase.RenameAsset(violation.AssetPath, violation.Suggestion);

            if (!string.IsNullOrEmpty(error))
            {
                CustomLogger.LogError($"Renaming {violation.AssetPath} failed: {error}", null);
                return false;
            }

            AssetNamingHistoryStore.Add(violation.CurrentName, violation.Suggestion,
                BuildNewPath(violation));

            return true;
        }

        /// <summary>Renames every entry of the list and returns how many assets were renamed.</summary>
        public static int RenameAll(IReadOnlyList<AssetNamingViolation> violations)
        {
            if (violations == null)
            {
                CustomLogger.LogError("Renaming needs a list of violations.", null);
                return 0;
            }

            int renamed = 0;

            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (AssetNamingViolation violation in violations)
                {
                    if (!Rename(violation))
                        continue;

                    renamed++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return renamed;
        }

        /// <summary>Path of the asset after the rename, kept for the history.</summary>
        private static string BuildNewPath(AssetNamingViolation violation)
        {
            string directory = Path.GetDirectoryName(violation.AssetPath);
            string extension = Path.GetExtension(violation.AssetPath);

            if (string.IsNullOrEmpty(directory))
                return violation.Suggestion + extension;

            return directory.Replace('\\', PathSeparator) + PathSeparator + violation.Suggestion + extension;
        }
    }
}
