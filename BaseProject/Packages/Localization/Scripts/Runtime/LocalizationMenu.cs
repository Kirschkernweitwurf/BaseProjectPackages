#if UNITY_EDITOR
using UnityEditor;

namespace Base.Localization
{
    /// <summary>
    /// Adds menu items to the Unity Editor for syncing String Table
    /// Collections with Google Sheets and opening the sync window.
    /// </summary>
    public static class LocalizationMenu
    {
        private const string Root = "Tools/Base Packages/Localization/";

        [MenuItem(Root + "Pull All String Tables", false, 2)]
        private static void PullAll() => GoogleSheetsSync.SyncAll(ESyncDirection.Pull);

        [MenuItem(Root + "Push All String Tables", false, 2)]
        private static void PushAll() => GoogleSheetsSync.SyncAll(ESyncDirection.Push);

        [MenuItem(Root + "Open Sync Window", false, 2)]
        private static void OpenWindow() => LocalizationSyncWindow.Open();
    }
}
#endif