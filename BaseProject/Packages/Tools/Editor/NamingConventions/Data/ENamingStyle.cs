namespace Base.ToolsPackage.Editor.NamingConventions.Data
{
    /// <summary>Casing a name can follow, once prefix, suffix and number are stripped.</summary>
    internal enum ENamingStyle : byte
    {
        /// <summary>No casing check at all.</summary>
        Any = 0,

        /// <summary>MyName</summary>
        PascalCase = 1,

        /// <summary>myName</summary>
        CamelCase = 2,

        /// <summary>MY_NAME</summary>
        UpperSnakeCase = 3,

        /// <summary>my_name</summary>
        LowerSnakeCase = 4,

        /// <summary>My_Name, so a category stays separate from the asset itself.</summary>
        PascalSnakeCase = 5
    }
}