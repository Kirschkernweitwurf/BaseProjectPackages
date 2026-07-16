namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Defines where captured play mode values are written once play mode ends.
    /// </summary>
    public enum EPlayModeApplyTarget : byte
    {
        SceneInstance = 0,
        PrefabOverride = 1,
        PrefabAsset = 2
    }
}
