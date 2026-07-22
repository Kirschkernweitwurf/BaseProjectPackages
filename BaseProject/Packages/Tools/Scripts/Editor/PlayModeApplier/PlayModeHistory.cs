using System;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Records what the Play Mode Saver did since the last play session.
    /// Lives in the store rather than in memory, because the interesting entries are written while play mode
    /// is exiting and would otherwise be lost to the domain reload before anyone could read them.
    /// </summary>
    public static class PlayModeHistory
    {
        private const string TimeFormat = "HH:mm:ss";

        /// <summary>Records one action. The history is wiped when the next play session starts.</summary>
        public static void Record(EPlayModeHistoryAction action, string displayName, string detail)
        {
            PlayModeStateStore store = PlayModeStateStore.instance;
            store.AddHistoryEntry(new PlayModeHistoryEntry
            {
                timestamp = DateTime.Now.ToString(TimeFormat),
                action = action,
                displayName = displayName,
                detail = detail
            });

            store.Persist();
        }
    }
}