namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// The kinds of events the Play Mode Saver records for the current session.
    /// </summary>
    public enum EPlayModeHistoryAction : byte
    {
        Captured = 0,
        Applied = 1,
        Discarded = 2,
        Failed = 3
    }
}
