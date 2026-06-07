#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.Reporting;
using UnityEngine;

namespace Tools.Localization
{
    /// <summary>
    /// Syncs String Table Collections with Google Sheets based on the Google Sheets extension settings.
    /// </summary>
    public static class GoogleSheetsSync
    {
        public static IReadOnlyList<StringTableCollection> Collections =>
            LocalizationEditorSettings.GetStringTableCollections()
                .Where(c => c.Extensions.Any(e => e is GoogleSheetsExtension))
                .ToList();

        /// <summary>
        /// Syncs a String Table Collection with Google Sheets based on the Google Sheets extension settings.
        /// </summary>
        /// <param name="collection">The String Table Collection to sync.</param>
        /// <param name="direction">
        /// The direction to sync.
        /// Pull will overwrite local data with the sheet data, while Push will overwrite the sheet with local data.
        /// </param>
        /// <returns>A <see cref="SyncResult"/> indicating success or failure and an error message if failed.</returns>
        public static SyncResult Sync(StringTableCollection collection, ESyncDirection direction)
        {
            List<GoogleSheetsExtension> extensions =
                collection.Extensions.OfType<GoogleSheetsExtension>().ToList();

            if (extensions.Count == 0)
                return SyncResult.Fail("No Google Sheets extension.");

            foreach (GoogleSheetsExtension extension in extensions)
            {
                if (extension.SheetsServiceProvider == null)
                    return SyncResult.Fail("No Sheets Service Provider set.");

                if (string.IsNullOrEmpty(extension.SpreadsheetId))
                    return SyncResult.Fail("No Spreadsheet Id set.");

                GoogleSheets google = new(extension.SheetsServiceProvider)
                {
                    SpreadSheetId = extension.SpreadsheetId
                };

                ProgressBarReporter reporter = new();

                if (direction == ESyncDirection.Pull)
                    google.PullIntoStringTableCollection(
                        extension.SheetId, collection, extension.Columns,
                        extension.RemoveMissingPulledKeys, reporter, true);
                else
                    google.PushStringTableCollection(
                        extension.SheetId, collection, extension.Columns, reporter);
            }

            if (direction == ESyncDirection.Pull)
                EditorUtility.SetDirty(collection);

            return SyncResult.Ok();
        }

        /// <summary>
        /// Syncs all String Table Collections with Google Sheets based on the Google Sheets extension settings.
        /// </summary>
        /// <param name="direction">
        /// The direction to sync.
        /// Pull will overwrite local data with the sheet data, while Push will overwrite the sheet with local data.
        /// </param>
        public static void SyncAll(ESyncDirection direction)
        {
            IReadOnlyList<StringTableCollection> collections = Collections;

            if (collections.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Localization",
                    "No collection with a Google Sheets extension found.", "OK");
                return;
            }

            if (direction == ESyncDirection.Push &&
                !EditorUtility.DisplayDialog(
                    "Push to Google Sheets",
                    $"This overwrites the sheets with local data for {collections.Count} collection(s). Continue?",
                    "Push", "Cancel"))
                return;

            int succeeded = 0;
            List<string> failed = new();

            try
            {
                for (int i = 0; i < collections.Count; i++)
                {
                    StringTableCollection collection = collections[i];
                    EditorUtility.DisplayProgressBar(
                        $"{direction} from Google Sheets",
                        collection.TableCollectionName,
                        (float)i / collections.Count);

                    SyncResult result = Sync(collection, direction);
                    if (result.Success)
                        succeeded++;
                    else
                        failed.Add($"{collection.TableCollectionName}: {result.Message}");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            Log(direction, succeeded, failed);
        }

        private static void Log(ESyncDirection direction, int succeeded, List<string> failed)
        {
            if (failed.Count == 0)
            {
                Debug.Log($"[Localization] {direction} done for {succeeded} collection(s).");
                return;
            }

            Debug.LogWarning(
                $"[Localization] {direction} done for {succeeded} collection(s). " +
                $"Skipped {failed.Count}:\n - {string.Join("\n - ", failed)}");
        }
    }
}
#endif