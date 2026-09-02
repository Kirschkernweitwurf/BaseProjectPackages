namespace Base.AttributesPackage
{
    /// <summary>Which axis a flat scene handle such as a disc is oriented around.</summary>
    public enum ENormalAxis : byte
    {
        /// <summary>Facing along the X axis of the transform.</summary>
        X = 0,

        /// <summary>Facing along the Y axis of the transform, which is the ground plane.</summary>
        Y = 1,

        /// <summary>Facing along the Z axis of the transform.</summary>
        Z = 2
    }
}