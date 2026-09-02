using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A reference filled with a prefab carrying a component.</summary>
    [AttributeSample(typeof(GetPrefabWithComponentAttribute), EAttributeCategory.References,
        Description = "Fills itself with the first prefab in the project that carries the given component, and assigns "
            + "the prefab root rather than the component. The weakest of the auto-getters: first found is arbitrary "
            + "the moment a second matching prefab exists.",
        Requirements = "Exactly one matching prefab should exist, or the result is whichever the project happens to "
            + "return first. Prefer assigning by hand when there are several.",
        Variations = new[]
        {
            "Left without a type, the field type is used.",
            "A type argument names the component the prefab has to carry."
        })]
    internal sealed class GetPrefabWithComponentSample : ScriptableObject
    {
        [GetPrefabWithComponent(typeof(Collider))]
        [Tooltip("Fills itself with the first prefab in the project carrying a collider.")]
        public GameObject colliderPrefab;
    }
}