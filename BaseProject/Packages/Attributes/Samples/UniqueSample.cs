using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A collection whose entries have to differ.</summary>
    [AttributeSample(typeof(UniqueAttribute), EAttributeCategory.Validation,
        Description = "Requires every entry of a collection to differ, and names the first pair that does not. Empty "
            + "entries are ignored, so a half-filled list is not reported as broken while it is being written.",
        Requirements = "Add two rows with the same text to see the error.",
        Variations = new[]
        {
            "Unique(message) writes a message of your own instead of the default one."
        })]
    internal sealed class UniqueSample : ScriptableObject
    {
        [Unique]
        [Tooltip("Add two rows with the same text and the error names them.")]
        public List<string> layers = new();
    }
}