using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Base class for components that react to settings changes and apply them to the game.
    /// Appliers only listen to settings; they never read storage, so they stay fully decoupled from
    /// how values are persisted. Subscribe in <see cref="OnInitialized"/> and unsubscribe in
    /// <see cref="OnTeardown"/>.
    /// </summary>
    public abstract class SettingsApplier : MonoBehaviour
    {
        /// <summary>
        /// The settings service this applier listens to. Null until <see cref="Initialize"/> has run.
        /// </summary>
        protected ISettingsService Settings { get; private set; }

        private bool _isInitialized;

        /// <summary>
        /// Injects the settings service and starts listening. Safe to call once; later calls are ignored.
        /// </summary>
        /// <param name="settings">The settings service to listen to.</param>
        public void Initialize(ISettingsService settings)
        {
            if (_isInitialized)
                return;

            if (settings == null)
            {
                CustomLogger.LogError("SettingsApplier received a null settings service.", this);
                return;
            }

            Settings = settings;
            _isInitialized = true;

            OnInitialized(settings);
        }

        protected virtual void OnDestroy()
        {
            if (!_isInitialized)
                return;

            OnTeardown();
            _isInitialized = false;
        }

        /// <summary>Called once after the settings service is injected. Subscribe to settings here.</summary>
        /// <param name="settings">The injected settings service.</param>
        protected abstract void OnInitialized(ISettingsService settings);

        /// <summary>Called when the component is destroyed. Unsubscribe from settings here.</summary>
        protected abstract void OnTeardown();
    }
}