using Base.CorePackage.Services;
using Base.SettingsPackage.Core;
using Base.ToolPackage.Identification;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Non-generic base for every setting component. Lets settings of different value types be discovered and
    /// inspected polymorphically (for example via <see cref="Object.FindObjectsByType{T}(FindObjectsSortMode)"/>).
    /// Concrete components inherit from <see cref="SettingComponent{TValue, TSetting}"/>, not this type.
    /// </summary>
    public abstract class SettingComponent : MonoBehaviour
    {
        /// <summary>The key under which this component registers its setting.</summary>
        public abstract PersistentKey Key { get; }

        /// <summary>
        /// The setting backing this component, or null until <see cref="Awake"/> has resolved the context.
        /// </summary>
        public abstract ISetting Setting { get; }

#region Unity Callbacks
        protected virtual void Awake()
        {
            if (!ServiceLocator.TryGet(out SettingsContext context) || context.Registry == null)
            {
                CustomLogger.LogWarning(
                    $"No {nameof(SettingsContext)} with a registry available; {name} will not register", this);

                return;
            }

            RegisterAndSubscribe(context);
        }

        protected virtual void OnDestroy() => Unsubscribe();
#endregion

        /// <summary>Creates the setting, registers it on the context, and subscribes the applier.</summary>
        protected abstract void RegisterAndSubscribe(SettingsContext context);

        /// <summary>Detaches the applier from the setting.</summary>
        protected abstract void Unsubscribe();
    }

    /// <summary>
    /// Typed base for setting components. Subclasses provide the key, the default value, and the apply logic;
    /// everything else (creating the typed setting, registering it, wiring the value-changed event, tearing
    /// down) is handled here.
    /// </summary>
    /// <typeparam name="TValue">The value type held by the setting.</typeparam>
    /// <typeparam name="TSetting">The concrete <see cref="Setting{T}"/> type.</typeparam>
    public abstract class SettingComponent<TValue, TSetting> : SettingComponent
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