using Base.ToolsPackage.Editor.TodoOverview.Model;
using Base.ToolsPackage.Editor.TodoOverview.Scanning;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.TodoOverview
{
    /// <summary>
    /// Opens the file an item sits in at the exact line and column of its keyword. A file the asset
    /// database does not know, which is what a package installed from Git looks like, is handed to the
    /// external editor directly.
    /// </summary>
    internal static class TodoNavigator
    {
        /// <summary>Opens the item in the configured script editor.</summary>
        /// <param name="entry">The item to jump to.</param>
        internal static void Open(TodoEntry entry)
        {
            if (entry == null)
                return;

            Object asset = AssetDatabase.LoadMainAssetAtPath(entry.AssetPath);

            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset, entry.Line, entry.Column);
                return;
            }

            string fullPath = TodoSourceReader.ResolveFullPath(entry.AssetPath);

            if (string.IsNullOrEmpty(fullPath))
                return;

            InternalEditorUtility.OpenFileAtLineExternal(fullPath, entry.Line, entry.Column);
        }

        /// <summary>Selects and pings the file the item sits in.</summary>
        /// <param name="entry">The item whose file is revealed.</param>
        internal static void Ping(TodoEntry entry)
        {
            if (entry == null)
                return;

            Object asset = AssetDatabase.LoadMainAssetAtPath(entry.AssetPath);

            if (asset == null)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}