using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>An integer snapped to a power of two.</summary>
    [AttributeSample(typeof(PowerOfTwoAttribute), EAttributeCategory.Validation,
        Description = "Snaps the value to the nearest power of two, for the sizes the hardware wants that way.",
        Requirements = "Type 200 and watch it settle on 256.",
        Variations = new[]
        {
            "Nothing to configure. The minimum is one, so zero and negatives snap up."
        })]
    internal sealed class PowerOfTwoSample : ScriptableObject
    {
        [PowerOfTwo]
        [Tooltip("Type 200 and it settles on 256.")]
        public int textureSize = 256;
    }
}