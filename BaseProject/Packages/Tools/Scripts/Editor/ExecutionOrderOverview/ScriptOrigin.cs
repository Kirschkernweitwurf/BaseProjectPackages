namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>Where a script's source lives.</summary>
    public enum ScriptOrigin : byte
    {
        /// <summary>Inside the project's Assets folder.</summary>
        Project = 0,

        /// <summary>Inside an imported package.</summary>
        Package = 1,

        /// <summary>Built into Unity, with no editable source file.</summary>
        BuiltIn = 2
    }
}