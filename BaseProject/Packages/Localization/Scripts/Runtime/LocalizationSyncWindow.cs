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
        private const float MinWindowHeight = 220f;
        private const float MinWindowWidth = 380f;
        private const float RefreshButtonWidth = 70f;
        private const float SyncAllButtonHeight = 26f;
        private const float SyncButtonWidth = 60f;

        private IReadOnlyList<StringTableCollection> _collections = Array.Empty<StringTableCollection>();
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable() => Refresh();

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("String Table Collections", EditorStyles.boldLabel);
                if (GUILayout.Button("Refresh", GUILayout.Width(RefreshButtonWidth)))
                    Refresh();
            }

            if (_collections.Count == 0)
            {
                EditorGUILayout.HelpBox("No collection with a Google Sheets extension found.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Pull All", GUILayout.Height(SyncAllButtonHeight)))
                    GoogleSheetsSync.SyncAll(ESyncDirection.Pull);

                if (GUILayout.Button("Push All", GUILayout.Height(SyncAllButtonHeight)))
                    GoogleSheetsSync.SyncAll(ESyncDirection.Push);
            }

            EditorGUILayout.Space();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (StringTableCollection collection in _collections)
            {
                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    EditorGUILayout.LabelField(collection.TableCollectionName);
                    if (GUILayout.Button("Pull", GUILayout.Width(SyncButtonWidth)))
                        Run(collection, ESyncDirection.Pull);

                    if (GUILayout.Button("Push", GUILayout.Width(SyncButtonWidth)))
                        Run(collection, ESyncDirection.Push);
                }
            }

            EditorGUILayout.EndScrollView();
        }
#endregion

        /// <summary>
        /// Opens the Localization Sync window. Also reachable via the menu items in <see cref="LocalizationMenu"/>.
        /// </summary>
        public static void Open()
        {
            LocalizationSyncWindow window = GetWindow<LocalizationSyncWindow>("Localization Sync");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Refresh();
            window.Show();
        }

        private static void Run(StringTableCollection collection, ESyncDirection direction)
        {
            SyncResult result = GoogleSheetsSync.Sync(collection, direction);
            if (result.Success)
            {
                AssetDatabase.SaveAssets();
                CustomLogger.Log($"{direction} '{collection.TableCollectionName}' done.", null);
            }
            else
            {
                CustomLogger.LogWarning($"{direction} '{collection.TableCollectionName}' skipped: {result.Message}",
                    null);
            }
        }

        private void Refresh() => _collections = GoogleSheetsSync.GetCollections();
    }
}