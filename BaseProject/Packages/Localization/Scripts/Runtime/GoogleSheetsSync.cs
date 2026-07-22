using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.Reporting;

namespace Base.LocalizationPackage
{
    /// <summary>
    /// Syncs String Table Collections with Google Sheets based on the Google Sheets extension settings.
    /// </summary>
    public static class GoogleSheetsSync
    {
        /// <summary>
        /// Collects all String Table Collections that have a <see cref="GoogleSheetsExtension"/>.
        /// Scans the Asset Database, so cache the result instead of calling this repeatedly.
        /// </summary>
        /// <returns>All String Table Collections with a <see cref="GoogleSheetsExtension"/>.</returns>
        public static List<StringTableCollection> GetCollections()
        {
            List<StringTableCollection> result = new();

            foreach (StringTableCollection collection in LocalizationEditorSettings.GetStringTableCollections())
            {
                if (HasGoogleSheetsExtension(collection))
                    if (HasGoogleSheetsExtension(collection))
                        result.Add(collection);
            }

            return result;
        }

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
            List<GoogleSheetsExtension> extensions = new();

            foreach (CollectionExtension extension in collection.Extensions)
            {
                if (extension is GoogleSheetsExtension googleSheetsExtension)
                    extensions.Add(googleSheetsExtension);
            }

            if (extensions.Count == 0)
                return SyncResult.Fail("No Google Sheets extension.");

            // Validate every extension up front so a misconfigured one cannot leave the collection half synced.
            foreach (GoogleSheetsExtension extension in extensions)
            {
                if (extension.SheetsServiceProvider == null)
                    return SyncResult.Fail("No Sheets Service Provider set.");

                if (string.IsNullOrEmpty(extension.SpreadsheetId))
                    return SyncResult.Fail("No Spreadsheet Id set.");
            }

            foreach (GoogleSheetsExtension extension in extensions)
            {
                GoogleSheets google = new(extension.SheetsServiceProvider)
                {
                    SpreadSheetId = extension.SpreadsheetId
                };

                ProgressBarReporter reporter = new();

                if (direction == ESyncDirection.Pull)
                    google.PullIntoStringTableCollection(extension.SheetId, collection, extension.Columns,
                        extension.RemoveMissingPulledKeys, reporter, true);
                else
                    google.PushStringTableCollection(extension.SheetId, collection, extension.Columns, reporter);
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
            List<StringTableCollection> collections = GetCollections();

            if (collections.Count == 0)
            {
                EditorUtility.DisplayDialog("Localization",
                    "No collection with a Google Sheets extension found.", "OK");

                return;
            }

            if (direction == ESyncDirection.Push
                && !EditorUtility.DisplayDialog("Push to Google Sheets",
                    $"This overwrites the sheets with local data for {collections.Count} collection(s). Continue?",
                    "Push", "Cancel"))
                return;

            string title = direction == ESyncDirection.Pull
                ? "Pull from Google Sheets"
                : "Push to Google Sheets";

            int succeeded = 0;
            List<string> failed = new();

            try
            {
                for (int i = 0; i < collections.Count; i++)
                {
                    StringTableCollection collection = collections[i];
                    EditorUtility.DisplayProgressBar(title, collection.TableCollectionName,
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

        private static bool HasGoogleSheetsExtension(StringTableCollection collection)
        {
            foreach (CollectionExtension extension in collection.Extensions)
            {
                if (extension is GoogleSheetsExtension)
                    return true;
            }

            return false;
        }

        private static void Log(ESyncDirection direction, int succeeded, IReadOnlyList<string> failed)
        {
            if (failed.Count == 0)
            {
                CustomLogger.Log($"{direction} done for {succeeded} collection(s).", null);
                return;
            }

            CustomLogger.LogWarning($"{direction} done for {succeeded} collection(s). "
                + $"Skipped {failed.Count}:\n - {string.Join("\n - ", failed)}", null);
        }
    }
}