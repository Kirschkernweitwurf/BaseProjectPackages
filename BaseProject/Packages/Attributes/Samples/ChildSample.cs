using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A reference filled from a child.</summary>
    [AttributeSample(typeof(ChildAttribute), EAttributeCategory.References,
        Description = "Fills itself with a component of the field type from the children while the field is empty. For "
            + "the muzzle, the visual root or whatever else a component owns one level down.",
        Requirements = "A child has to carry that component. This sample builds one when it is created.",
        Variations = new[]
        {
            "A name argument narrows the search to the child with that name.",
            "IncludeInactive decides whether inactive children count."
        })]
    internal sealed class ChildSample : MonoBehaviour, ISampleSetup
    {
        [Child("Muzzle")]
        [Tooltip("Filled from the named child this sample creates under itself.")]
        public BoxCollider muzzle;

        /// <summary>Builds the child the getter above searches.</summary>
        public void BuildSample()
        {
            GameObject child = new("Muzzle");

            child.transform.SetParent(transform);
            child.AddComponent<BoxCollider>();
        }
    }
}