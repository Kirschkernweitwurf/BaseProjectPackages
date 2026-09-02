using Base.AttributesPackage;
using Base.SettingsPackage.Controls;
using Base.UtilityPackage.Identification;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Persists every binding override of one <see cref="InputActionAsset"/> as the JSON the input system
    /// writes itself. One setting covers the whole asset rather than one key per binding, so a rebind row
    /// added later needs no migration and nothing has to enumerate the bindings to save them.
    /// </summary>
    /// <remarks>
    /// The overrides land on the asset instance this resolves, so it has to be the one the game actually
    /// plays with. A project whose input comes from a generated wrapper plays with that wrapper's clone,
    /// not with the source asset, and has to subclass this and return the clone from
    /// <see cref="ResolveAsset"/>.
    /// </remarks>
    public class RebindSetting : StringSettingComponent
    {
        [Title("Rebinds")]
        [Tooltip("The action asset the overrides are applied to. Leave empty when a subclass supplies it.")]
        [SerializeField] private InputActionAsset actionAsset;

        /// <summary>The asset the overrides are applied to, or null when none was resolved.</summary>
        public InputActionAsset ActionAsset { get; private set; }

        /// <inheritdoc/>
        public override PersistentKey Key => ControlSettingKeys.Rebinds;

        /// <inheritdoc/>
        protected override string DefaultValue => string.Empty;

#region Unity Callbacks
        // Resolved before registering, because registering is what makes the first Apply reachable.
        protected override void Awake()
        {
            ActionAsset = ResolveAsset();

            // Nothing else reports this: the serialized field is deliberately optional so a subclass can
            // supply the asset instead, which leaves both being empty as the one case worth shouting about.
            if (ActionAsset == null)
                CustomLogger.LogError("No action asset was resolved, so no rebind is stored or applied.", this);

            base.Awake();
        }
#endregion

        /// <summary>
        /// Copies the asset's current overrides into the setting. Call after changing a binding, so the
        /// change is persisted with everything else on the next save.
        /// </summary>
        public void CaptureOverrides()
        {
            if (ActionAsset == null
                || TypedSetting == null)
                return;

            // An asset with nothing overridden still serializes to a JSON envelope, which would read as a
            // value differing from the default and leave the whole page looking permanently modified.
            TypedSetting.Value = HasAnyOverride()
                ? ActionAsset.SaveBindingOverridesAsJson()
                : string.Empty;
        }

        /// <summary>
        /// Supplies the asset the overrides apply to. Override to return the runtime clone a generated
        /// actions wrapper creates, which is the instance the game reads and the source asset is not.
        /// </summary>
        /// <returns>The asset to apply overrides to.</returns>
        protected virtual InputActionAsset ResolveAsset() => actionAsset;

        /// <inheritdoc/>
        protected override void Apply(string overridesJson)
        {
            if (ActionAsset == null)
                return;

            // An empty value is the default, which means no overrides at all rather than nothing to do:
            // resetting has to actively clear whatever the player had bound before.
            if (string.IsNullOrEmpty(overridesJson))
            {
                ActionAsset.RemoveAllBindingOverrides();
                return;
            }

            ActionAsset.LoadBindingOverridesFromJson(overridesJson);
        }

        private bool HasAnyOverride()
        {
            foreach (InputBinding binding in ActionAsset.bindings)
            {
                if (binding.hasOverrides)
                    return true;
            }

            return false;
        }
    }
}