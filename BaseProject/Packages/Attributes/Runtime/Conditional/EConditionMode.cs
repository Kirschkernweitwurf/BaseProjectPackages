namespace Base.AttributesPackage
{
    /// <summary>How multiple condition members are combined into a single result.</summary>
    public enum EConditionMode : byte
    {
        /// <summary>Every member must be true.</summary>
        All = 0,

        /// <summary>At least one member must be true.</summary>
        Any = 1
    }
}