using System.Collections.Generic;
using Base.UtilityPackage.Logging;

namespace Base.UtilityPackage.Randomization
{
    /// <summary>
    /// Draws items at a weight each, where twice the weight means twice as likely. The weights are
    /// summed up as entries are added, so a draw costs one random value and a binary search instead
    /// of a walk over the whole table.
    /// </summary>
    /// <remarks>
    /// An entry with a weight of zero or less is dropped instead of stored, which is what makes a
    /// weight of zero the way to switch a row off in the inspector without deleting it.
    /// </remarks>
    /// <typeparam name="T">The type being drawn.</typeparam>
    public sealed class WeightedTable<T>
    {
        private const string NothingToDrawMessage = "Nothing to draw. The table holds no entry with a weight "
            + "above zero, or the random source is missing.";

        private readonly List<float> _cumulativeWeights = new();
        private readonly List<T> _items = new();

        /// <summary>The number of entries that can be drawn.</summary>
        public int Count => _items.Count;

        /// <summary>The sum of every stored weight, or zero while the table is empty.</summary>
        public float TotalWeight => _cumulativeWeights.Count == 0
            ? 0f
            : _cumulativeWeights[^1];

        /// <summary>Creates an empty table.</summary>
        public WeightedTable()
        {
        }

        /// <summary>Creates a table holding the given entries.</summary>
        /// <param name="entries">The entries to add. Missing entries and zero weights are skipped.</param>
        public WeightedTable(IEnumerable<WeightedEntry<T>> entries) => AddRange(entries);

        /// <summary>
        /// Draws once straight from a list of entries, walking it in full. Meant for a one-off draw
        /// from an inspector list; build a table instead when the same list is drawn from often.
        /// </summary>
        /// <param name="entries">The entries to draw from.</param>
        /// <param name="source">The generator to draw with.</param>
        /// <param name="item">The drawn item, or the default value when nothing could be drawn.</param>
        /// <returns>True when an item was drawn.</returns>
        public static bool TryDrawFrom(IReadOnlyList<WeightedEntry<T>> entries, IRandomSource source, out T item)
        {
            item = default(T);

            if (entries == null
                || source == null)
                return false;

            float total = Sum(entries);

            if (total <= 0f)
                return false;

            float roll = source.NextFloat() * total;
            float running = 0f;

            foreach (WeightedEntry<T> entry in entries)
            {
                if (!IsDrawable(entry))
                    continue;

                running += entry.Weight;
                item = entry.Item;

                if (roll < running)
                    return true;
            }

            // Reached only when rounding puts the roll on the total itself, which belongs to the
            // last drawable entry. That entry is already in hand from the loop above.
            return true;
        }

        /// <summary>Adds an entry. An entry without weight is ignored.</summary>
        /// <param name="item">The value handed back when this entry is drawn.</param>
        /// <param name="weight">How likely this entry is compared to the others.</param>
        public void Add(T item, float weight)
        {
            if (weight <= 0f)
                return;

            _items.Add(item);
            _cumulativeWeights.Add(TotalWeight + weight);
        }

        /// <summary>Adds every entry of a list.</summary>
        /// <param name="entries">The entries to add. Missing entries and zero weights are skipped.</param>
        public void AddRange(IEnumerable<WeightedEntry<T>> entries)
        {
            if (entries == null)
                return;

            foreach (WeightedEntry<T> entry in entries)
            {
                if (!IsDrawable(entry))
                    continue;

                Add(entry.Item, entry.Weight);
            }
        }

        /// <summary>Empties the table so it can be filled again.</summary>
        public void Clear()
        {
            _items.Clear();
            _cumulativeWeights.Clear();
        }

        /// <summary>Draws one item and reports whether there was anything to draw.</summary>
        /// <param name="source">The generator to draw with.</param>
        /// <param name="item">The drawn item, or the default value when nothing could be drawn.</param>
        /// <returns>True when an item was drawn.</returns>
        public bool TryDraw(IRandomSource source, out T item)
        {
            item = default(T);

            if (source == null
                || _items.Count == 0)
                return false;

            item = _items[FindIndex(source.NextFloat() * TotalWeight)];

            return true;
        }

        /// <summary>
        /// Draws one item, logging when there is nothing to draw so a table that was never filled
        /// turns up in the console instead of quietly handing back the default value.
        /// </summary>
        /// <param name="source">The generator to draw with.</param>
        /// <returns>The drawn item, or the default value when nothing could be drawn.</returns>
        public T Draw(IRandomSource source)
        {
            if (TryDraw(source, out T item))
                return item;

            CustomLogger.LogWarning(NothingToDrawMessage, null);

            return default(T);
        }

        // An entry only counts once it has both a body and a weight, which is the same test the
        // sum, the draw and the table filling all have to agree on for the shares to come out right.
        private static bool IsDrawable(WeightedEntry<T> entry) => entry != null
            && entry.Weight > 0f;

        private static float Sum(IReadOnlyList<WeightedEntry<T>> entries)
        {
            float total = 0f;

            foreach (WeightedEntry<T> entry in entries)
            {
                if (!IsDrawable(entry))
                    continue;

                total += entry.Weight;
            }

            return total;
        }

        // The stored totals only ever rise, so the entry a roll lands in is the first one whose
        // running total is above it, which a binary search finds without touching the rest.
        private int FindIndex(float roll)
        {
            int low = 0;
            int high = _cumulativeWeights.Count - 1;

            while (low < high)
            {
                int middle = (low + high) / 2;

                if (roll < _cumulativeWeights[middle])
                    high = middle;
                else
                    low = middle + 1;
            }

            return low;
        }
    }
}