using Base.SettingsPackage.Core;
using Base.UtilityPackage.Identification;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Typed base for setting components. Subclasses provide the key, the default value, and the apply logic;
    /// everything else (creating the typed setting, registering it, wiring the value-changed event, tearing
    /// down) is handled here.
    /// </summary>
    /// <typeparam name="TValue">The value type held by the setting.</typeparam>
    /// <typeparam name="TSetting">The concrete <see cref="Setting{T}"/> type.</typeparam>
    public abstract class TypedSettingComponent<TValue, TSetting> : SettingComponent
        where TSetting : Setting<TValue>
    {
        /// <summary>
        /// The typed setting once registered. Null before <see cref="SettingComponent.Awake"/> completes.
        /// </summary>
        public TSetting TypedSetting { get; private set; }

        /// <inheritdoc/>
        public sealed override ISetting Setting => TypedSetting;

        /// <summary>The value applied when nothing has been persisted yet or after a reset.</summary>
        protected abstract TValue DefaultValue { get; }

        /// <inheritdoc/>
        protected sealed override void RegisterAndSubscribe(SettingsContext context)
        {
            TypedSetting = CreateSetting(context.Store, Key, DefaultValue);
            context.Registry.Register(TypedSetting);
            TypedSetting.OnValueChanged += Apply;
        }

        /// <inheritdoc/>
        protected sealed override void Unsubscribe()
        {
            if (TypedSetting != null)
                TypedSetting.OnValueChanged -= Apply;
        }

        /// <summary>Creates the typed setting bound to the given store.</summary>
        protected abstract TSetting CreateSetting(ISettingsStore store, PersistentKey key, TValue defaultValue);

        /// <summary>Applies a value to whatever the setting controls.</summary>
        protected abstract void Apply(TValue value);
    }
}