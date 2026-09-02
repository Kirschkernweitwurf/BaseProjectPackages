using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A reference required only under a condition.</summary>
    [AttributeSample(typeof(RequiredIfAttribute), EAttributeCategory.Validation,
        Description = "Requires the field only while the members it names are true, for a reference that matters in "
            + "one mode and is meaningless in the other.",
        Requirements = "The members it names have to be bool. Toggle the one below to see the error appear and "
            + "disappear.",
        Variations = new[]
        {
            "RequiredIf(nameof(member)) for one member.",
            "Several members combine with EConditionMode, exactly as they do for show-if."
        })]
    internal sealed class RequiredIfSample : ScriptableObject
    {
        [Tooltip("Turn this on and the field below becomes required.")]
        public bool usesMaterial;

        [RequiredIf(nameof(usesMaterial))]
        [Tooltip("Only required while the toggle above is on.")]
        public Material conditionalMaterial;
    }
}