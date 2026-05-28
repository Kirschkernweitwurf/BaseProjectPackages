using System;
using UnityEngine;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// An integer setting that is clamped to an inclusive [min, max] range.
    /// </summary>
    public sealed class IntSetting : Setting<int>
    {
        /// <summary>
        /// Creates an int setting.
        /// </summary>
        /// <param name="store">The backing store.</param>
        /// <param name="key">The unique persistence key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="minValue">The inclusive lower bound. Defaults to <see cref="int.MinValue"/>.</param>
        /// <param name="maxValue">The inclusive upper bound. Defaults to <see cref="int.MaxValue"/>.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="minValue"/>
        /// exceeds <paramref name="maxValue"/>.</exception>
        public IntSetting(ISettingsStore store, string key, int defaultValue,
            int minValue = int.MinValue, int maxValue = int.MaxValue)
            : base(store, key, defaultValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException(
                    $"minValue ({minValue}) must not be greater than maxValue ({maxValue}).", nameof(minValue));
            }

            MinValue = minValue;
            MaxValue = maxValue;
        }

        /// <summary>The inclusive lower bound of the value.</summary>
        public int MinValue { get; }

        /// <summary>The inclusive upper bound of the value.</summary>
        public int MaxValue { get; }

        protected override int ReadFromStore(ISettingsStore store, string key, int defaultValue)
        {
            return store.GetInt(key, defaultValue);
        }

        protected override void WriteToStore(ISettingsStore store, string key, int value) => store.SetInt(key, value);

        protected override int Coerce(int value) => Mathf.Clamp(value, MinValue, MaxValue);
    }
}