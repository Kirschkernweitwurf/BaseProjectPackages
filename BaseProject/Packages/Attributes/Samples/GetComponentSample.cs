using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A reference filled from the same GameObject.</summary>
    [RequireComponent(typeof(BoxCollider))]
    [AttributeSample(typeof(GetComponentAttribute), EAttributeCategory.References,
        Description = "Fills itself with a component of the field type from the same GameObject while the field is "
            + "empty. The reference that would otherwise be dragged onto itself every time a prefab is made.",
        Requirements = "A component of that type has to be on the same GameObject. This sample requires one, so the "
            + "field fills the moment the component exists.",
        Variations = new[]
        {
            "Nothing to configure. It only fills while the field is empty, so an explicit assignment is never "
            + "overwritten.",
            "Use RequiredGet when a missing one should also be reported as an error."
        })]
    internal sealed class GetComponentSample : MonoBehaviour
    {
        [GetComponent]
        [Tooltip("Filled from the collider this sample requires on its own GameObject.")]
        public BoxCollider ownCollider;
    }
}