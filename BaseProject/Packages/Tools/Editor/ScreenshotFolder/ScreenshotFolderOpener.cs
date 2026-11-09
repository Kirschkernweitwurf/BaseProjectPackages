using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.ScreenshotFolder
{
    /// <summary>
    /// Opens the folder the operating system saves screenshots to in the file explorer, so a fresh screenshot is one
    /// click away.
    /// </summary>
    internal static class ScreenshotFolderOpener
    {
        private const string HomePrefix = "~/";
        private const string MacLocationArguments = "read com.apple.screencapture location";
        private const string MacLocationCommand = "defaults";
        private const string MacLocationFailedFormat = "Can't read the screenshot location, using the Desktop: {0}";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Open Screenshot Folder";
        private const string MissingFolderFormat = "There is no screenshot folder at {0} yet. Take a screenshot "
            + "first, or check where your system saves them.";
        private const string ScreenshotsFolderName = "Screenshots";

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            string folder = FindScreenshotFolder();
            if (!Directory.Exists(folder))
            {
                CustomLogger.LogWarning(string.Format(MissingFolderFormat, folder), null);
                return;
            }

            EditorUtility.OpenWithDefaultApp(folder);
        }

        // Windows saves to Pictures\Screenshots. On Linux the Pictures folder comes from the XDG user dirs, and GNOME
        // and KDE both save into its Screenshots folder.
        private static string FindScreenshotFolder() => Application.platform == RuntimePlatform.OSXEditor
            ? FindMacScreenshotFolder()
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), ScreenshotsFolderName);

        // macOS saves to the Desktop unless another place was picked in the Screenshot app.
        private static string FindMacScreenshotFolder()
        {
            string location = ReadMacScreenshotLocation();
            if (location.Length == 0)
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            return location.StartsWith(HomePrefix, StringComparison.Ordinal)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    location.Substring(HomePrefix.Length))
                : location;
        }

        // Empty while the location was never changed, because then the setting does not exist.
        private static string ReadMacScreenshotLocation()
        {
            ProcessStartInfo info = new(MacLocationCommand, MacLocationArguments)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using Process process = Process.Start(info);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return process.ExitCode == 0
                    ? output.Trim()
                    : string.Empty;
            }
            catch (Win32Exception exception)
            {
                CustomLogger.LogWarning(string.Format(MacLocationFailedFormat, exception.Message), null);

                return string.Empty;
            }
        }
    }
}