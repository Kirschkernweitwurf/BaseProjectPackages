namespace Base.AttributePackage
{
    /// <summary>How badly a value failed the check that looked at it.</summary>
    public enum EValidationSeverity : byte
    {
        /// <summary>The value is fine. Nothing is drawn.</summary>
        Valid = 0,

        /// <summary>The value works but is probably not what was meant.</summary>
        Warning = 1,

        /// <summary>The value is wrong and something will break.</summary>
        Error = 2
    }
}