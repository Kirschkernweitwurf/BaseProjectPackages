using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>Consecutive fields collected under one collapsible group.</summary>
    [AttributeSample(typeof(FoldoutAttribute), EAttributeCategory.Layout,
        Description = "Puts consecutive fields that share a name into one collapsible group. The run ends where the "
            + "name changes, so there is no closing marker to forget.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Every field of one group repeats the same name.",
            "A field between two groups with no attribute ends the first group."
        })]
    internal sealed class FoldoutSample : ScriptableObject
    {
        [Foldout("Bounds")]
        [Tooltip("First field of the group. The group is named after the string, not after this field.")]
        public float width = 1f;

        [Foldout("Bounds")]
        [Tooltip("Same name, so it joins the group above rather than opening a new one.")]
        public float height = 2f;

        [Foldout("Timing")]
        [Tooltip("A different name, so the previous group ends here and a new one opens.")]
        public float delay = 0.5f;

        [Tooltip("No attribute at all, so it sits outside every group.")]
        public string note = "Outside the groups";
    }
}