using Base.UtilityPackage.Menus;

namespace Base.LocalizationPackage.Editor
{
    /// <summary>
    /// Adds menu items to the Unity Editor for syncing String Table
    /// Collections with Google Sheets and opening the sync window.
    /// </summary>
    internal static class LocalizationMenu
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