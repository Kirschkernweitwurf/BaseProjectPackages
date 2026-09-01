namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Represents a hit of a static field reference in the code.
    /// This is used to track where static fields are accessed and whether they are reset on Enter Play Mode.
    /// </summary>
    internal sealed class FieldHit
    {
        /// <summary>Character offset of the declaration, turned into a line number later.</summary>
        internal int Index;

        /// <summary>Name of the field, property or event.</summary>
        internal string Name;

        /// <summary>What kind of member it is.</summary>
        internal string Kind;
    }
}