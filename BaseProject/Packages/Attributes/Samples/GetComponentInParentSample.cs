using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A reference filled from an ancestor.</summary>
    [AttributeSample(typeof(GetComponentInParentAttribute), EAttributeCategory.References,
        Description = "Fills itself with a component of the field type from an ancestor while the field is empty, "
            + "skipping the GameObject it is on. For the reference a child holds to whatever owns it.",
        Requirements = "An ancestor has to carry that component. This sample builds a parent with one when it is "
            + "created.",
        Variations = new[]
        {
            "A name argument narrows the search to an ancestor with that name.",
            "IncludeInactive decides whether inactive ancestors count."
        })]
    internal sealed class GetComponentInParentSample : MonoBehaviour, ISampleSetup
    {
        [GetComponentInParent]
        [Tooltip("Filled from the parent this sample creates around itself.")]
        public Rigidbody ownerBody;

        /// <summary>Builds the parent the getter above searches.</summary>
        public void BuildSample()
        {
            GameObject parent = new("Sample Parent");

            parent.AddComponent<Rigidbody>();
            transform.SetParent(parent.transform);
        }
    }
}