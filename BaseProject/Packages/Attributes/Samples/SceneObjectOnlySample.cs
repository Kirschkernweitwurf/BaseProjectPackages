using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A reference restricted to scene objects.</summary>
    [AttributeSample(typeof(SceneObjectOnlyAttribute), EAttributeCategory.Validation,
        Description = "Refuses a project asset, so a reference that has to point at something in the open scene cannot "
            + "be filled with a prefab that only looks right.",
        Requirements = "Try dragging a prefab from the project window in to see it rejected.",
        Variations = new[]
        {
            "AssetOnly is the inverse, for a reference stored on an asset."
        })]
    internal sealed class SceneObjectOnlySample : MonoBehaviour
    {
        [SceneObjectOnly]
        [Tooltip("Only an object from the open scene is accepted here.")]
        public GameObject sceneObject;
    }
}