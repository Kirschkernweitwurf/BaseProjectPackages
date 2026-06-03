using UnityEngine;

namespace Base.SettingsPackage.Core
{
    /// <summary>
    /// A ScriptableObject used to uniquely identify a single setting. Acts as the binding
    /// target for UI elements and the lookup key in the registry. <see cref="StorageKey"/>
    /// is the string used for persistence.
    /// </summary>
    [CreateAssetMenu(fileName = "SettingIdentifier", menuName = "ScriptableObjects/Settings/Setting Identifier")]
    public class SettingIdentifier : ScriptableObject
    {
        [Tooltip("Key used to save/load this setting. Leave empty to use the asset name.")]
        [SerializeField] private string storageKey;

        /// <summary>The string key used to store and load this setting's value.</summary>
        public string StorageKey => string.IsNullOrWhiteSpace(storageKey) ? name : storageKey;
    }
}