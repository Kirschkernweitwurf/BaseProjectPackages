using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A string picked from the project tags.</summary>
    [AttributeSample(typeof(TagAttribute), EAttributeCategory.Pickers,
        Description = "Shows a dropdown of the tags the project has. The value stays a string, but it is picked from a "
            + "list instead of typed, so it cannot be spelled wrong.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Tag(true) hides the option to type a tag that does not exist yet."
        })]
    internal sealed class TagSample : ScriptableObject
    {
        [Tag]
        [Tooltip("Picked from the project tags rather than typed.")]
        public string tag = "Untagged";
    }
}