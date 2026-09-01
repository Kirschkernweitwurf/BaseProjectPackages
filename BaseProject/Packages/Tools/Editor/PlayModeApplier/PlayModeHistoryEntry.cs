using System;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// One line in the session history.
    /// </summary>
    [Serializable]
    internal class PlayModeHistoryEntry
    {
        /// <summary>When the action happened, already formatted for display.</summary>
        public string timestamp;

        /// <summary>What was done: captured, applied, or discarded.</summary>
        public EPlayModeHistoryAction action;

        /// <summary>Name of the object the action concerned.</summary>
        public string displayName;

        /// <summary>Extra context, such as why an apply was skipped.</summary>
        public string detail;
    }
}