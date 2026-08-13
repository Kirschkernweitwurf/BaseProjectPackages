using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A field renamed in the inspector only.</summary>
    [AttributeSample(typeof(LabelAttribute), EAttributeCategory.Layout,
        Description = "Replaces the label of the field without renaming the field, for the case where "
            + "the good code name and the good inspector name are not the same one.",
        Requirements = "Nothing.",
        Info = "A label longer than the label column widens it for that row rather than being cut, so a "
            + "sentence as a label still reads.",
        Variations = new[]
        {
            "Label(text) for a fixed name.",
            "A text starting with a dollar names a member to read, so the label can say what the value "
                + "currently means."
        })]
    internal sealed class LabelSample : ScriptableObject
    {
        [Label("Renamed here only")]
        [Tooltip("The field is still called internalName in code.")]
        public string internalName = "Only the label changed";

        [Label("$" + nameof(CountLabel))]
        [Tooltip("The label is read from a property, so it changes with the value.")]
        public int itemCount = 1;

        // A string argument starting with a dollar names a member to read. Written as "$" + nameof(X) so
        // a rename moves the reference with it.
        private string CountLabel => itemCount == 1
            ? "One item"
            : $"{itemCount} items";
    }
}