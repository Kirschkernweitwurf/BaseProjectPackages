using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Fields that complain when they are wrong.</summary>
    [AttributeSample("Validation")]
    internal sealed class ValidationSample : ScriptableObject
    {
        [Required]
        [Tooltip("Marks the reference as one that has to be filled in. Clear it and an error box appears "
            + "under the field, so a missing reference shows up before the game runs.")]
        public Material material;

        [Required(FixAction = nameof(UseFallback), FixActionName = "Use fallback")]
        [Tooltip("The same check, with a button in the error box that fills the field for you. Most "
            + "missing references have one obvious answer, so the box can offer it.")]
        public Texture2D icon;

        [NotNullOrEmpty("A profile needs a name.")]
        [Tooltip("Requires a non-empty string, with a message written for this field.")]
        public string profileName = "Default";

        [MinMax(0, 100)] public int health = 50;

        [Max(10f)] public float cooldown = 2f;

        [PowerOfTwo] public int textureSize = 256;

        [Unique] public List<string> layers = new();

        [ValidateInput(nameof(ValidateSize))] public int gridSize = 8;

        private void UseFallback() => icon = Texture2D.whiteTexture;

        // Returning a result rather than a bool lets one validator name which check failed and choose
        // between an error and a warning.
        private ValidationResult ValidateSize()
        {
            if (gridSize <= 0)
                return ValidationResult.Error("The grid needs at least one cell.");

            return gridSize % 2 == 0
                ? ValidationResult.Valid
                : ValidationResult.Warning("An odd grid has no centre cell.");
        }
    }
}