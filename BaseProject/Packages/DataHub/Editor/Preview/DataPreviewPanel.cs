using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Extensions;
using Base.DataHubPackage.Editor.Table;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Preview
{
    /// <summary>
    /// The Preview tab: how the selected asset looks and how it is doing, at a glance. The project's
    /// <see cref="IDataPreview"/> cards come first, Unity's own asset preview stands in for types without one,
    /// and the checks of the row close the tab, so what needs fixing is read next to what it looks like.
    /// </summary>
    internal sealed class DataPreviewPanel
    {
        private const string ChecksPassedText = "All checks pass.";
        private const string ChecksTitle = "Checks";
        private const string FirstOfFormat = "Showing the first of {0} selected assets.";
        private const string NoPreviewFormat = "No preview for this kind of asset yet. A project can add one with "
            + "{0}.";
        private const string NothingSelectedHint = "Its preview shows up here.";
        private const string NothingSelectedMessage = "Select a row";
        private const float PreviewHeight = 240f;
        private const string PreviewTitle = "Preview";
        private const float ThumbnailSize = 64f;

        private readonly List<IDataPreview> _previews = new();
        private readonly DataHubRegistry _registry;

        private UnityEditor.Editor _editor;
        private Vector2 _scroll;

        /// <summary>Creates the tab.</summary>
        /// <param name="registry">The extensions, for the project's preview cards.</param>
        internal DataPreviewPanel(DataHubRegistry registry) => _registry = registry;

        /// <summary>Draws the preview of the first selected asset.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="targets">The selected assets.</param>
        /// <param name="row">The table row of the first selected asset, for its checks, or null.</param>
        /// <param name="styles">The built window styles.</param>
        internal void Draw(Rect area, Object[] targets, DataRow row, DataHubStyles styles)
        {
            GUILayout.BeginArea(area);
            DrawContent(targets, row, styles);
            GUILayout.EndArea();
        }

        /// <summary>Releases the Unity preview. Call when the window closes.</summary>
        internal void Dispose()
        {
            if (_editor != null)
                Object.DestroyImmediate(_editor);

            _editor = null;
        }

        private static void DrawChecks(DataRow row, DataHubStyles styles)
        {
            if (row == null)
                return;

            GUILayout.Label(ChecksTitle, styles.SectionHeader);
            EditorGUILayout.BeginVertical(styles.Card);

            if (!row.HasProblems
                && !row.HasWarnings)
                GUILayout.Label(new GUIContent(ChecksPassedText, EditorIcons.Success), styles.CheckLine);

            foreach (string problem in row.Problems)
                GUILayout.Label(new GUIContent(problem, EditorIcons.Error), styles.CheckLine);

            foreach (string warning in row.Warnings)
                GUILayout.Label(new GUIContent(warning, EditorIcons.Warning), styles.CheckLine);

            EditorGUILayout.EndVertical();
        }

        private void DrawContent(Object[] targets, DataRow row, DataHubStyles styles)
        {
            // A deleted asset stays selected until the next rebuild, a moment later.
            if (targets.Length == 0
                || Array.Exists(targets, target => target == null))
            {
                EditorWindowChrome.DrawEmptyState(styles, null, NothingSelectedMessage, NothingSelectedHint);
                return;
            }

            Object asset = targets[0];

            _previews.Clear();
            _registry.CollectPreviews(asset.GetType(), _previews);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (targets.Length > 1)
            {
                GUILayout.Label(string.Format(FirstOfFormat, targets.Length), styles.Detail);
                GUILayout.Space(EditorMetrics.ItemGap);
            }

            if (_previews.Count == 0)
                DataHubGui.ExtensionCard(PreviewTitle, asset, DrawUnityPreview, styles);

            foreach (IDataPreview preview in _previews)
                DataHubGui.ExtensionCard(preview.Title, asset, preview.Draw, styles);

            DrawChecks(row, styles);

            EditorGUILayout.EndScrollView();
        }

        // Models, textures, sprites and prefabs have a preview of their own. A plain data asset has none,
        // so it shows its icon and a pointer to the extension that would give it one.
        private void DrawUnityPreview(Object asset)
        {
            UnityEditor.Editor.CreateCachedEditor(asset, null, ref _editor);

            if (_editor.HasPreviewGUI())
            {
                Rect area = GUILayoutUtility.GetRect(0f, PreviewHeight, GUILayout.ExpandWidth(true));
                _editor.OnInteractivePreviewGUI(area, GUIStyle.none);

                return;
            }

            Rect line = GUILayoutUtility.GetRect(0f, ThumbnailSize, GUILayout.ExpandWidth(true));
            Rect icon = new(line.x + (line.width - ThumbnailSize) * 0.5f, line.y, ThumbnailSize, ThumbnailSize);

            DataHubGui.Icon(icon, AssetPreview.GetMiniThumbnail(asset));

            GUILayout.Space(EditorMetrics.ItemGap);
            GUILayout.Label(string.Format(NoPreviewFormat, nameof(IDataPreview)), EditorStyles.centeredGreyMiniLabel);
        }
    }
}