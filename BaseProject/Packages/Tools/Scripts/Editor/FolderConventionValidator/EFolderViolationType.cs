namespace Base.ToolPackage.Editor.FolderConventionValidator
{
    /// <summary>Kind of folder rule that was broken.</summary>
    public enum EFolderViolationType : byte
    {
        MissingFolder = 0,
        NamingStyle = 1,
        ForbiddenName = 2,
        ExceededDepth = 3,
        LooseAsset = 4
    }
}