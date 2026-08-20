using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A reference that has to be filled in.</summary>
    [AttributeSample(typeof(RequiredAttribute), EAttributeCategory.Validation,
        Description = "Marks a reference as one that has to be filled in, and shows an error under the field while it "
            + "is empty. A missing reference then shows up while the object is being set up rather than as a null at "
            + "runtime.",
        Requirements = "Clear the field to see the error appear.",
        Variations = new[]
        {
            "Required(message) writes a message of your own instead of the default one.",
            "FixAction names a method the error box offers as a button, and FixActionName labels it."
        })]
    internal sealed class RequiredSample : ScriptableObject
    {
        [Required]
        [Tooltip("Clear this and an error box appears under it.")]
        public Material material;

        [Required(FixAction = nameof(UseFallback), FixActionName = "Use fallback")]
        [Tooltip("The same check with a button in the box, since most missing references have one obvious answer.")]
        public Texture2D icon;

        private void UseFallback() => icon = Texture2D.whiteTexture;
    }
}