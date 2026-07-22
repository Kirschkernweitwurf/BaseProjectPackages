using System.IO;
using Base.AttributePackage;
using Base.UtilityPackage;
using TMPro;
using UnityEngine;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// Fills a given Text with information about the version and when the last build was made
    /// from a version txt in the StreamingAssets folder.
    /// </summary>
    public class BuildVersion : MonoBehaviour
    {
        private const int VersionInfoLineCount = 3;

        private static readonly string PathVersionFile = Application.streamingAssetsPath + "/version.txt";

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
        public static void UpdateVersionInfo()
        {
            // Read Current Version Info (To Increase Build Number)
            string[] versionInfo = ReadVersionInfo();
            int buildNumber = int.TryParse(versionInfo[2], out buildNumber)
                ? buildNumber + 1
                : 1;

            // Update Current Version Info
            versionInfo[1] = Application.version;
            versionInfo[2] = buildNumber.ToString();

            // Assuming the path to streaming assets folder might be missing on some platforms
            string path = Path.GetDirectoryName(PathVersionFile);
            if (path != null && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.WriteAllLines(PathVersionFile, versionInfo);
        }
#endif

        /// <summary>
        /// Reads the version file and always returns an array with <see cref="VersionInfoLineCount"/> entries,
        /// even if the file is missing or has fewer lines.
        /// </summary>
        private static string[] ReadVersionInfo()
        {
            string[] versionInfo = new string[VersionInfoLineCount];

            if (!File.Exists(PathVersionFile))
                return versionInfo;

            string[] lines = File.ReadAllLines(PathVersionFile);
            for (int i = 0; i < versionInfo.Length && i < lines.Length; i++)
                versionInfo[i] = lines[i];

            return versionInfo;
        }

        private void DisplayVersionInfo()
        {
            string[] versionInfo = ReadVersionInfo();

            versionText.text = string.IsNullOrEmpty(versionInfo[1]) && string.IsNullOrEmpty(versionInfo[2])
                ? string.Empty
                : $"{versionInfo[1]} [{versionInfo[2]}]";
        }
    }
}