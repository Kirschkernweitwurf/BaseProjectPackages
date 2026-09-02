using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field greyed out while a condition holds.</summary>
    [AttributeSample(typeof(DisableIfAttribute), EAttributeCategory.Conditions,
        Description = "Greys the field out while the members it names are true. The inverse of enable-if.",
        Requirements = "The members it names have to be bool. A field, a property or a parameterless method all work. "
            + "Pointing it at a number or a reference is the single most common mistake with this family, and the "
            + "troubleshoot tab reports it.",
        Variations = new[]
        {
            "DisableIf(nameof(member)) for one member.",
            "Several members combine with EConditionMode, exactly as they do for show-if."
        })]
    internal sealed class DisableIfSample : ScriptableObject
    {
        [Tooltip("Drives the field below. Toggle it and watch the field lock.")]
        public bool locked = true;

        [DisableIf(nameof(locked))]
        [Tooltip("Greyed out while the toggle above is on.")]
        public string editable = "Locked by a sibling";
    }
}