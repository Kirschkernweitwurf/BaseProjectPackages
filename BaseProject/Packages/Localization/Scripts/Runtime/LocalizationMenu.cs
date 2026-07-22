using Base.ToolPackage.MenuManagerWindow;

namespace Base.LocalizationPackage
{
    /// <summary>
    /// Adds menu items to the Unity Editor for syncing String Table
    /// Collections with Google Sheets and opening the sync window.
    /// </summary>
    public static class LocalizationMenu
    {
        private const string Root = "Tools/Base Packages/Assets/Localization/";

        [DynamicMenuItem(Root + "Pull All String Tables")]
        private static void PullAll() => GoogleSheetsSync.SyncAll(ESyncDirection.Pull);

        [DynamicMenuItem(Root + "Push All String Tables")]
        private static void PushAll() => GoogleSheetsSync.SyncAll(ESyncDirection.Push);

        [DynamicMenuItem(Root + "Open Sync Window")]
        private static void OpenWindow() => LocalizationSyncWindow.Open();
    }
}