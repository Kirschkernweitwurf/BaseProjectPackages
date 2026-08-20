namespace Base.AttributePackage
{
    /// <summary>
    /// Icon and severity shown by an <see cref="InfoBoxAttribute"/>.
    /// </summary>
    public enum EInfoBoxType : byte
    {
        /// <summary>No icon.</summary>
        None = 0,

        /// <summary>Neutral information icon.</summary>
        Info = 1,

        /// <summary>Warning icon.</summary>
        Warning = 2,

        /// <summary>Error icon.</summary>
        Error = 3
    }
}