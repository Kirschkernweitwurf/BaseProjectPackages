using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A thumbnail under an asset reference.</summary>
    [AttributeSample(typeof(ShowAssetPreviewAttribute), EAttributeCategory.Widgets,
        Description = "Draws a small thumbnail of the assigned asset under the field. The lighter version of the "
            + "preview attribute, for when a reminder of what is assigned is enough.",
        Requirements = "Assign an asset with a thumbnail to see it.",
        Variations = new[]
        {
            "A size argument sets the thumbnail size."
        })]
    internal sealed class ShowAssetPreviewSample : ScriptableObject
    {
        [ShowAssetPreview]
        [Tooltip("Assign a sprite to see its thumbnail.")]
        public Sprite preview;
    }
}