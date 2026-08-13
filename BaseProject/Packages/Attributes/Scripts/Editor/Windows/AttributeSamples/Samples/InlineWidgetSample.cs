using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Buttons and actions that sit on the field's own row.</summary>
    [AttributeSample("Widgets")]
    internal sealed class InlineWidgetSample : ScriptableObject
    {

        [InlineButton(nameof(Reroll), "Roll")]
        [Tooltip("A button beside the field that calls a method. With no label it falls back to the "
            + "method name, nicified.")]
        public int damage = 6;

        [ClearButton]
        [Tooltip("A small button that empties the field, for a reference you clear more often than you "
            + "reassign.")]
        public string clearable = "Clear me";

        [CopyButton]
        [Tooltip("Copies the value to the clipboard, for an identifier you paste elsewhere.")]
        public string copyable = "Copy me";

        [CurveRange(0f, 0f, 1f, 1f, EColor.Magenta)]
        [Tooltip("Locks an animation curve to a box and tints it, so a normalized curve cannot wander "
            + "outside the range that reads it.")]
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [ColorPalette(nameof(Brand))]
        [Tooltip("Restricts a color to a set of swatches, so a tint cannot drift off the palette.")]
        public Color brandColor = Color.white;

        [MaxLength(12)]
        [Tooltip("Trims the string to a maximum length after editing. Type past the limit to see it cut.")]
        public string shortText = "Twelve chars";

        [NotZero]
        [Tooltip("Refuses to sit on zero, stepping away when you try, for a divisor or a scale.")]
        public float notZero = 1f;

        // The palette reads from here. Instance rather than static, because the member resolver only
        // looks at instance members and a static source would silently find nothing.
        private Color[] Brand => new[]
        {
            new Color(0.20f, 0.60f, 0.86f),
            new Color(0.18f, 0.80f, 0.44f),
            new Color(0.95f, 0.61f, 0.07f)
        };

        private void Reroll() => damage = Random.Range(1, 7);
    }
}