using System;
using System.Collections.Generic;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Strongly-typed, persisted setting. Wraps a single value held in an <see cref="ISettingsStore"/>,
    /// caches it after the first read, and notifies listeners whenever the value changes.
    /// Concrete types only need to describe how their value is read, written and (optionally) coerced.
    /// </summary>
    /// <typeparam name="T">The value type of the setting.</typeparam>
    public abstract class Setting<T> : ISetting
    {
        /// <summary>Raised after the value changes. The argument is the new, coerced value.</summary>
        public event Action<T> OnValueChanged;

        private readonly ISettingsStore _store;
        private readonly T _defaultValue;

        private T _cachedValue;
        private bool _isLoaded;

        /// <summary>
        /// Creates a new setting bound to the given store and key.
        /// </summary>
        /// <param name="store">The backing store used to read and write the value.</param>
        /// <param name="key">The unique persistence key for this setting.</param>
        /// <param name="defaultValue">The value used when the store holds no entry for this key.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
        protected Setting(ISettingsStore store, string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Setting key must not be null or empty.", nameof(key));

            _store = store ?? throw new ArgumentNullException(nameof(store));
            Key = key;
            _defaultValue = defaultValue;
        }

        /// <summary>The unique persistence key for this setting.</summary>
        public string Key { get; }

        /// <summary>The value used when the store holds no entry for this setting.</summary>
        public T DefaultValue => Coerce(_defaultValue);

        /// <summary>
        /// The current value. Reading loads it from the store on first access. Writing coerces the
        /// value, persists it and raises <see cref="OnValueChanged"/> only when it actually changes.
        /// </summary>
        public T Value
        {
            get
            {
                EnsureLoaded();
                return _cachedValue;
            }
            set => SetValue(value);
        }

        /// <summary>
        /// Sets the value. Persists and raises <see cref="OnValueChanged"/> only when the coerced value
        /// differs from the current value.
        /// </summary>
        /// <param name="newValue">The requested value.</param>
        public void SetValue(T newValue)
        {
            EnsureLoaded();

            T coercedValue = Coerce(newValue);

            if (EqualityComparer<T>.Default.Equals(coercedValue, _cachedValue))
                return;

            _cachedValue = coercedValue;
            WriteToStore(_store, Key, coercedValue);
            RaiseValueChanged();
        }

        /// <summary>Resets the value back to <see cref="DefaultValue"/>.</summary>
        public void ResetToDefault() => SetValue(_defaultValue);

        /// <summary>
        /// Discards the cached value and reloads it from the store. Raises <see cref="OnValueChanged"/>
        /// if the reloaded value differs from the previously cached one.
        /// </summary>
        public void Reload()
        {
            bool wasLoaded = _isLoaded;
            T previousValue = _cachedValue;

            _isLoaded = false;
            EnsureLoaded();

            if (!wasLoaded)
                return;

            if (EqualityComparer<T>.Default.Equals(previousValue, _cachedValue))
                return;

            RaiseValueChanged();
        }

        /// <summary>
        /// Subscribes a listener to value changes.
        /// </summary>
        /// <param name="listener">The callback invoked with the new value.</param>
        /// <param name="invokeImmediately">
        /// When true, the listener is invoked once with the current value so it can sync straight away.
        /// </param>
        public void Subscribe(Action<T> listener, bool invokeImmediately = true)
        {
            if (listener == null)
                return;

            OnValueChanged += listener;

            if (!invokeImmediately)
                return;

            listener.Invoke(Value);
        }

        /// <summary>Unsubscribes a previously registered listener.</summary>
        /// <param name="listener">The callback to remove.</param>
        public void Unsubscribe(Action<T> listener)
        {
            if (listener == null)
                return;

            OnValueChanged -= listener;
        }

        /// <summary>Reads the raw value from the store.</summary>
        protected abstract T ReadFromStore(ISettingsStore store, string key, T defaultValue);

        /// <summary>Writes the value to the store.</summary>
        protected abstract void WriteToStore(ISettingsStore store, string key, T value);

        /// <summary>
        /// Coerces a value into the valid range for this setting (for example clamping a number).
        /// The default implementation returns the value unchanged.
        /// </summary>
        protected virtual T Coerce(T value) => value;

        private void EnsureLoaded()
        {
            if (_isLoaded)
                return;

            _cachedValue = Coerce(ReadFromStore(_store, Key, _defaultValue));
            _isLoaded = true;
        }

        private void RaiseValueChanged() => OnValueChanged?.Invoke(_cachedValue);
    }
}