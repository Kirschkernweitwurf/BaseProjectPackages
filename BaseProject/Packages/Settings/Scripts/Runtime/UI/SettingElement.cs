using System;
using Base.AttributePackage;
using Base.CorePackage.Services;
using Base.SettingsPackage.Core;
using Base.ToolPackage.Identification;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// Non-generic base for every settings UI element. Resolves the <see cref="SettingsContext"/>, broadcasts
    /// flavor text while focused, and resets its bound setting on request. Concrete elements inherit from
    /// <see cref="TypedSettingElement{TValue, TSetting}"/>, not this type.
    /// </summary>
    public abstract class SettingElement : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        /// <summary>Raised with the title and description of the focused element.</summary>
        public static event Action<string, string> OnHoverFlavorChanged;

        [Header("Setting Element")]

        [SerializeField] [NotNullOrEmpty] private string settingKey;
        [SerializeField] private LocalizedString title;
        [SerializeField] private LocalizedString description;

        /// <summary>Key of the setting this element binds to. Built once in <see cref="Awake"/>.</summary>
        protected PersistentKey SettingKey { get; private set; }

#region Unity Callbacks
        protected virtual void Awake() => SettingKey = new(settingKey);

        protected virtual void OnEnable()
        {
            SettingsEvents.OnResetSelected += HandleResetSelected;

            if (EventSystem.current == null)
            {
                CustomLogger.LogWarning(
                    $"No {nameof(EventSystem)} found in scene; {name} cannot respond to reset events.", this);

                return;
            }

            if (EventSystem.current.currentSelectedGameObject == gameObject)
                OnSelect(eventData: null);
        }

        protected virtual void Start()
        {
            // TryGet reports a missing context itself, so an unbound element stays quiet here.
            if (!ServiceLocator.TryGet(out SettingsContext context))
                return;

            Bind(context.Registry);
        }

        protected virtual void OnDisable() => SettingsEvents.OnResetSelected -= HandleResetSelected;
#endregion

        /// <inheritdoc/>
        public void OnPointerEnter(PointerEventData eventData) => OnSelect(eventData);

        /// <summary>Broadcasts this element's flavor text.</summary>
        public virtual void OnSelect(BaseEventData eventData)
            => OnHoverFlavorChanged?.Invoke(title.GetLocalizedString(), description.GetLocalizedString());

        /// <summary>Wires this element to its setting in the given registry.</summary>
        protected abstract void Bind(SettingsRegistry registry);

        /// <summary>Resets the bound setting to its default. Called only while this element is focused.</summary>
        protected abstract void ResetSetting();

        private void HandleResetSelected()
        {
            if (EventSystem.current != null
                && EventSystem.current.currentSelectedGameObject == gameObject)
                ResetSetting();
        }
    }
}