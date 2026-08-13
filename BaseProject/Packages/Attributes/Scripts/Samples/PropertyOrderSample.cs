using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A field drawn somewhere other than where it is declared.</summary>
    [AttributeSample(typeof(PropertyOrderAttribute), EAttributeCategory.Layout,
        Description = "Moves a field in the inspector without moving it in the file, so a field can be pulled to the "
            + "top for the reader without changing the order the data is serialized in.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "A lower number draws earlier. Fields without the attribute are treated as zero.",
            "Negative numbers pull a field above everything undecorated."
        })]
    internal sealed class PropertyOrderSample : ScriptableObject
    {
        [Tooltip("Declared first, drawn second, because the field below asks to go above it.")]
        public string declaredFirst = "Declared first";

        [Tooltip("Declared second, drawn third.")]
        public string declaredSecond = "Declared second";

        [PropertyOrder(-1)]
        [Tooltip("Declared last, drawn first. Only this field carries the attribute.")]
        public string pinnedToTop = "Declared last, drawn first";
    }
}