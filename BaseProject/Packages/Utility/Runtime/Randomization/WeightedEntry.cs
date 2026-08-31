using System;
using UnityEngine;

namespace Base.UtilityPackage.Randomization
{
    /// <summary>
    /// One item together with the weight it is drawn at. Serializable, so a weighted list can be
    /// authored in the inspector as a list of these and handed straight to a
    /// <see cref="WeightedTable{T}"/> or to <see cref="WeightedTable{T}.TryDrawFrom"/>.
    /// </summary>
    /// <typeparam name="T">The type being drawn.</typeparam>
    [Serializable]
    public sealed class WeightedEntry<T>
    {
        private const float DefaultWeight = 1f;

        [field: Tooltip("The value handed back when this entry is drawn.")]
        [field: SerializeField] public T Item { get; private set; }

        [field: Tooltip("How likely this entry is compared to the others. Twice the weight means twice as"
            + " likely. Zero takes the entry out of the draw without deleting the row.")]
        [field: Min(0f)]
        [field: SerializeField] public float Weight { get; private set; } = DefaultWeight;

        /// <summary>
        /// Creates an empty entry. Declared explicitly because Unity's serializer builds instances
        /// through the parameterless constructor, which the one below would otherwise remove.
        /// </summary>
        public WeightedEntry()
        {
        }

        /// <summary>Creates an entry for one item.</summary>
        /// <param name="item">The value handed back when this entry is drawn.</param>
        /// <param name="weight">How likely this entry is compared to the others.</param>
        public WeightedEntry(T item, float weight)
        {
            Item = item;
            Weight = weight;
        }
    }
}