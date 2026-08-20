using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A number with an upper bound only.</summary>
    [AttributeSample(typeof(MaxAttribute), EAttributeCategory.Validation,
        Description = "Clamps the value to a maximum, for a value with a natural ceiling and no meaningful floor.",
        Requirements = "Type a number above the bound and watch it snap back.",
        Variations = new[]
        {
            "Works on int, float and the vector types.",
            "Use MinMax when the value needs a floor as well."
        })]
    internal sealed class MaxSample : ScriptableObject
    {
        [Max(10f)]
        [Tooltip("Type 40 and it settles on 10.")]
        public float cooldown = 2f;
    }
}