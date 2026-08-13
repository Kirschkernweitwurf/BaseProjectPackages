using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A shader property picked from a material.</summary>
    [AttributeSample(typeof(ShaderParamAttribute), EAttributeCategory.Pickers,
        Description = "Lists the shader properties of an assigned material, optionally narrowed to one type, so a tint "
            + "field cannot end up pointing at a texture property.",
        Requirements = "Assign the material field first.",
        Variations = new[]
        {
            "A second argument narrows the list to colors, floats, textures or vectors.",
            "The source can be a Renderer as well as a Material."
        })]
    internal sealed class ShaderParamSample : ScriptableObject
    {
        [Tooltip("Assign a material here first. The pickers below read from it.")]
        public Material material;

        [ShaderParam(nameof(material), EShaderParamType.Color)]
        [Tooltip("Only the color properties of the material above.")]
        public string shaderColor;

        [ShaderParam(nameof(material))]
        [Tooltip("Every property, since no type was given.")]
        public string anyProperty;
    }
}