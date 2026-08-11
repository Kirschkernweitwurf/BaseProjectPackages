using System;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// One line in the session history.
    /// </summary>
    [Serializable]
    internal class PlayModeHistoryEntry
    {
        public string timestamp;
        public EPlayModeHistoryAction action;
        public string displayName;
        public string detail;
    }
}