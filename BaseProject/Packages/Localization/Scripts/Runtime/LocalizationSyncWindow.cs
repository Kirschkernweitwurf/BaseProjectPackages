using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace Base.LocalizationPackage
{
    /// <summary>
    /// A custom Unity Editor window for syncing String Table Collections with Google Sheets.
    /// </summary>
    public sealed class LocalizationSyncWindow : EditorWindow
    {
        private const string BoxStyle = "box";
        private const string HeaderLabel = "String Table Collections";
        private const float MinWindowHeight = 220f;
        private const float MinWindowWidth = 380f;
        private const string PullAllLabel = "Pull All";
        private const string PullLabel = "Pull";
        private const string PushAllLabel = "Push All";
        private const string PushLabel = "Push";
        private const float RefreshButtonWidth = 70f;
        private const string RefreshLabel = "Refresh";
        private const int SingleCollection = 1;
        private const float SyncAllButtonHeight = 26f;
        private const float SyncButtonWidth = 60f;
        private const string WindowTitle = "Localization Sync";

        private IReadOnlyList<StringTableCollection> _collections = Array.Empty<StringTableCollection>();
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable() => Refresh();

        private void OnGUI()
        {
            DrawHeader();

            if (_collections.Count == 0)
            {
                EditorGUILayout.HelpBox(GoogleSheetsSync.NoCollectionsMessage, MessageType.Info);
                return;
            }

            DrawSyncAllButtons();
            EditorGUILayout.Space();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (StringTableCollection collection in _collections)
                DrawCollection(collection);

            EditorGUILayout.EndScrollView();
        }
#endregion

        /// <summary>
        /// Opens the Localization Sync window. It is also reachable via the menu items in <see cref="LocalizationMenu"/>.
        /// </summary>
        public static void Open()
        {
            LocalizationSyncWindow window = GetWindow<LocalizationSyncWindow>(WindowTitle);
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
        }

        private static void DrawSyncAllButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(PullAllLabel, GUILayout.Height(SyncAllButtonHeight)))
                    GoogleSheetsSync.SyncAll(ESyncDirection.Pull);

                if (GUILayout.Button(PushAllLabel, GUILayout.Height(SyncAllButtonHeight)))
                    GoogleSheetsSync.SyncAll(ESyncDirection.Push);
            }
        }

        private static void DrawCollection(StringTableCollection collection)
        {
            using (new EditorGUILayout.HorizontalScope(BoxStyle))
            {
                EditorGUILayout.LabelField(collection.TableCollectionName);

                if (GUILayout.Button(PullLabel, GUILayout.Width(SyncButtonWidth)))
                    Run(collection, ESyncDirection.Pull);

                if (GUILayout.Button(PushLabel, GUILayout.Width(SyncButtonWidth)))
                    Run(collection, ESyncDirection.Push);
            }
        }

        private static void Run(StringTableCollection collection, ESyncDirection direction)
        {
            // A single push is just as destructive as a push of all collections, so it is confirmed the same way.
            if (direction == ESyncDirection.Push
                && !GoogleSheetsSync.IsPushConfirmed(SingleCollection))
                return;

            SyncResult result = GoogleSheetsSync.Sync(collection, direction);

            if (result.Success)
            {
                AssetDatabase.SaveAssets();
                CustomLogger.Log($"{direction} '{collection.TableCollectionName}' done.", collection);
                return;
            }

            CustomLogger.LogWarning($"{direction} '{collection.TableCollectionName}' skipped: {result.Message}",
                collection);
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(HeaderLabel, EditorStyles.boldLabel);

                if (GUILayout.Button(RefreshLabel, GUILayout.Width(RefreshButtonWidth)))
                    Refresh();
            }
        }

        private void Refresh() => _collections = GoogleSheetsSync.GetCollections();
    }
}