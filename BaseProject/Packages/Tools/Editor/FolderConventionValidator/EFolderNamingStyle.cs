namespace Base.ToolsPackage.Editor.FolderConventionValidator
{
    /// <summary>Naming style every folder name below the scan root has to follow.</summary>
    internal enum EFolderNamingStyle : byte
    {
        Any = 0,
        PascalCase = 1,
        CamelCase = 2,
        SnakeCase = 3,
        KebabCase = 4
    }
}