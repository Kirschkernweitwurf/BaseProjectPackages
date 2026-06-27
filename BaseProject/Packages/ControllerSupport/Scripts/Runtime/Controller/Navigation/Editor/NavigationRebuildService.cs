#if UNITY_EDITOR
using UnityEngine;

namespace Base.ControllerSupport.Controller.Navigation.Editor
{
    /// <summary>
    /// Keeps every <see cref="NavigableGroup"/> wired without manual upkeep. Rebuilds all groups in the
    /// loaded scenes when entering play mode, and exposes the shared rebuild entry point used by the
    /// inspector buttons and the build preprocessor.
    /// </summary>
    public static class NavigationRebuildService
    {
        /// <summary>Rebuilds navigation on every group in the loaded scenes, including inactive ones.</summary>
        public static void RebuildAll()
        {
            NavigableGroup[] groups = Object.FindObjectsByType<NavigableGroup>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (NavigableGroup group in groups)
                group.Rebuild();
        }
    }
}
#endif