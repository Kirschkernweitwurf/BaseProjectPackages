using Base.AttributePackage;
using Base.SettingsPackage.Components;
using Base.SettingsPackage.Core;
using Base.UtilityPackage.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// One rebindable row. Clicking it listens for the next control the player presses and writes the
    /// result back through the <see cref="RebindSetting"/> that owns the asset, so every row on the page
    /// shares a single persisted value and none of them has a key of its own.
    /// </summary>
    /// <remarks>
    /// The action is resolved by id against the asset the <see cref="RebindSetting"/> resolved, not
    /// against the one the <see cref="InputActionReference"/> points at. The reference only names which
    /// action is meant; a project playing with a generated wrapper's clone rebinds that clone.
    /// </remarks>
    [RequireComponent(typeof(Button))]
    public sealed class RebindButton : TypedSettingElement<string, StringSetting>
    {
        private const string MissingBinding = "-";

        // Pointer motion would complete the rebind before the player pressed anything at all.
        private const string MousePositionPath = "<Mouse>/position";

        [Title("Rebind")]
        [Tooltip("The component owning the asset and the persisted overrides. Its key goes in Setting Key.")]
        [SerializeField] [Required] private RebindSetting rebindSetting;

        [Tooltip("Names the action to rebind. Resolved by id against the asset above.")]
        [SerializeField] [Required] private InputActionReference actionReference;

        [Tooltip("Index of the binding inside the action. Every part of a composite has its own index.")]
        [SerializeField] private int bindingIndex;

        [SerializeField] [GetComponent] private Button triggerButton;
        [SerializeField] [Required] private TMP_Text bindingText;

        [Tooltip("Shown while waiting for the player to press something.")]
        [SerializeField] private LocalizedString listeningLabel;

        [Tooltip("Control that aborts the rebind and keeps the old binding. Leave empty to allow none.")]
        [SerializeField] private string cancelPath = "<Keyboard>/escape";

        private InputActionRebindingExtensions.RebindingOperation _operation;
        private InputAction _action;
        private bool _wasActionEnabled;

#region Unity Callbacks
        protected override void OnDestroy()
        {
            base.OnDestroy();

            triggerButton.onClick.RemoveListener(StartRebind);

            // A menu closed while a rebind was running would otherwise leave the action disabled.
            StopRebind();
        }
#endregion

        /// <inheritdoc/>
        protected override void OnBound()
        {
            // The asset is fixed for the lifetime of the scene, so the lookup happens once instead of on
            // every label refresh, each of which would otherwise turn the action id back into a string.
            _action = ResolveAction();

            RefreshLabel();

            triggerButton.onClick.AddListener(StartRebind);
        }

        /// <inheritdoc/>
        protected override void OnSettingChanged(string overridesJson) => RefreshLabel();

        /// <inheritdoc/>
        /// <remarks>
        /// Clears only this row's override. Resetting the shared setting instead would wipe every other
        /// rebind on the page, which is not what a reset on a single row means.
        /// </remarks>
        protected override void ResetSetting()
        {
            if (_action == null)
                return;

            _action.RemoveBindingOverride(bindingIndex);

            rebindSetting.CaptureOverrides();

            // Rebinding back to the default leaves the stored value unchanged, so the setting raises
            // nothing and the label has to be refreshed here rather than through OnSettingChanged.
            RefreshLabel();
        }

        private void StartRebind()
        {
            if (_action == null)
                return;

            StopRebind();

            bindingText.text = listeningLabel.GetLocalizedString();

            // An enabled action cannot be rebound, and re-enabling one that was already off afterwards
            // would quietly switch on a map the game had disabled on purpose.
            _wasActionEnabled = _action.enabled;
            _action.Disable();

            InputActionRebindingExtensions.RebindingOperation operation = _action
                .PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding(MousePositionPath);

            if (!string.IsNullOrEmpty(cancelPath))
                operation = operation.WithCancelingThrough(cancelPath);

            _operation = operation
                .OnCancel(_ => FinishRebind())
                .OnComplete(_ => FinishRebind());

            _operation.Start();
        }

        private void FinishRebind()
        {
            StopRebind();

            rebindSetting.CaptureOverrides();

            RefreshLabel();
        }

        // Ends the operation and puts the action back the way it was found, so a completed rebind, a
        // canceled one and a menu torn down mid-rebind all leave the same state behind.
        private void StopRebind()
        {
            if (_operation == null)
                return;

            _operation.Dispose();
            _operation = null;

            if (_wasActionEnabled)
                _action.Enable();
        }

        private void RefreshLabel() => bindingText.text = _action == null
            ? MissingBinding
            : _action.GetBindingDisplayString(bindingIndex);

        private InputAction ResolveAction()
        {
            InputActionAsset asset = rebindSetting.ActionAsset;
            InputAction source = actionReference.action;

            if (asset == null
                || source == null)
                return null;

            InputAction action = asset.FindAction(source.id.ToString());

            if (action == null)
            {
                CustomLogger.LogError($"Action '{actionReference.name}' is not part of the rebound asset.", this);
                return null;
            }

            if (bindingIndex >= 0
                && bindingIndex < action.bindings.Count)
                return action;

            CustomLogger.LogError($"Binding index {bindingIndex} is outside '{action.name}'.", this);

            return null;
        }
    }
}