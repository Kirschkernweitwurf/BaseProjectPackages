namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot
{
    /// <summary>How badly a misconfigured attribute behaves at runtime.</summary>
    public enum EAttributeIssueSeverity : byte
    {
        /// <summary>The attribute cannot work at all and does nothing.</summary>
        Error = 0,

        /// <summary>The attribute works but not as intended, or only in some states.</summary>
        Warning = 1
    }
}