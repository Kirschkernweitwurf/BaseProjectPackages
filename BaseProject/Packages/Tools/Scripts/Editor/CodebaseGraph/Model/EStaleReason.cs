namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// Why a dismissal stopped matching. The two want different responses, so they are never shown
    /// under one label: a missing entity is dead configuration, while a finding that stopped firing
    /// means either you fixed it or a rule quietly stopped detecting something it used to catch.
    /// </summary>
    internal enum EStaleReason : byte
    {
        None = 0,
        Missing = 1,
        Resolved = 2
    }
}