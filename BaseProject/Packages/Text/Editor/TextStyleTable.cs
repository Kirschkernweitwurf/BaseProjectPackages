using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Reads the Text Styles asset table straight from the project, so the check works without play mode and in
    /// any language, not only the selected one.
    /// </summary>
    public static class TextStyleTable
    {
        private const string TableName = "Text Styles";

        /// <summary>
        /// The style behind <paramref name="key"/> in <paramref name="locale"/> or null when it is unset.
        /// </summary>
        internal static TextStyle For(Locale locale, string key)
        {
            AssetTableCollection collection = LocalizationEditorSettings.GetAssetTableCollection(TableName);

            return collection == null
                ? null
                : StyleOf(collection, TextFitLocales.SourceOf(locale), key);
        }

        /// <summary>
        /// The style <paramref name="box"/> has in <paramref name="locale"/> or null when it has none, for editor
        /// previews that show a prefab the way the game styles it.
        /// </summary>
        public static TextStyle For(Locale locale, LocalizedTextBox box)
        {
            AssetTableCollection collection = LocalizationEditorSettings.GetAssetTableCollection(TableName);

            return collection == null
                || box.StyleEntry.ReferenceType == TableEntryReference.Type.Empty
                    ? null
                    : StyleOf(collection, TextFitLocales.SourceOf(locale), box.StyleEntry);
        }

        /// <summary>
        /// The name of the Text Styles entry <paramref name="box"/> uses or null when it has none. Most boxes point at
        /// their entry by id, which a check that only read the name took for a box without a style.
        /// </summary>
        public static string KeyOf(LocalizedTextBox box)
        {
            AssetTableCollection collection = LocalizationEditorSettings.GetAssetTableCollection(TableName);

            return collection == null
                ? null
                : box.StyleEntry.ResolveKeyName(collection.SharedData);
        }

        /// <summary>
        /// The smallest size <paramref name="box"/> can shrink to in any language. Checks read this instead of the
        /// size baked into the prefab, which only changes when someone saves the prefab after a style change.
        /// </summary>
        /// <returns>False when the box has no style.</returns>
        public static bool TryGetSmallestSize(LocalizedTextBox box, out float size)
        {
            size = float.MaxValue;
            AssetTableCollection collection = LocalizationEditorSettings.GetAssetTableCollection(TableName);

            if (collection == null
                || box.StyleEntry.ReferenceType == TableEntryReference.Type.Empty)
                return false;

            foreach (Locale locale in LocalizationEditorSettings.GetLocales())
            {
                TextStyle style = StyleOf(collection, locale, box.StyleEntry);
                if (style != null)
                    size = Mathf.Min(size, style.SmallestSize);
            }

            return size < float.MaxValue;
        }

        private static TextStyle StyleOf(AssetTableCollection collection, Locale locale,
            TableEntryReference entryReference)
        {
            AssetTable table = collection.GetTable(locale.Identifier) as AssetTable;
            AssetTableEntry entry = table == null
                ? null
                : table.GetEntryFromReference(entryReference);

            return entry == null
                ? null
                : AssetDatabase.LoadAssetAtPath<TextStyle>(AssetDatabase.GUIDToAssetPath(entry.Guid));
        }
    }
}