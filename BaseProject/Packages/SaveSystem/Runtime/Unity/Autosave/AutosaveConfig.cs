using Base.AttributePackage;
using Base.UtilityPackage.Menus;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Autosave
{
    /// <summary>
    /// The defaults the autosave starts from. One asset per project, referenced by the
    /// <see cref="AutosaveService"/> and by the settings components that persist the player's choices,
    /// so a default lives in exactly one place instead of being retyped on every component that needs it.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Save/New Autosave Config", "AC_AutosaveConfig")]
    public sealed class AutosaveConfig : ScriptableObject
    {
        private const float DefaultCooldownSeconds = 60f;
        private const float DefaultIntervalSeconds = 300f;

        [Title("Defaults")]
        [Tooltip("Whether autosaving is on before the player has ever changed the setting.")]
        [SerializeField] private bool autosaveEnabled = true;

        [Tooltip("Seconds between timed autosaves before the player has ever changed the setting. "
            + "0 turns the timer off and leaves only requests.")]
        [Min(0f)]
        [Suffix(SuffixAttribute.Second)]
        [SerializeField] private float intervalSeconds = DefaultIntervalSeconds;

        [Tooltip("Shortest gap between two autosaves. A request that arrives sooner is not dropped; it "
            + "runs as soon as the gap has passed.")]
        [Min(0f)]
        [Suffix(SuffixAttribute.Second)]
        [SerializeField] private float cooldownSeconds = DefaultCooldownSeconds;

        [Title("Target")]
        [Tooltip("Where an autosave is written.")]
        [EnumToggleButtons]
        [SerializeField] private EAutosaveTarget target = EAutosaveTarget.DedicatedSlot;

        [Tooltip("The slot id autosaves are written to. Keep it stable across versions.")]
        [ShowIfEnum(nameof(target), EAutosaveTarget.DedicatedSlot)]
        [NotNullOrEmpty]
        [SerializeField] private string dedicatedSlotId = "autosave";

        [Tooltip("Name shown for the autosave in a load menu.")]
        [SerializeField] private string displayName = "Autosave";

        [Title("Content")]
        [Tooltip("Store a thumbnail with the autosave. Needs a screenshot capturer in the scene.")]
        [SerializeField] private bool captureScreenshot = true;

        [Tooltip("Save when the app loses focus or is paused. Ignores the cooldown, since the app may "
            + "never get another frame.")]
        [SerializeField] private bool saveOnFocusLoss = true;

        /// <summary>Whether autosaving is on before the player has ever changed the setting.</summary>
        public bool AutosaveEnabled => autosaveEnabled;

        /// <summary>Seconds between timed autosaves before the player has ever changed the setting.</summary>
        public float IntervalSeconds => intervalSeconds;

        /// <summary>Shortest gap between two autosaves before the player has ever changed the setting.</summary>
        public float CooldownSeconds => cooldownSeconds;

        /// <summary>Where an autosave is written.</summary>
        public EAutosaveTarget Target => target;

        /// <summary>The slot id autosaves are written to.</summary>
        public string DedicatedSlotId => dedicatedSlotId;

        /// <summary>Name shown for the autosave in a load menu.</summary>
        public string DisplayName => displayName;

        /// <summary>Whether a thumbnail is stored with the autosave.</summary>
        public bool CaptureScreenshot => captureScreenshot;

        /// <summary>Whether losing focus triggers a save.</summary>
        public bool SaveOnFocusLoss => saveOnFocusLoss;
    }
}