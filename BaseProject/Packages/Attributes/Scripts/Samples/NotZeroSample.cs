using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A number that refuses to sit on zero.</summary>
    [AttributeSample(typeof(NotZeroAttribute), EAttributeCategory.Validation,
        Description = "Steps the value away from zero when it is set to zero, for a divisor or a scale where zero is "
            + "not a value but a bug.",
        Requirements = "Set the field to zero and watch it step away.",
        Variations = new[]
        {
            "NotZero() steps by one.",
            "NotZero(step) sets how far it steps. Both signs are allowed, only zero is not."
        })]
    internal sealed class NotZeroSample : ScriptableObject
    {
        [NotZero]
        [Tooltip("Set this to zero and it steps away by one.")]
        public float divisor = 1f;

        [NotZero(0.01f)]
        [Tooltip("The same with a smaller step, for a value that lives near zero.")]
        public float fine = 0.05f;
    }
}