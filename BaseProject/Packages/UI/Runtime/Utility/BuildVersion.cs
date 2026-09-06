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
        private const string VersionFileName = "version.txt";

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
                versionText.text = BuildVersionFile.Format(BuildVersionFile.Read(VersionFilePath));
        }
#endregion

#if UNITY_EDITOR
        /// <summary>
        /// Writes the current application version into the version file and increments the build number.
        /// Runs from the build pipeline before a build starts.
        /// </summary>
        public static void UpdateVersionInfo()
        {
            string[] versionInfo = BuildVersionFile.Read(VersionFilePath);

            BuildVersionFile.Write(VersionFilePath, BuildVersionFile.Advance(versionInfo, Application.version));
        }
#endif
    }
}