using System.IO;
using Base.CorePackage.Screenshot;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;

namespace Base.ToolsPackage.Editor.ScreenshotFolder
{
    /// <summary>
    /// Opens the folder <see cref="ScreenshotManager"/> saves its screenshots to in the file explorer, so a fresh
    /// screenshot is one click away.
    /// </summary>
    internal static class ScreenshotFolderOpener
    {
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Open Screenshot Folder";
        private const string MissingFolderFormat = "There is no screenshot folder at {0} yet. Take a screenshot in "
            + "Play Mode first.";

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            string folder = ScreenshotManager.ScreenshotDirectory;
            if (!Directory.Exists(folder))
            {
                CustomLogger.LogWarning(string.Format(MissingFolderFormat, folder), null);
                return;
            }

            EditorUtility.OpenWithDefaultApp(folder);
        }
    }
}