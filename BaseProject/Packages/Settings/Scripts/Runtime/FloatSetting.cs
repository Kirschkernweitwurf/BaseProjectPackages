using System;
using UnityEngine;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// A floating point setting that is clamped to an inclusive [min, max] range.
    /// </summary>
    public sealed class FloatSetting : Setting<float>
    {
        /// <summary>
        /// Creates a float setting.
        /// </summary>
        /// <param name="store">The backing store.</param>
        /// <param name="key">The unique persistence key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="minValue">The inclusive lower bound. Defaults to negative infinity (no bound).</param>
        /// <param name="maxValue">The inclusive upper bound. Defaults to positive infinity (no bound).</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="minValue"/> exceeds <paramref name="maxValue"/>.</exception>
        public FloatSetting(ISettingsStore store, string key, float defaultValue,
            float minValue = float.NegativeInfinity, float maxValue = float.PositiveInfinity)
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
        public float MinValue { get; }

        /// <summary>The inclusive upper bound of the value.</summary>
        public float MaxValue { get; }

        protected override float ReadFromStore(ISettingsStore store, string key, float defaultValue)
        {
            return store.GetFloat(key, defaultValue);
        }

        protected override void WriteToStore(ISettingsStore store, string key, float value)
        {
            store.SetFloat(key, value);
        }

        protected override float Coerce(float value) => Mathf.Clamp(value, MinValue, MaxValue);
    }
}