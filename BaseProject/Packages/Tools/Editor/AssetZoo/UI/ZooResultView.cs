using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.AssetZoo.Generation;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// Draws the outcome of the last auto generation run as a card: an icon for how it went, the one
    /// line summary, and the report about prefixes and skipped assets under it. Shared by the window
    /// and the config inspector so both say the same thing in the same shape.
    /// </summary>
    internal static class ZooResultView
    {
        // The built-in icons are small bitmaps. The size asked for here is only the space they are
        // given; EditorIcons settles on the size they are actually drawn at.
        private const float IconSize = 16f;

        /// <summary>
        /// Draws the result card.
        /// </summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="result">The result of the last run.</param>
        public static void Draw(EditorWindowStyles styles, ZooGenerationResult result)
        {
            if (styles == null)
                return;

            EditorWindowChrome.BeginCard(styles);

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

            EditorWindowChrome.EndCard();
        }

        private static Texture Icon(ZooGenerationResult result) => result.Success
            ? EditorIcons.Success
            : EditorIcons.Error;
    }
}