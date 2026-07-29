using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.Reporting;

namespace Base.LocalizationPackage.Editor
{
    /// <summary>
    /// Syncs String Table Collections with Google Sheets based on the Google Sheets extension settings.
    /// </summary>
    public static class GoogleSheetsSync
    {
        /// <summary>
        /// Shown when no String Table Collection with a <see cref="GoogleSheetsExtension"/> exists.
        /// </summary>
        internal const string NoCollectionsMessage = "No collection with a Google Sheets extension found.";

        private const string CancelButton = "Cancel";
        private const string ConfirmPushMessage =
            "This overwrites the sheets with local data for {0} collection(s). Continue?";
        private const string DialogTitle = "Localization";
        private const string MissingCollectionMessage = "No collection.";
        private const string MissingExtensionMessage = "No Google Sheets extension.";
        private const string MissingProviderMessage = "No Sheets Service Provider set.";
        private const string MissingSpreadsheetIdMessage = "No Spreadsheet Id set.";
        private const string OkButton = "OK";
        private const string PullTitle = "Pull from Google Sheets";
        private const string PushButton = "Push";
        private const string PushTitle = "Push to Google Sheets";

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
                    result.Add(collection);
            }

            return result;
        }

        /// <summary>
        /// Asks the user to confirm a push, which overwrites the sheet data.
        /// </summary>
        /// <param name="collectionCount">The number of collections that would be pushed.</param>
        /// <returns><c>true</c> if the user confirmed the push, otherwise <c>false</c>.</returns>
        public static bool IsPushConfirmed(int collectionCount) => EditorUtility.DisplayDialog(PushTitle,
            string.Format(ConfirmPushMessage, collectionCount), PushButton, CancelButton);

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
            if (collection == null)
            {
                CustomLogger.LogError($"{nameof(collection)} is null.", null);
                return SyncResult.Fail(MissingCollectionMessage);
            }

            List<GoogleSheetsExtension> extensions = GetExtensions(collection);

            if (extensions.Count == 0)
                return SyncResult.Fail(MissingExtensionMessage);

            // Validate every extension up front so a misconfigured one cannot leave the collection half synced.
            foreach (GoogleSheetsExtension extension in extensions)
            {
                if (extension.SheetsServiceProvider == null)
                    return SyncResult.Fail(MissingProviderMessage);

                if (string.IsNullOrEmpty(extension.SpreadsheetId))
                    return SyncResult.Fail(MissingSpreadsheetIdMessage);
            }

            ProgressBarReporter reporter = new();

            foreach (GoogleSheetsExtension extension in extensions)
            {
                GoogleSheets google = new(extension.SheetsServiceProvider)
                {
                    SpreadSheetId = extension.SpreadsheetId
                };

                if (direction == ESyncDirection.Pull)
                    google.PullIntoStringTableCollection(extension.SheetId, collection, extension.Columns,
                        extension.RemoveMissingPulledKeys, reporter, true);
                else
                    google.PushStringTableCollection(extension.SheetId, collection, extension.Columns,
                        reporter);
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
                EditorUtility.DisplayDialog(DialogTitle, NoCollectionsMessage, OkButton);
                return;
            }

            if (direction == ESyncDirection.Push
                && !IsPushConfirmed(collections.Count))
                return;

            string title = direction == ESyncDirection.Pull
                ? PullTitle
                : PushTitle;

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

        private static List<GoogleSheetsExtension> GetExtensions(StringTableCollection collection)
        {
            List<GoogleSheetsExtension> result = new();

            foreach (CollectionExtension extension in collection.Extensions)
            {
                if (extension is GoogleSheetsExtension googleSheetsExtension)
                    result.Add(googleSheetsExtension);
            }

            return result;
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