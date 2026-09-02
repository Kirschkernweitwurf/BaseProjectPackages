using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A reference restricted to project assets.</summary>
    [AttributeSample(typeof(AssetOnlyAttribute), EAttributeCategory.Validation,
        Description = "Refuses anything that lives in a scene, so a reference stored on an asset cannot be pointed at "
            + "an object that will not exist the next time the asset is loaded.",
        Requirements = "Try dragging a scene object in to see it rejected.",
        Variations = new[]
        {
            "SceneObjectOnly is the inverse, for a reference that has to come from the open scene."
        })]
    internal sealed class AssetOnlySample : ScriptableObject
    {
        [AssetOnly]
        [Tooltip("Only a project asset is accepted here.")]
        public GameObject assetOnly;
    }
}