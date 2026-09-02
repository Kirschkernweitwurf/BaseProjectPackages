using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A small button that empties the field.</summary>
    [AttributeSample(typeof(ClearButtonAttribute), EAttributeCategory.Widgets,
        Description = "Puts a small button on the field row that resets it to none or empty, for a reference you clear "
            + "more often than you reassign.",
        Requirements = "Works on object references and strings.",
        Variations = new[]
        {
            "Nothing to configure."
        })]
    internal sealed class ClearButtonSample : ScriptableObject
    {
        [ClearButton]
        [Tooltip("Press the button to empty the field.")]
        public string clearable = "Clear me";

        [ClearButton]
        [Tooltip("The same on a reference, where it resets to none.")]
        public Material clearableReference;
    }
}