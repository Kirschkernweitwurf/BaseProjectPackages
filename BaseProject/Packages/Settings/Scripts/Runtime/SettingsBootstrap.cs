using UnityEngine;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Scene-level entry point for the settings system. Builds the <see cref="GameSettingsService"/>
    /// on top of <see cref="PlayerPrefsSettingsStore"/> and injects it into the assigned appliers.
    /// Keeping the service behind <see cref="ISettingsService"/> and injecting it manually means no
    /// system needs a global singleton or static access to reach the settings.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// 1. Place this component on a bootstrap GameObject in your first scene.
    /// 2. Add the applier components (audio, display, ...) and drag them into the Appliers list.
    /// 3. Read or change values through <see cref="Settings"/>, for example from a settings menu.
    /// </remarks>
    public sealed class SettingsBootstrap : MonoBehaviour
    {
        [Tooltip("Components that receive the settings service once it has been created.")]
        [SerializeField] private SettingsApplier[] appliers;

        [Tooltip("If true, settings are flushed to disk automatically when the application quits.")]
        [SerializeField] private bool saveOnQuit = true;

        private GameSettingsService _settings;

        /// <summary>The settings service created by this bootstrap. Null before <c>Awake</c> has run.</summary>
        public ISettingsService Settings => _settings;

        private void Awake()
        {
            ISettingsStore store = new PlayerPrefsSettingsStore();
            _settings = new GameSettingsService(store);

            InitializeAppliers();
        }

        private void OnApplicationQuit()
        {
            if (!saveOnQuit)
                return;

            _settings?.Save();
        }

        private void InitializeAppliers()
        {
            if (appliers == null)
                return;

            foreach (SettingsApplier applier in appliers)
            {
                if (applier == null)
                    continue;

                applier.Initialize(_settings);
            }
        }
    }
}