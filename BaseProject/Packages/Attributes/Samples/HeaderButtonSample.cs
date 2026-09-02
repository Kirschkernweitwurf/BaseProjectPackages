using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A button in the component header.</summary>
    [AttributeSample(typeof(HeaderButtonAttribute), EAttributeCategory.Widgets,
        Description = "Puts a button in the component title bar, for the action that belongs to the component as a "
            + "whole rather than to any one field.",
        Requirements = "Drawn by the real Inspector, which is what owns the component title bar. Use the button below "
            + "to put this sample into your scene, then look at it in the Inspector.",
        Variations = new[]
        {
            "HeaderButton(label) sets the label, and leaving it out uses the method name.",
            "Width sets how much room it takes, and Confirm asks before running.",
            "Mode limits the button to play mode or edit mode."
        })]
    internal sealed class HeaderButtonSample : MonoBehaviour
    {
        [Tooltip("The header button writes into this field so you can see it run.")]
        public string log = "Nothing yet";

        /// <summary>Runs from the component title bar rather than from the body.</summary>
        [HeaderButton("Reset")]
        internal void ResetLog() => log = nameof(ResetLog);
    }
}