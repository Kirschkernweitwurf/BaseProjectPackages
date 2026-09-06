namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>How much is known about whether a declared reference is needed.</summary>
    internal enum EReferenceStatus : byte
    {
        /// <summary>Nothing was found that needs the reference, so it is worth checking by hand.</summary>
        Candidate = 0,

        /// <summary>Usage could not be determined, so the reference is left alone.</summary>
        Unknown = 1,

        /// <summary>Something in the assembly needs the reference.</summary>
        Used = 2
    }
}