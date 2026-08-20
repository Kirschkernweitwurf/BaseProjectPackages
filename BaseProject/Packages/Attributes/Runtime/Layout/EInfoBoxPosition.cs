namespace Base.AttributePackage
{
    /// <summary>Where an <see cref="InfoBoxAttribute"/> draws relative to the field.</summary>
    public enum EInfoBoxPosition : byte
    {
        /// <summary>Above the field, like a header.</summary>
        Above = 0,

        /// <summary>Below the field, like a validation message.</summary>
        Below = 1
    }
}