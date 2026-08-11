using Base.AttributePackage;
using Base.ToolPackage.MenuManagerWindow;
using UnityEngine;

namespace Base.ControllerSupport.Haptics
{
    /// <summary>
    /// The defaults every rumble consumer starts from. One asset per project, referenced by the
    /// <see cref="RumbleService"/> and by the settings components that persist the player's choices, so
    /// a default lives in exactly one place instead of being retyped on every component that needs it.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Input/New Rumble Config", "RC_RumbleConfig")]
    public sealed class RumbleConfig : ScriptableObject
    {
        private const float FullIntensity = 1f;

        [Title("Defaults")]
        [Tooltip("Whether rumble is on before the player has ever changed the setting.")]
        [SerializeField] private bool rumbleEnabled = true;

        [Tooltip("Global strength before the player has ever changed the setting.")]
        [Percentage(slider: true)]
        [SerializeField] private float mainIntensity = FullIntensity;

        /// <summary>Whether rumble is on before the player has ever changed the setting.</summary>
        public bool RumbleEnabled => rumbleEnabled;

        /// <summary>Global strength before the player has ever changed the setting.</summary>
        public float MainIntensity => mainIntensity;
    }
}