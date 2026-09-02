namespace Base.ToolsPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Describes how a captured object reference can be resolved again once play mode ends.
    /// </summary>
    internal enum EPlayModeReferenceKind : byte
    {
        Asset = 0,
        SceneObject = 1,
        PrefabInternal = 2,
        Unresolvable = 3
    }
}