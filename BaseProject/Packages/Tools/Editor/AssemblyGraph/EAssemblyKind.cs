namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>Category of an assembly, used for filtering and cleanup permission.</summary>
    public enum EAssemblyKind : byte
    {
        /// <summary>An assembly defined inside the project's Assets folder.</summary>
        Project = 0,

        /// <summary>An assembly defined inside an imported package.</summary>
        Package = 1,

        /// <summary>An assembly shipped by Unity itself.</summary>
        UnityPackage = 2,

        /// <summary>A precompiled assembly with no editable source.</summary>
        Library = 3
    }
}