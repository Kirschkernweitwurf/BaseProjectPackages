using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A reference filled from anywhere in the scene.</summary>
    [AttributeSample(typeof(GetInSceneAttribute), EAttributeCategory.References,
        Description = "Fills itself with the first component of the field type found anywhere in the open scene. The "
            + "widest of the getters, and the one to reach for last: first found is arbitrary the moment a second one "
            + "exists.",
        Requirements = "Something in the open scene has to carry that component. This sample searches your scene, so "
            + "what it finds depends on what is open.",
        Variations = new[]
        {
            "GetInScene(false) skips inactive objects.",
            "Prefer GetComponent or Child when the thing being looked for is nearby, since both are predictable."
        })]
    internal sealed class GetInSceneSample : MonoBehaviour
    {
        [GetInScene]
        [Tooltip("Filled from the first camera in the open scene, whichever that turns out to be.")]
        public Camera sceneCamera;
    }
}