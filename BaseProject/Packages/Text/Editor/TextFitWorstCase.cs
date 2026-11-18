using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Puts the text pipeline into the hardest case a player can pick, the widest readable font with wider spacing at
    /// the largest text size, so the text fit check sees what those players see. Undone after the check, so nothing
    /// else in the editor changes.
    /// </summary>
    public static class TextFitWorstCase
    {
        private const string MissingSetText = "No Readable Font Set asset found, so the check runs with the designed "
            + "fonts.";

        /// <summary>
        /// Switches to the widest font with wider spacing at the largest text size; false when no font set exists.
        /// </summary>
        public static bool TryBegin()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(ReadableFontSet));
            if (guids.Length == 0)
            {
                CustomLogger.LogWarning(MissingSetText, null);
                return false;
            }

            ReadableFontSet fontSet = AssetDatabase.LoadAssetAtPath<ReadableFontSet>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
            ReadableText.SetFont(fontSet.WidestFont);
            ReadableText.SetSpacing(fontSet.WideSpacing);
            ReadableText.SetScale(ReadableText.LargestScale);
            return true;
        }

        /// <summary>Back to the designed fonts, spacing and size.</summary>
        public static void End() => ReadableText.Clear();
    }
}