namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// How much attention a finding deserves. A scan of a real project produces thousands of true but
    /// uninteresting statements, so ranking is what makes the handful of real ones findable.
    /// </summary>
    public enum ESeverity : byte
    {
        High = 0,
        Medium = 1,
        Low = 2
    }
}
