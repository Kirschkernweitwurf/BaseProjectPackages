using System;
using Base.ToolPackage.Identification;

namespace Base.SettingsPackage.Core
{
    /// <summary>
    /// Base type for a single, persistable setting value. Holds the current value, the default,
    /// and a snapshot of the last persisted value to support reverting.
    /// </summary>
    /// <typeparam name="T">The value type held by the setting.</typeparam>
    public abstract class Setting<T> : ISetting
    {
        /// <summary>Raised whenever <see cref="Value"/> changes, including on load, revert, and reset.</summary>
        public event Action<T> OnValueChanged;

        /// <inheritdoc/>
        public PersistentKey Key { get; }

        /// <summary>The value applied when nothing has been persisted yet or after a reset.</summary>
        public T DefaultValue { get; }

        /// <summary>The current value. Assigning it raises <see cref="OnValueChanged"/>.</summary>
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        private readonly ISettingsStore _store;

        private T _value;
        private T _savedValue;

        /// <summary>Creates a setting bound to a store with the given key and default value.</summary>
        protected Setting(ISettingsStore store, PersistentKey key, T defaultValue)
        {
            _store = store;
            Key = key;
            DefaultValue = defaultValue;
            _value = defaultValue;
            _savedValue = defaultValue;
        }

        /// <inheritdoc/>
        public void Load()
        {
            _value = Read(_store, DefaultValue);
            _savedValue = _value;
            OnValueChanged?.Invoke(_value);
        }

        /// <inheritdoc/>
        public void Save()
        {
            Write(_store, _value);
            _savedValue = _value;
        }

        /// <inheritdoc/>
        public void Revert() => Value = _savedValue;

        /// <inheritdoc/>
        public void ResetToDefault() => Value = DefaultValue;

        /// <summary>Reads the persisted value from the store, or returns <paramref name="fallback"/> when absent.</summary>
        protected abstract T Read(ISettingsStore store, T fallback);

        /// <summary>Writes the value to the store without flushing it to permanent storage.</summary>
        protected abstract void Write(ISettingsStore store, T value);
    }
}