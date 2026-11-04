using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.AssetZoo.Generation;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// Says what the last scan found and what it left out. Part of the card the scan was started from, so the answer
    /// appears next to the question.
    /// </summary>
    internal static class ZooResultView
    {
        // The built-in icons are small bitmaps. The size asked for here is only the space they are
        // given; EditorIcons settles on the size they are actually drawn at.
        private const float IconSize = 16f;

        /// <summary>
        /// Draws the result lines.
        /// </summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="result">The result of the last run.</param>
        public static void Draw(EditorWindowStyles styles, ZooGenerationResult result)
        {
            if (styles == null)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                Rect icon = GUILayoutUtility.GetRect(IconSize, IconSize, GUILayout.Width(IconSize),
                    GUILayout.Height(IconSize));

                EditorIcons.Draw(icon, Icon(result));

                GUILayout.Space(EditorMetrics.TightGap);

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Label(result.Message, styles.SectionHeader);

                    if (result.HasDetails)
                        GUILayout.Label(result.Details, styles.Description);
                }
            }
        }

        private static Texture Icon(ZooGenerationResult result) => result.Success
            ? EditorIcons.Success
            : EditorIcons.Error;
    }
}