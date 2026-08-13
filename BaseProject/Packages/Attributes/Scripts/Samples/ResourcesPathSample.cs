using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A path stored ready for Resources.Load.</summary>
    [AttributeSample(typeof(ResourcesPathAttribute), EAttributeCategory.Pickers,
        Description = "An object picker that stores the path relative to a Resources folder, so the value "
            + "can be handed straight to Resources.Load rather than being typed and hoped for.",
        Requirements = "The asset has to live under a Resources folder. The picker cannot be limited to "
            + "those, so it lists every asset of the type and refuses the ones from elsewhere with a "
            + "warning under the field. Pick something outside Resources to see it.",
        Info = "Only the path is stored. Nothing keeps it pointing at the asset, so moving or renaming the "
            + "file breaks the reference silently, which is the price of loading by path at all.",
        Variations = new[]
        {
            "ResourcesPath() accepts any object.",
            "ResourcesPath(typeof(T)) narrows the picker to one type."
        })]
    internal sealed class ResourcesPathSample : ScriptableObject
    {
        [ResourcesPath(typeof(Texture2D))]
        [Tooltip("Stores a Resources relative path rather than a reference.")]
        public string resourcePath;
    }
}