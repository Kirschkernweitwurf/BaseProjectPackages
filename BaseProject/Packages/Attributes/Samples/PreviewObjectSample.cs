using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A large preview of the assigned object.</summary>
    [AttributeSample(typeof(PreviewObjectAttribute), EAttributeCategory.Widgets,
        Description = "Draws the assigned object as a large preview under the field, for a reference picked by eye "
            + "rather than by name.",
        Requirements = "Assign a texture, sprite, material or mesh to see the preview.",
        Variations = new[]
        {
            "PreviewObject() uses the default height.",
            "PreviewObject(height) sets it."
        })]
    internal sealed class PreviewObjectSample : ScriptableObject
    {
        [PreviewObject(96f)]
        [Tooltip("Assign a texture to see it drawn under the field.")]
        public Texture2D preview;
    }
}