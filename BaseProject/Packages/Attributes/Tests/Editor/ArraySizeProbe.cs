using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// One field per shape the bounds reader has to tell apart, including the two that carry the
    /// attribute but are not arrays at all.
    /// </summary>
    internal sealed class ArraySizeProbe : ScriptableObject
    {
        /// <summary>Serialized name of the array locked to an exact count.</summary>
        internal const string FixedField = nameof(fixedArray);

        /// <summary>Serialized name of the array whose bounds are equal.</summary>
        internal const string EqualBoundsField = nameof(equalBoundsArray);

        /// <summary>Serialized name of the array with a floor and no ceiling.</summary>
        internal const string MinimumOnlyField = nameof(minimumOnlyArray);

        /// <summary>Serialized name of the field that carries the attribute but is not a collection.</summary>
        internal const string NumberField = nameof(number);

        /// <summary>Serialized name of the array carrying no bounds at all.</summary>
        internal const string PlainField = nameof(plainArray);

        /// <summary>Serialized name of the array bounded at both ends by different numbers.</summary>
        internal const string RangedField = nameof(rangedArray);

        /// <summary>Serialized name of the string that carries the attribute.</summary>
        internal const string TextField = nameof(text);

        private const int EqualBound = 3;
        private const int FixedCount = 4;
        private const int Highest = 5;
        private const int Lowest = 2;

        [SerializeField] [ArraySize(FixedCount)] private int[] fixedArray;
        [SerializeField] [ArraySize(Min = Lowest, Max = Highest)] private int[] rangedArray;
        [SerializeField] [ArraySize(Min = EqualBound, Max = EqualBound)] private int[] equalBoundsArray;
        [SerializeField] [ArraySize(Min = Lowest)] private int[] minimumOnlyArray;
        [SerializeField] private int[] plainArray;
        [SerializeField] [ArraySize(FixedCount)] private string text;
        [SerializeField] [ArraySize(FixedCount)] private int number;
    }
}