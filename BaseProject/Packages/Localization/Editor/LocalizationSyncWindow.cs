using System;
using System.Collections.Generic;
using Base.EditorUiPackage;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace Base.LocalizationPackage.Editor
{
    /// <summary>
    /// A custom Unity Editor window for syncing String Table Collections with Google Sheets.
    /// </summary>
    internal sealed class LocalizationSyncWindow : EditorWindow
    {
        private const string Description = "Pulls String Table Collections down from Google Sheets, or "
            + "pushes the project's entries back up. A push overwrites the sheet.";
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

        private readonly EditorWindowStyles _styles = new();

        private IReadOnlyList<StringTableCollection> _collections = Array.Empty<StringTableCollection>();
        private Vector2 _scroll;
        private int _rowIndex;

#region Unity Callbacks
        private void OnEnable() => Refresh();

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);

            DrawCollectionsToolbar();

            if (_collections.Count == 0)
            {
                EditorWindowChrome.DrawEmptyState(_styles, EditorIcons.Success, HeaderLabel,
                    GoogleSheetsSync.NoCollectionsMessage);

                return;
            }

            DrawSyncAllButtons();
            EditorGUILayout.Space(EditorMetrics.ItemGap);

            _rowIndex = 0;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorWindowChrome.BeginCard(_styles);

            foreach (StringTableCollection collection in _collections)
                DrawCollection(collection);

            EditorWindowChrome.EndCard();

            EditorGUILayout.EndScrollView();
        }

        private void OnDisable() => _styles.Dispose();
#endregion

        /// <summary>
        /// Opens the Localization Sync window.
        /// It is also reachable via the menu items in <see cref="LocalizationMenu"/>.
        /// </summary>
        internal static void Open()
        {
            LocalizationSyncWindow window = GetWindow<LocalizationSyncWindow>(WindowTitle);
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
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

        private void DrawSyncAllButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (EditorWindowChrome.PrimaryButton(_styles, PullAllLabel,
                        GUILayout.Height(SyncAllButtonHeight)))
                    GoogleSheetsSync.SyncAll(ESyncDirection.Pull);

                GUILayout.Space(EditorMetrics.TightGap);

                if (EditorWindowChrome.SecondaryButton(_styles, PushAllLabel,
                        GUILayout.Height(SyncAllButtonHeight)))
                    GoogleSheetsSync.SyncAll(ESyncDirection.Push);
            }
        }

        // Striped by hand rather than through EditorRows, because the rows are laid out by the
        // layout system here and the background has to be reserved before the controls go in.
        private void DrawCollection(StringTableCollection collection)
        {
            Rect row = EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorTableStyles.RowHeight));

            EditorRows.DrawRowBackground(row, _rowIndex);
            _rowIndex++;

            GUILayout.Label(collection.TableCollectionName, _styles.Name);
            GUILayout.FlexibleSpace();

            if (EditorWindowChrome.SecondaryButton(_styles, PullLabel, GUILayout.Width(SyncButtonWidth)))
                Run(collection, ESyncDirection.Pull);

            GUILayout.Space(EditorMetrics.TightGap);

            if (EditorWindowChrome.SecondaryButton(_styles, PushLabel, GUILayout.Width(SyncButtonWidth)))
                Run(collection, ESyncDirection.Push);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCollectionsToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(HeaderLabel, _styles.SectionHeader);
                GUILayout.FlexibleSpace();

                if (EditorWindowChrome.SecondaryButton(_styles, RefreshLabel,
                        GUILayout.Width(RefreshButtonWidth)))
                    Refresh();
            }

            EditorGUILayout.Space(EditorMetrics.TightGap);
        }

        private void Refresh() => _collections = GoogleSheetsSync.GetCollections();
    }
}