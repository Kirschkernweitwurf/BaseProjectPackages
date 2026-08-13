using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A referenced asset edited inline.</summary>
    [AttributeSample(typeof(ExpandableAttribute), EAttributeCategory.Widgets,
        Description = "Opens the referenced asset inline under the field, so a settings object can be edited without "
            + "leaving the component that points at it.",
        Requirements = "Assign a ScriptableObject asset to see it open.",
        Variations = new[]
        {
            "StartExpanded opens it on the first draw."
        })]
    internal sealed class ExpandableSample : ScriptableObject
    {
        [Expandable]
        [Tooltip("Assign any ScriptableObject and its fields appear under this one.")]
        public ScriptableObject inlineAsset;
    }
}