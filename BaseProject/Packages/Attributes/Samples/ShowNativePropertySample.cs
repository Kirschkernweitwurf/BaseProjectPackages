using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A property shown as a read-only row.</summary>
    [AttributeSample(typeof(ShowNativePropertyAttribute), EAttributeCategory.Callbacks,
        Description = "Shows a property, which has no serialized value at all, as a read-only row, for the summary a "
            + "component can work out about itself.",
        Requirements = "Read-only by nature. The property is called on every repaint, so it should be cheap and free "
            + "of side effects.",
        Variations = new[]
        {
            "Nothing to configure."
        })]
    internal sealed class ShowNativePropertySample : ScriptableObject
    {
        [Tooltip("Edit this and the property below follows.")]
        public int itemCount = 3;

        /// <summary>A computed property surfaced in the inspector, read-only by nature.</summary>
        [ShowNativeProperty]
        [Tooltip("Has no serialized value, so it is shown rather than edited.")]
        internal string Summary => $"{itemCount} items";
    }
}