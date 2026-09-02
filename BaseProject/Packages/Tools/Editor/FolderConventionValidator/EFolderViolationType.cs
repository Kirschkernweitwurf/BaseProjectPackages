namespace Base.ToolsPackage.Editor.FolderConventionValidator
{
    /// <summary>Kind of folder rule that was broken.</summary>
    internal enum EFolderViolationType : byte
    {
        MissingFolder = 0,
        NamingStyle = 1,
        ForbiddenName = 2,
        ExceededDepth = 3,
        LooseAsset = 4
    }
}