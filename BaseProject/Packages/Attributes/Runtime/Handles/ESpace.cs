namespace Base.AttributesPackage
{
    /// <summary>Which space a scene handle interprets a stored vector in.</summary>
    public enum ESpace : byte
    {
        /// <summary>The value is relative to the component's transform.</summary>
        Local = 0,

        /// <summary>The value is an absolute world position.</summary>
        World = 1
    }
}