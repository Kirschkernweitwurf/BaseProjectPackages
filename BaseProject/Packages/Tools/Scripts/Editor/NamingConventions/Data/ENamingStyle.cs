namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>Casing a name can follow, once prefix, suffix and enumeration are stripped.</summary>
    public enum ENamingStyle : byte
    {
        /// <summary>No casing check at all.</summary>
        Any = 0,

        /// <summary>ExampleName</summary>
        PascalCase = 1,

        /// <summary>exampleName</summary>
        CamelCase = 2,

        /// <summary>EXAMPLE_NAME</summary>
        UpperSnakeCase = 3,

        /// <summary>example_name</summary>
        LowerSnakeCase = 4
    }
}
