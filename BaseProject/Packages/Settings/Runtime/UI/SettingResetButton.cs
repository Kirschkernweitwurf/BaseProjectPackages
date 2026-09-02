using Base.AttributesPackage;
using Base.ServicesPackage;
using Base.SettingsPackage.Core;
using Base.UtilityPackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// Resets one <see cref="SettingElement"/> to its default. Sits next to the element it points at, so a
    /// row can be reset by clicking it rather than by focusing the row and pressing the reset key, which
    /// is the only path <see cref="SettingsEvents.RaiseResetSelected"/> offers.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class SettingResetButton : MonoBehaviour
    {
        [Title("Reset")]
        [Tooltip("The element this button resets.")]
        [SerializeField] [Required] private SettingElement target;

        [SerializeField] [GetComponent] private Button button;

        [Tooltip("Greys the button out while the setting already holds its default value.")]
        [SerializeField] private bool disableWhenDefault = true;

        private SettingsRegistry _registry;

#region Unity Callbacks
        private void OnEnable()
        {
            button.onClick.AddListener(ResetTarget);

            // The registry is only needed to keep the greyed-out state current, so a button that never
            // changes its look neither resolves the context nor subscribes to anything.
            if (!disableWhenDefault)
                return;

            // TryGet reports a missing context itself, so an unbound button stays quiet here.
            if (!ServiceLocator.TryGet(out SettingsContext context))
                return;

            _registry = context.Registry;
            _registry.OnAnyValueChanged += Refresh;

            // Elements bind in Start, so the first state this button can read is one frame away.
            CoroutineRunner.Instance.RunNextFrame(Refresh);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(ResetTarget);

            if (_registry == null)
                return;

            _registry.OnAnyValueChanged -= Refresh;
            _registry = null;
        }
#endregion

        private void ResetTarget()
        {
            target.ResetToDefault();

            Refresh();
        }

        private void Refresh()
        {
            if (!disableWhenDefault)
                return;

            // The delayed first refresh can land after the menu was torn down.
            if (target == null)
                return;

            ISetting setting = target.BoundSetting;

            button.interactable = setting != null
                && !setting.IsDefault;
        }
    }
}