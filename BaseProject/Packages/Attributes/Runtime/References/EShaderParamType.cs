namespace Base.AttributesPackage
{
    /// <summary>Which shader property kinds a <see cref="ShaderParamAttribute"/> dropdown offers.</summary>
    public enum EShaderParamType : byte
    {
        /// <summary>Every property of the shader.</summary>
        Any = 0,

        /// <summary>Color properties.</summary>
        Color = 1,

        /// <summary>Vector properties.</summary>
        Vector = 2,

        /// <summary>Float and range properties.</summary>
        Float = 3,

        /// <summary>Texture properties.</summary>
        Texture = 4,

        /// <summary>Integer properties.</summary>
        Integer = 5
    }
}