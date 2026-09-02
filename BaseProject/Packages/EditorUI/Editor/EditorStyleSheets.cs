using UnityEditor;
using UnityEngine.UIElements;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// Loads the USS a UI Toolkit window is styled with and attaches it, addressed by asset GUID.
    /// </summary>
    /// <remarks>
    /// By GUID rather than by name, because a name search answers with whatever file in the project
    /// happens to be called that, and the window is then styled by a stranger's sheet. A GUID also
    /// survives the sheet being renamed or moved, and it survives the package being embedded, which a
    /// hard coded package path does not.
    /// <para>
    /// The GUID belongs to the window as a constant next to the rest of its own constants. It is
    /// readable at the top of the sheet's <c>.meta</c> file.
    /// </para>
    /// </remarks>
    public static class EditorStyleSheets
    {
        /// <summary>
        /// Loads a style sheet by its asset GUID.
        /// </summary>
        /// <param name="sheetGuid">The GUID from the sheet's meta file.</param>
        /// <returns>The sheet, or null when nothing is filed under that GUID.</returns>
        public static StyleSheet Load(string sheetGuid)
        {
            if (string.IsNullOrEmpty(sheetGuid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(sheetGuid);

            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
        }

        /// <summary>
        /// Attaches a style sheet to an element, unless it is already on it.
        /// </summary>
        /// <remarks>
        /// The return value is worth acting on. A window whose sheet went missing still opens, but it
        /// opens unstyled, and an unstyled UI Toolkit window looks broken rather than plain.
        /// </remarks>
        /// <param name="root">The element to style, usually a window's root visual element.</param>
        /// <param name="sheetGuid">The GUID from the sheet's meta file.</param>
        /// <returns>False when the sheet could not be found, so the caller can report it.</returns>
        public static bool Apply(VisualElement root, string sheetGuid)
        {
            if (root == null)
                return false;

            StyleSheet sheet = Load(sheetGuid);

            if (sheet == null)
                return false;

            // Adding the same sheet twice stacks it twice. A window that rebuilds its root, which is
            // what happens on a skin change, would otherwise collect a copy on every rebuild.
            if (root.styleSheets.Contains(sheet))
                return true;

            root.styleSheets.Add(sheet);

            return true;
        }
    }
}