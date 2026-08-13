using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Headings, boxes, lines and spacing.</summary>
    [AttributeSample("Layout")]
    internal sealed class LayoutSample : ScriptableObject
    {
        [Title("Movement", EColor.Cyan)]
        [Tooltip("Puts a heading above this field. Use it to split a long component into parts, so the "
            + "reader can find a section again instead of scanning every field.")]
        public float speed = 5f;

        [InfoBox("A note to whoever edits this component next.")]
        [Tooltip("Puts a box of text above this field, for saying something the field name cannot. "
            + "Comes in info, warning and error, and can take a color of its own.")]
        public float acceleration = 12f;

        [HorizontalLine(EColor.Red)]
        [Tooltip("Draws a line above this field. Use it to separate two groups of fields when neither "
            + "of them needs a heading. The color, thickness and spacing are all yours to set.")]
        public int damage = 10;

        [Indent]
        [Tooltip("Pushes this field one step to the right, which reads as belonging to the field above "
            + "it. A negative number pulls it back to the left instead.")]
        public float critMultiplier = 2f;

        [Suffix(SuffixAttribute.MetersPerSecondSquared)]
        [Tooltip("Adds a small label after the value, almost always a unit. The units come from a "
            + "shared list, so the same one is spelled the same way on every field that uses it.")]
        public float gravity = 9.81f;
    }
}