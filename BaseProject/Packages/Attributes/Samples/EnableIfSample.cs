using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field editable only while a condition holds.</summary>
    [AttributeSample(typeof(EnableIfAttribute), EAttributeCategory.Conditions,
        Description = "Leaves the field visible but only editable while the members it names are true. Greying out "
            + "rather than hiding keeps the field where the reader last saw it, which is usually the friendlier of the "
            + "two.",
        Requirements = "The members it names have to be bool. A field, a property or a parameterless method all work.",
        Variations = new[]
        {
            "EnableIf(nameof(member)) for one member.",
            "Several members combine with EConditionMode, exactly as they do for show-if.",
            "DisableIf is the same check negated."
        })]
    internal sealed class EnableIfSample : ScriptableObject
    {
        [Tooltip("Drives the field below. Toggle it and watch the field grey out.")]
        public bool verbose;

        [EnableIf(nameof(verbose))]
        [Tooltip("Editable only while the toggle above is on.")]
        public int logDepth = 3;
    }
}