using UnityEditor;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Finds the stylesheet. Loaded from the path it is known to live at rather than searched for by name.
    /// Because a project-wide search for an asset called CodebaseGraph will happily return
    /// somebody else's file, and the window would then be styled by whatever it happened to find.
    /// </summary>
    internal static class CodebaseGraphStyle
    {
        private const string FolderPath = "Packages/com.baseprojectpackages.tools/Editor/CodebaseGraph";
        private const string SheetFilter = "CodebaseGraph t:StyleSheet";
        private const string SheetName = "CodebaseGraph.uss";

        /// <summary>Attaches the stylesheet to an element.</summary>
        /// <param name="root">Element to style.</param>
        public static void Apply(VisualElement root)
        {
            StyleSheet sheet = Load();

            if (sheet != null)
                root.styleSheets.Add(sheet);
        }

        private static StyleSheet Load()
        {
            StyleSheet known = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{FolderPath}/{SheetName}");
            if (known != null)
                return known;

            // An embedded or renamed package moves the folder, so the fallback searches, but only
            // inside this tool's own folder rather than across the whole project.
            foreach (string guid in AssetDatabase.FindAssets(SheetFilter, new[]
                     {
                         FolderPath
                     }))
            {
                StyleSheet found = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guid));

                if (found != null)
                    return found;
            }

            return null;
        }
    }
}