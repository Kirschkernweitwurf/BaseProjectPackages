using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A button beside the field.</summary>
    [AttributeSample(typeof(InlineButtonAttribute), EAttributeCategory.Widgets,
        Description = "Puts a button on the field own row that calls a method, for the small action that belongs to "
            + "that one value.",
        Requirements = "The method has to be on the same object and take no parameters.",
        Variations = new[]
        {
            "InlineButton(nameof(Method)) falls back to the method name, nicified.",
            "InlineButton(nameof(Method), label) sets the label."
        })]
    internal sealed class InlineButtonSample : ScriptableObject
    {
        [InlineButton(nameof(Reroll), "Roll")]
        [Tooltip("Press the button beside the field.")]
        public int damage = 6;

        private void Reroll() => damage = Random.Range(1, 7);
    }
}