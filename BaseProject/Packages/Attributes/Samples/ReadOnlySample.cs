using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A field shown but never editable.</summary>
    [AttributeSample(typeof(ReadOnlyAttribute), EAttributeCategory.Conditions,
        Description = "Shows the field and never lets it be edited, for a value the component works out for itself and "
            + "only reports.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Nothing to configure. For a value that is only read-only some of the time, use disable-if instead."
        })]
    internal sealed class ReadOnlySample : ScriptableObject
    {
        [ReadOnly]
        [Tooltip("Visible, greyed out, and not editable at any point.")]
        public string computed = "Never editable";
    }
}