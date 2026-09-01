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
        /// <summary>
        /// Where this hook sits among the build callbacks. Nothing here depends on another hook
        /// having run, so it takes the default slot.
        /// </summary>
        public int callbackOrder => 0;

        /// <summary>Refreshes the version file so the build picks up the current values.</summary>
        /// <param name="report">The build being prepared. Not read; the version is project wide.</param>
        public void OnPreprocessBuild(BuildReport report) => BuildVersion.UpdateVersionInfo();
    }
}