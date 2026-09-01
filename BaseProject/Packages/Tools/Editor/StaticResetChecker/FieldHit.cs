namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Represents a hit of a static field reference in the code.
    /// This is used to track where static fields are accessed and whether they are reset on Enter Play Mode.
    /// </summary>
    internal class FieldHit
    {
        internal int Index;
        internal string Name;
        internal string Kind;
    }
}