using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field hidden while a condition holds.</summary>
    [AttributeSample(typeof(HideIfAttribute), EAttributeCategory.Conditions,
        Description = "Hides the field while the members it names are true. The same check as show-if, negated, for "
            + "when the readable way to say it is the negative one.",
        Requirements = "The members it names have to be bool. A field, a property or a parameterless method all work.",
        Variations = new[]
        {
            "HideIf(nameof(member)) for one member.",
            "Several members combine with EConditionMode, exactly as they do for show-if."
        })]
    internal sealed class HideIfSample : ScriptableObject
    {
        [Tooltip("Drives the field below. Toggle it and watch the field disappear.")]
        public bool verbose;

        [HideIf(nameof(verbose))]
        [Tooltip("Hidden while the toggle above is on.")]
        public string quietOnly = "Hidden while verbose";
    }
}