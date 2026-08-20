namespace Base.AttributePackage.Editor.Drawers.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>How badly a misconfigured attribute behaves at runtime.</summary>
    internal enum EAttributeIssueSeverity : byte
    {
        /// <summary>The attribute cannot work at all and does nothing.</summary>
        Error = 0,

        /// <summary>The attribute works but not as intended, or only in some states.</summary>
        Warning = 1
    }
}