using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field visible only while a condition holds.</summary>
    [AttributeSample(typeof(ShowIfAttribute), EAttributeCategory.Conditions,
        Description = "Shows the field only while the members it names are true, so a mode nobody is using takes up no "
            + "room at all.",
        Requirements = "The members it names have to be bool. A field, a property or a parameterless method all work.",
        Variations = new[]
        {
            "ShowIf(nameof(member)) for one member.",
            "ShowIf(nameof(a), nameof(b)) requires both, which is what several members mean by default.",
            "ShowIf(EConditionMode.Any, nameof(a), nameof(b)) requires either one.",
            "HideIf is the same check negated, for when the readable version is the negative one."
        })]
    internal sealed class ShowIfSample : ScriptableObject
    {
        [Tooltip("Drives the fields below. Toggle it and watch them appear.")]
        public bool useOverride;

        [Tooltip("The second toggle, so the combined conditions have two things to work with.")]
        public bool verbose;

        [ShowIf(nameof(useOverride))]
        [Tooltip("Visible while the first toggle is on.")]
        public float overrideValue = 1f;

        [ShowIf(EConditionMode.Any, nameof(useOverride), nameof(verbose))]
        [Tooltip("Visible while either toggle is on.")]
        public string shownByEither = "Either one is enough";

        [ShowIf(nameof(useOverride), nameof(verbose))]
        [Tooltip("Visible only while both are on.")]
        public string shownByBoth = "Both are needed";
    }
}