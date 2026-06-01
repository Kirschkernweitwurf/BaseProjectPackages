using System;
using Base.SettingsPackage.Core;
using Base.SystemsCorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

namespace Base.SettingsPackage.GUI
{
    /// <summary>
    /// Base for every settings UI element. Resolves its <see cref="SettingsContext"/>, binds itself to a setting
    /// by key, broadcasts flavor text and input prompts while focused, and resets its bound setting on request.
    /// </summary>
    public abstract class SettingElement : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        /// <summary>Raised with the title and description of the focused element.</summary>
        public static event Action<string, string> OnHoverFlavourChanged;

        [Header("Setting Element")]
        [SerializeField] private string settingKey;
        [SerializeField] private LocalizedString title;
        [SerializeField] private LocalizedString description;

        /// <summary>Key of the setting this element binds to.</summary>
        protected string SettingKey => settingKey;

        protected virtual void Start()
        {
            if (!ServiceLocator.TryGet(out SettingsContext settingsContext))
                return;

            if (settingsContext.Registry == null)
            {
                CustomLogger.LogError($"{nameof(SettingsContext)} has no registry; cannot bind {name}", this);
                return;
            }

            Bind(settingsContext.Registry);
        }

        protected virtual void OnEnable()
        {
            SettingsEvents.OnResetSelected += HandleResetSelected;

            if (EventSystem.current == null)
            {
                CustomLogger.LogWarning($"No {nameof(EventSystem)} found in scene;" +
                                        $" {name} cannot respond to reset events", this);
                return;
            }

            if (EventSystem.current.currentSelectedGameObject == gameObject)
                OnSelect(null);
        }

        protected virtual void OnDisable() => SettingsEvents.OnResetSelected -= HandleResetSelected;

        /// <summary>Broadcasts this element's flavor text and prompts.</summary>
        public virtual void OnSelect(BaseEventData eventData)
            => OnHoverFlavourChanged?.Invoke(title.GetLocalizedString(), description.GetLocalizedString());

        /// <inheritdoc/>
        public void OnPointerEnter(PointerEventData eventData) => OnSelect(eventData);

        /// <summary>Resolves the registry and wires this element to its setting.</summary>
        protected abstract void Bind(SettingsRegistry registry);

        /// <summary>Resets the bound setting to its default. Called only while this element is focused.</summary>
        protected abstract void ResetSetting();

        private void HandleResetSelected()
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
                ResetSetting();
        }
    }
}