using Base.SettingsPackage.Core;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// Typed base for settings UI elements. Resolves the setting from the registry, keeps the subscription
    /// alive for as long as the element lives and tears it down again. Subclasses react through
    /// <see cref="OnBound"/> and <see cref="OnSettingChanged"/>.
    /// </summary>
    /// <typeparam name="TValue">The value type held by the setting.</typeparam>
    /// <typeparam name="TSetting">The concrete <see cref="Setting{T}"/> type.</typeparam>
    public abstract class TypedSettingElement<TValue, TSetting> : SettingElement
        where TSetting : Setting<TValue>
    {
        /// <summary>The bound setting, or null until <see cref="OnBound"/> has run.</summary>
        protected TSetting Setting { get; private set; }

#region Unity Callbacks
        protected virtual void OnDestroy()
        {
            if (Setting != null)
                Setting.OnValueChanged -= OnSettingChanged;
        }
#endregion

        /// <inheritdoc/>
        protected sealed override void Bind(SettingsRegistry registry)
        {
            // TryGet reports a missing key or a type mismatch itself.
            if (!registry.TryGet(SettingKey, out TSetting setting))
                return;

            Setting = setting;
            Setting.OnValueChanged += OnSettingChanged;

            OnBound();
        }

        /// <inheritdoc/>
        protected sealed override void ResetSetting() => Setting?.ResetToDefault();

        /// <summary>Called once the setting is bound, so the element can show its current value.</summary>
        protected abstract void OnBound();

        /// <summary>Called whenever the setting changes, including on load, revert and reset.</summary>
        protected abstract void OnSettingChanged(TValue value);
    }
}