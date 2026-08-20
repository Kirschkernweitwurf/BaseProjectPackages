using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A small button that copies the value.</summary>
    [AttributeSample(typeof(CopyButtonAttribute), EAttributeCategory.Widgets,
        Description = "Puts a small button on the field row that copies the value to the clipboard, for an identifier "
            + "you paste somewhere else.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Nothing to configure."
        })]
    internal sealed class CopyButtonSample : ScriptableObject
    {
        [CopyButton]
        [Tooltip("Press the button to copy the value.")]
        public string copyable = "Copy me";
    }
}