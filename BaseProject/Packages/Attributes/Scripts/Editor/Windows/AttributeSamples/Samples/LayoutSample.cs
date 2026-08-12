using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Titles, boxes, separators and spacing.</summary>
    [AttributeSample("Layout")]
    internal sealed class LayoutSample : ScriptableObject
    {
        [Title("Movement", EColor.Cyan)]
        [InfoBox("A title is a heading above the field that carries it. An info box explains one.")]
        [Tooltip("An ordinary field under a title, to show what a heading separates.")]
        public float speed = 5f;

        [Suffix(SuffixAttribute.MetersPerSecondSquared)]
        [Tooltip("A unit written from the shared vocabulary rather than as a literal.")]
        public float acceleration = 12f;

        [Title("Combat", EColor.Red)]
        [HorizontalLine(EColor.Red)]
        [Tooltip("Sits under a colored separator line.")]
        public int damage = 10;

        [Indent] public float critMultiplier = 2f;

        [Indent(-1)]
        [InfoBox("A negative indent pulls back out again.", EInfoBoxType.None)]
        [Tooltip("Pulled back out to the first column by a negative indent.")]
        public bool pierces;
    }
}