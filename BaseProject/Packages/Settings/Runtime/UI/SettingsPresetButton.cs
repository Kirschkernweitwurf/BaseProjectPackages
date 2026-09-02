using Base.AttributesPackage;
using Base.ServicesPackage;
using Base.SettingsPackage.Core;
using Base.SettingsPackage.Presets;
using Base.UtilityPackage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// Applies a <see cref="SettingsPreset"/> on click and shows whether the current settings still match
    /// it. Put one per preset next to each other for a Low, Medium and High row; the indicators go dark on
    /// all of them as soon as the player tunes something by hand, which is the honest state to show.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class SettingsPresetButton : MonoBehaviour
    {
        [Title("Preset")]
        [Tooltip("The preset this button applies.")]
        [SerializeField] [Required] private SettingsPreset preset;

        [SerializeField] [GetComponent] private Button button;

        [Tooltip("Optional label filled with the preset's own display name.")]
        [SerializeField] private TMP_Text label;

        [Tooltip("Optional object shown while every value in the preset matches the current settings.")]
        [SerializeField] private GameObject activeIndicator;

        private SettingsRegistry _registry;
        private bool _isFollowingChanges;

#region Unity Callbacks
        private void OnEnable()
        {
            button.onClick.AddListener(ApplyPreset);

            if (label != null)
                label.text = preset.DisplayName.GetLocalizedString();

            // TryGet reports a missing context itself, so an unbound button stays quiet here.
            if (!ServiceLocator.TryGet(out SettingsContext context))
                return;

            _registry = context.Registry;

            // Without an indicator there is nothing to keep current, so the walk over every entry of every
            // preset is skipped rather than run on each change and thrown away.
            if (activeIndicator == null)
                return;

            _registry.OnAnyValueChanged += Refresh;
            _isFollowingChanges = true;

            // Setting components load their values during startup, so the match is only meaningful once
            // every one of them has had its turn.
            CoroutineRunner.Instance.RunNextFrame(Refresh);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(ApplyPreset);

            // Tracked with a flag rather than re-checking the indicator, which may already be destroyed by
            // now and would leave this subscribed to a registry that outlives the menu.
            if (_isFollowingChanges)
            {
                _registry.OnAnyValueChanged -= Refresh;
                _isFollowingChanges = false;
            }

            _registry = null;
        }
#endregion

        private void ApplyPreset()
        {
            if (_registry == null)
                return;

            preset.Apply(_registry);
        }

        private void Refresh()
        {
            // The delayed first refresh can land after the menu was torn down.
            if (activeIndicator == null)
                return;

            activeIndicator.SetActive(_registry != null
                && preset.IsActive(_registry));
        }
    }
}