using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A heading above a field.</summary>
    [AttributeSample(typeof(TitleAttribute), EAttributeCategory.Layout,
        Description = "Puts a heading above the field, so a long component reads as a few named parts instead of one "
            + "flat list. Optionally collapsible, which folds every field under it until the next heading.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Title(text) for a plain heading.",
            "Title(text, EColor.Cyan) for a color from the shared palette.",
            "Title(text, hexColor) for anything the palette does not cover.",
            "Foldout = true makes the section collapsible, and DefaultExpanded decides how it starts.",
            "A text starting with a dollar names a member to read, so the heading can be computed."
        })]
    internal sealed class TitleSample : ScriptableObject
    {
        [Title("Movement", EColor.Cyan)]
        [Tooltip("Sits under a plain heading.")]
        public float speed = 5f;

        [Tooltip("Still under the heading above, since the run only ends at the next one.")]
        public float acceleration = 12f;

        [Title("Combat", "#E86A6A", Foldout = true, DefaultExpanded = true)]
        [Tooltip("Under a collapsible heading. Click the heading to fold both fields away.")]
        public int damage = 10;

        [Tooltip("Folds away together with the field above it.")]
        public float critMultiplier = 2f;
    }
}