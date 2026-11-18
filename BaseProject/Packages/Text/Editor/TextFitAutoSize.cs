using Base.UtilityPackage.Logging;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// The one-click fix for a box without a text style: turn on auto size, so a long translation shrinks a little
    /// instead of spilling out of the box. A styled box keeps the locked size of its style and is fixed there.
    /// </summary>
    internal static class TextFitAutoSize
    {
        private const float MinSizeFactor = 2f / 3f;
        private const string MissingTextFormat = "No {0} on '{1}', so auto size could not be turned on.";
        private const string UndoName = "Auto Size Text";

        /// <summary>True when auto size can fix <paramref name="box"/>: no style and it does not shrink yet.</summary>
        internal static bool CanFix(TextBox box) => !box.HasStyle
            && !box.Text.enableAutoSizing;

        /// <summary>
        /// The smallest size the box may shrink to. Two thirds of its size keeps it readable next to its neighbors.
        /// </summary>
        internal static float MinSizeOf(TextBox box) => Mathf.Floor(box.Text.fontSize * MinSizeFactor);

        /// <summary>
        /// Opens the box in its prefab or scene and turns on auto size there, from its size down to
        /// <see cref="MinSizeOf"/>. Undo works and the prefab saves like any edit in prefab mode.
        /// </summary>
        internal static void Apply(TextBox box)
        {
            float minSize = MinSizeOf(box);
            GameObject target = box.Location.Show();

            if (target == null)
                return;

            TMP_Text text = target.GetComponent<TMP_Text>();

            if (text == null)
            {
                CustomLogger.LogWarning(string.Format(MissingTextFormat, nameof(TMP_Text), target.name), target);
                return;
            }

            Undo.RecordObject(text, UndoName);
            text.enableAutoSizing = true;
            text.fontSizeMin = minSize;
            text.fontSizeMax = text.fontSize;
            PrefabUtility.RecordPrefabInstancePropertyModifications(text);
            EditorSceneManager.MarkSceneDirty(target.scene);
        }
    }
}