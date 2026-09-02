using System.IO;
using Base.AttributesPackage;
using Base.UtilityPackage;
using TMPro;
using UnityEngine;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// Fills a given Text with information about the version and when the last build was made
    /// from a version txt in the StreamingAssets folder.
    /// </summary>
    public sealed class BuildVersion : MonoBehaviour
    {
        private const int BuildNumberLineIndex = 2;
        private const int FirstBuildNumber = 1;
        private const string VersionFileName = "version.txt";
        private const int VersionInfoLineCount = 3;
        private const int VersionLineIndex = 1;

        private static readonly string VersionFilePath =
            Path.Combine(Application.streamingAssetsPath, VersionFileName);

        [SerializeField] private bool hideOnRelease;
        [Required] [SerializeField] private TMP_Text versionText;

#region Unity Callbacks
        private void Start()
        {
            if (hideOnRelease && Platform.IsRelease)
                versionText.gameObject.SetActive(false);
            else
                DisplayVersionInfo();
        }
#endregion

#if UNITY_EDITOR
        /// <summary>
        /// Writes the current application version into the version file and increments the build number.
        /// Runs from the build pipeline before a build starts.
        /// </summary>
        public static void UpdateVersionInfo()
        {
            string[] versionInfo = ReadVersionInfo();

            // Read the stored build number first so it can be increased
            int buildNumber = int.TryParse(versionInfo[BuildNumberLineIndex], out int storedBuildNumber)
                ? storedBuildNumber + 1
                : FirstBuildNumber;

            versionInfo[VersionLineIndex] = Application.version;
            versionInfo[BuildNumberLineIndex] = buildNumber.ToString();

            // The StreamingAssets folder is missing on some platforms
            string directory = Path.GetDirectoryName(VersionFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllLines(VersionFilePath, versionInfo);
        }
#endif

        /// <summary>
        /// Reads the version file and always returns an array with <see cref="VersionInfoLineCount"/> entries,
        /// even if the file is missing or has fewer lines.
        /// </summary>
        private static string[] ReadVersionInfo()
        {
            string[] versionInfo = new string[VersionInfoLineCount];

            if (!File.Exists(VersionFilePath))
                return versionInfo;

            string[] lines = File.ReadAllLines(VersionFilePath);
            for (int i = 0; i < versionInfo.Length && i < lines.Length; i++)
                versionInfo[i] = lines[i];

            return versionInfo;
        }

        private void DisplayVersionInfo()
        {
            string[] versionInfo = ReadVersionInfo();

            versionText.text = string.IsNullOrEmpty(versionInfo[VersionLineIndex])
                && string.IsNullOrEmpty(versionInfo[BuildNumberLineIndex])
                    ? string.Empty
                    : $"{versionInfo[VersionLineIndex]} [{versionInfo[BuildNumberLineIndex]}]";
        }
    }
}