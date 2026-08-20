using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A keyword picked from a material shader.</summary>
    [AttributeSample(typeof(ShaderKeywordAttribute), EAttributeCategory.Pickers,
        Description = "Lists the keywords declared by the shader of an assigned material.",
        Requirements = "Assign the material field first, and its shader has to declare keywords.",
        Variations = new[]
        {
            "The source can be a Renderer as well as a Material."
        })]
    internal sealed class ShaderKeywordSample : ScriptableObject
    {
        [Tooltip("Assign a material here first. The picker below reads from it.")]
        public Material material;

        [ShaderKeyword(nameof(material))]
        [Tooltip("Lists the keywords declared by the shader of the material above.")]
        public string shaderKeyword;
    }
}