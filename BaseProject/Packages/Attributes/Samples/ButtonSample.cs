using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A method turned into a button.</summary>
    [AttributeSample(typeof(ButtonAttribute), EAttributeCategory.Widgets,
        Description = "Turns a method into a button drawn under the fields, which saves writing a small editor script "
            + "for every one-off action.",
        Requirements = "The method has to be on the same object. Parameters are allowed and are drawn above the "
            + "button.",
        Variations = new[]
        {
            "Button(label) sets the label, and leaving it out uses the method name.",
            "Row puts several buttons on one line.",
            "Foldout collects buttons under a named collapsible group.",
            "Size picks between the normal and the large button.",
            "Confirm asks before running, and Mode limits the button to play mode or edit mode."
        })]
    internal sealed class ButtonSample : ScriptableObject
    {
        [Tooltip("The buttons below write into this field so you can see them run.")]
        public string log = "Nothing yet";

        /// <summary>A plain button, drawn under the fields.</summary>
        [Button("Reset")]
        internal void ResetLog() => log = nameof(ResetLog);

        /// <summary>Two buttons sharing a row, since they are opposites.</summary>
        [Button("Apply", Row = "applyRevert")]
        internal void Apply() => log = nameof(Apply);

        /// <summary>The other half of that row.</summary>
        [Button("Revert", Row = "applyRevert")]
        internal void Revert() => log = nameof(Revert);

        /// <summary>A button that takes arguments, so a one-off call needs no serialized fields.</summary>
        [Button("Spawn", Size = EButtonSize.Large)]
        internal void Spawn(int count = 3, float radius = 5f) => log = $"{nameof(Spawn)} {count} within {radius}";
    }
}