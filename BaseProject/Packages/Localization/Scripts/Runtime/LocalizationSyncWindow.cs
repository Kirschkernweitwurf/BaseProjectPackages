#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace Tools.Localization
{
    /// <summary>
    /// A custom Unity Editor window for syncing String Table Collections with Google Sheets.
    /// </summary>
    public class LocalizationSyncWindow : EditorWindow
    {
        private IReadOnlyList<StringTableCollection> _collections = new List<StringTableCollection>();
        private Vector2 _scroll;

        private void OnEnable() => Refresh();

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("String Table Collections", EditorStyles.boldLabel);
                if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                    Refresh();
            }

            if (_collections.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No collection with a Google Sheets extension found.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Pull All", GUILayout.Height(26)))
                    GoogleSheetsSync.SyncAll(ESyncDirection.Pull);
                if (GUILayout.Button("Push All", GUILayout.Height(26)))
                    GoogleSheetsSync.SyncAll(ESyncDirection.Push);
            }

            EditorGUILayout.Space();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (StringTableCollection collection in _collections)
            {
                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    EditorGUILayout.LabelField(collection.TableCollectionName);
                    if (GUILayout.Button("Pull", GUILayout.Width(60)))
                        Run(collection, ESyncDirection.Pull);
                    if (GUILayout.Button("Push", GUILayout.Width(60)))
                        Run(collection, ESyncDirection.Push);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Opens the Localization Sync window. Tools &gt; Base Packages &gt; Localization &gt; Open Sync Window.
        /// </summary>
        public static void Open()
        {
            LocalizationSyncWindow window = GetWindow<LocalizationSyncWindow>("Localization Sync");
            window.minSize = new Vector2(380, 220);
            window.Refresh();
            window.Show();
        }

        private void Refresh() => _collections = GoogleSheetsSync.Collections;

        private static void Run(StringTableCollection collection, ESyncDirection direction)
        {
            SyncResult result = GoogleSheetsSync.Sync(collection, direction);
            if (result.Success)
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"[Localization] {direction} '{collection.TableCollectionName}' done.");
            }
            else
            {
                Debug.LogWarning(
                    $"[Localization] {direction} '{collection.TableCollectionName}' skipped: {result.Message}");
            }
        }
    }
}
#endif