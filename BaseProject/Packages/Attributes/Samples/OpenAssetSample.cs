using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A button that opens the referenced asset.</summary>
    [AttributeSample(typeof(OpenAssetAttribute), EAttributeCategory.Widgets,
        Description = "Adds a button that opens the referenced asset in whatever editor owns it, for a reference you "
            + "follow more often than you change.",
        Requirements = "Assign an asset to enable the button.",
        Variations = new[]
        {
            "OpenAsset() uses the default label.",
            "OpenAsset(label) sets it."
        })]
    internal sealed class OpenAssetSample : ScriptableObject
    {
        [OpenAsset("Edit")]
        [Tooltip("Assign a text asset and press the button.")]
        public TextAsset openable;
    }
}