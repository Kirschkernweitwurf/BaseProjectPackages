using System.IO;
using System.Linq;
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
        private static readonly string PathVersionFile = Application.streamingAssetsPath + "/version.txt";

        [SerializeField] private bool hideOnRelease;
        [SerializeField] private TMP_Text versionText;

#region Unity Callbacks
        private void Start()
        {
            if (hideOnRelease && Platform.IsRelease)
                versionText?.gameObject.SetActive(false);
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

        private static string[] ReadVersionInfo()
        {
            string[] versionInfo = new string[3];

            if (File.Exists(PathVersionFile))
                versionInfo = File.ReadAllLines(PathVersionFile);

            return versionInfo;
        }

        private void DisplayVersionInfo()
        {
            string[] versionInfo = ReadVersionInfo();

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (versionInfo.All(string.IsNullOrEmpty))
                versionText.text = string.Empty;
            else
                versionText.text = $"{versionInfo.ElementAtOrDefault(1)} [{versionInfo.ElementAtOrDefault(2)}]";
        }
    }
}