using Base.UIPackage.Utility;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Base.UIPackage.Editor
{
    /// <summary>
    /// Writes the date-version and build-number into a version.txt in the Streaming Assets folder.
    /// Called before a build is started.
    /// </summary>
    internal sealed class BuildVersionProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) => BuildVersion.UpdateVersionInfo();
    }
}