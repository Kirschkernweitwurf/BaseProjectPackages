using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A reference that fills itself and complains when it cannot.</summary>
    [RequireComponent(typeof(AudioSource))]
    [AttributeSample(typeof(RequiredGetAttribute), EAttributeCategory.References,
        Description = "Fills itself the way the getters do, and reports an error when nothing was found. The two "
            + "halves belong together often enough that doing both with one attribute is worth it.",
        Requirements = "A component of that type has to be reachable. This sample requires one on its own GameObject, "
            + "so the field fills; remove it and the error appears.",
        Variations = new[]
        {
            "InParents and InChildren widen the search beyond the own GameObject.",
            "IncludeSelf and IncludeInactive narrow it.",
            "Message writes the error text instead of the default one."
        })]
    internal sealed class RequiredGetSample : MonoBehaviour
    {
        [RequiredGet]
        [Tooltip("Filled from the audio source this sample requires, and an error if it were missing.")]
        public AudioSource ownSource;
    }
}