using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field checked by a method of your own.</summary>
    [AttributeSample(typeof(ValidateInputAttribute), EAttributeCategory.Validation,
        Description = "Runs a method of your own and reports what it returns, for the checks no general attribute "
            + "could know about.",
        Requirements = "The method has to be on the same object and take no parameters. Returning a bool is enough, "
            + "but returning a validation result lets one method name which check failed and pick between an error and "
            + "a warning.",
        Variations = new[]
        {
            "ValidateInput(nameof(Method)) for a method returning bool.",
            "ValidateInput(nameof(Method), message) writes the message for a bool method.",
            "A method returning ValidationResult carries its own message and severity."
        })]
    internal sealed class ValidateInputSample : ScriptableObject
    {
        [ValidateInput(nameof(ValidateSize))]
        [Tooltip("Set this to zero for an error, or to an odd number for a warning.")]
        public int gridSize = 8;

        // Returning a result rather than a bool lets one validator name which check failed and choose between
        // an error and a warning.
        private ValidationResult ValidateSize()
        {
            if (gridSize <= 0)
                return ValidationResult.Error("The grid needs at least one cell.");

            return gridSize % 2 == 0
                ? ValidationResult.Valid
                : ValidationResult.Warning("An odd grid has no center cell.");
        }
    }
}