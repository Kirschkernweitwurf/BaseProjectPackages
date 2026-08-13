using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A string trimmed to a maximum length.</summary>
    [AttributeSample(typeof(MaxLengthAttribute), EAttributeCategory.Validation,
        Description = "Trims the string to a maximum character count after editing, for an identifier that has to fit "
            + "somewhere with a fixed width.",
        Requirements = "Type past the limit to see the text cut.",
        Info = "The count is characters rather than bytes, so anything outside the basic Latin range "
            + "still counts as one.",
        Variations = new[]
        {
            "MaxLength(n) for the limit."
        })]
    internal sealed class MaxLengthSample : ScriptableObject
    {
        [MaxLength(12)]
        [Tooltip("Type more than twelve characters and the rest is cut.")]
        public string shortText = "Twelve chars";
    }
}