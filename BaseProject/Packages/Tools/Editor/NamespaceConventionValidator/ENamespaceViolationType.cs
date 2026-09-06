namespace Base.ToolsPackage.Editor.NamespaceConventionValidator
{
    /// <summary>Kind of namespace rule that was broken.</summary>
    internal enum ENamespaceViolationType : byte
    {
        MissingNamespace = 0,
        MismatchedNamespace = 1
    }
}