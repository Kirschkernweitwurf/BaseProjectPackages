using Base.CorePackage.Services;
using UnityEngine;

namespace Base.SettingsPackage.Core
{
    /// <summary>
    /// Scene-level owner of a <see cref="SettingsRegistry"/> and its backing <see cref="ISettingsStore"/>.
    /// Setting components resolve this through the <see cref="ServiceLocator"/> in their own <see cref="Awake"/>,
    /// register themselves and load their persisted value immediately.
    /// </summary>
    [DefaultExecutionOrder(-98)]
    public class SettingsContext : GameServiceBehaviour
    {
        /// <summary>The registry that holds the project's settings.</summary>
        public SettingsRegistry Registry { get; private set; }

        /// <summary>The store the registry persists to.</summary>
        public ISettingsStore Store { get; private set; }

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            Store = new PlayerPrefsSettingsStore();
            Registry = new SettingsRegistry(Store, this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Save();
        }
#endregion

        /// <summary>Persists all settings.</summary>
        public void Save() => Registry?.SaveAll();

        /// <summary>Restores all settings to their last persisted value.</summary>
        public void Revert() => Registry?.RevertAll();

        /// <summary>Restores all settings to their default value.</summary>
        public void ResetToDefaults() => Registry?.ResetAllToDefault();

        /// <summary>Reloads every registered setting from the store and reapplies it.</summary>
        public void Reload() => Registry?.LoadAll();
    }
}