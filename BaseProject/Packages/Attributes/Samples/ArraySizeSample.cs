using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A collection with a locked element count.</summary>
    [AttributeSample(typeof(ArraySizeAttribute), EAttributeCategory.Validation,
        Description = "Locks the element count of a collection, so the add and remove buttons disappear and the size "
            + "cannot drift, for a list that mirrors something fixed like the sides of a die.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "ArraySize(n) locks the collection to exactly n elements.",
            "Min and Max allow a range instead of one exact count."
        })]
    internal sealed class ArraySizeSample : ScriptableObject
    {
        [ArraySize(3)]
        [Tooltip("Always three elements. The add and remove buttons are gone.")]
        public List<string> exactlyThree = new();

        [ArraySize(Min = 1, Max = 4)]
        [Tooltip("Between one and four elements, so the buttons stay but the bounds hold.")]
        public List<string> oneToFour = new();
    }
}