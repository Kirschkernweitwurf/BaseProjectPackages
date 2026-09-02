namespace Base.ToolsPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Describes how a marked object can be found again once play mode ends.
    /// </summary>
    internal enum EPlayModeSourceKind : byte
    {
        SceneObject = 0,
        RuntimeInstance = 1
    }
}