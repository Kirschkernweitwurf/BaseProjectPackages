using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A value picked from options a member provides.</summary>
    [AttributeSample(typeof(DropdownAttribute), EAttributeCategory.Pickers,
        Description = "Offers the values a member returns, so the list of valid answers lives in code next to whatever "
            + "reads it rather than in a comment.",
        Requirements = "The member has to be an instance field, property or parameterless method on the same object, "
            + "returning something enumerable of the field type.",
        Variations = new[]
        {
            "The member can be a field, a property or a method.",
            "Any enumerable works, so the options can be computed rather than listed."
        })]
    internal sealed class DropdownSample : ScriptableObject
    {
        [Dropdown(nameof(Presets))]
        [Tooltip("The three options below, offered as a dropdown.")]
        public string preset = "Low";

        // The dropdown reads its options from here, so they can be computed rather than listed twice. Instance
        // rather than static, because the member resolver only looks at instance members.
        private string[] Presets => new[]
        {
            "Low",
            "Medium",
            "High"
        };
    }
}