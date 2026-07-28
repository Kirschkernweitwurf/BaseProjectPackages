namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>Whether a declared reference appears to be used by the compiled assembly.</summary>
    public enum EReferenceStatus : byte
    {
        /// <summary>The reference is resolved and used.</summary>
        Used = 0,

        /// <summary>The reference is declared but nothing in the assembly needs it.</summary>
        Unused = 1,

        /// <summary>Usage could not be determined, so the reference is left alone.</summary>
        Unknown = 2
    }
}