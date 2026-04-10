#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace UI.Utility
{
    /// <summary>
    /// Writes the date-version and build-number into a version.txt in the Streaming Assets folder.
    /// Called before a build is started.
    /// </summary>
    public class BuildVersionProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            PlayerPrefs.DeleteAll();
            BuildVersion.UpdateVersionInfo();
        }
    }
}
#endif