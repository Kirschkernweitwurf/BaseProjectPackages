#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Utility.Logging;

namespace Utility.Identification.Editor
{
    /// <summary>
    /// Build hook that guarantees all UniqueIds are valid before we produce a build.
    /// </summary>
    internal class UniqueIdPreBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            UniqueIdProjectValidator.RebuildAndFixAll();
            AssetDatabase.SaveAssets();

            CustomLogger.Log("All UniqueIds validated before build.", null);
        }
    }
}
#endif