using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>An asset chosen from the Resources folders.</summary>
    [AttributeSample(typeof(ResourcesAssetAttribute), EAttributeCategory.Pickers,
        Description = "Picks an asset that lives under a Resources folder. The field stores the load path "
            + "because that is the only handle Resources.Load takes, but what is being chosen is the "
            + "asset, not a path somebody typed.",
        Requirements = "The asset has to live under a Resources folder. The picker cannot be limited to "
            + "those, so it lists every asset of the type and refuses the ones from elsewhere with a "
            + "warning under the field. Pick something outside Resources to see it.",
        Info = "Only the path is stored. Nothing keeps it pointing at the asset, so moving or renaming the "
            + "file breaks the reference silently, which is the price of loading by path at all.",
        Variations = new[]
        {
            "ResourcesAsset() accepts any object.",
            "ResourcesAsset(typeof(T)) narrows the picker to one type."
        })]
    internal sealed class ResourcesAssetSample : ScriptableObject
    {
        [ResourcesAsset(typeof(Texture2D))]
        [Tooltip("Pick a texture from a Resources folder. Anything else is refused below.")]
        public string resourcePath;
    }
}