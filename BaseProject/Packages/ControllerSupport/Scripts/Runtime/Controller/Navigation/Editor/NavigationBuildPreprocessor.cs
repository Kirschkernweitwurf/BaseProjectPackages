#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Base.ControllerSupport.Controller.Navigation.Editor
{
    /// <summary>
    /// Rebuilds all navigable groups in the loaded scenes when a player build starts, so the baked
    /// navigation that ships is always current.
    /// </summary>
    public sealed class NavigationBuildPreprocessor : IPreprocessBuildWithReport
    {
        /// <inheritdoc/>
        public int callbackOrder => 0;

        /// <inheritdoc/>
        public void OnPreprocessBuild(BuildReport report) => NavigationRebuildService.RebuildAll();
    }
}
#endif