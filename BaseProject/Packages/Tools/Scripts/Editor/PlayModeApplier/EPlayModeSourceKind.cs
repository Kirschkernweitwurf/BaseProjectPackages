namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Describes how a marked object can be found again once play mode ends.
    /// </summary>
    public enum EPlayModeSourceKind : byte
    {
        SceneObject = 0,
        RuntimeInstance = 1
    }
}