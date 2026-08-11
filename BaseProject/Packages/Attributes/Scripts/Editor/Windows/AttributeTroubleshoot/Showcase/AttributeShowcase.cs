using System.Collections.Generic;
using Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples;
using Base.UtilityPackage.Collections;
using Base.UtilityPackage.Serialization;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Showcase
{
    /// <summary>
    /// A throwaway asset whose only job is to carry one of every attribute, so the showcase tab can draw
    /// them through the real pipeline. The window creates it in memory and never saves it, so edits made
    /// here affect nothing.
    /// </summary>
    /// <remarks>
    /// The fields are public rather than serialized and private, which is the opposite of how a real
    /// component should be written. Nothing here is ever read from code, and a fixture full of write-only
    /// private fields is a fixture full of compiler warnings. Public fields also make each line read like
    /// the usage examples in the README, which is the point of the tab.
    /// </remarks>
    public sealed class AttributeShowcase : ScriptableObject
    {
        private const float FullHealth = 100f;
        private const int LongOptionCount = 40;
        private const int MaximumRoll = 7;
        private const int MinimumRoll = 1;
        private const int OptionsPerGroup = 10;

        [Title("Layout", EColor.Cyan)]
        [InfoBox("Titles, info boxes and lines are drawn before the field they sit on.")]
        public string label = "Editable text";

        [HorizontalLine(EColor.Gray)]
        [Prefix("Speed")]
        [Suffix("m/s")]
        public float speed = 3.5f;

        [Indent] public string indented = "One level in";

        [Foldout("Extra")]
        public string insideFoldout = "Grouped away";

        [Foldout("Extra")]
        public int alsoInsideFoldout = 2;

        [Title("Validation", EColor.Orange)]
        [Required] public Transform requiredTarget;

        public bool usesCustomIcon;

        [RequiredIf(nameof(usesCustomIcon))]
        public Texture2D conditionalIcon;

        [MustImplement(typeof(Collider))]
        public GameObject colliderHost;

        [MinMax(0f, FullHealth)] public int clamped = 50;

        [MaxLength(8)] public string shortCode = "ABC";

        [ValidateInput(nameof(IsEven), "Has to be even.")]
        public int evenOnly = 4;

        [Title("Conditions", EColor.Green)]
        public bool isAdvanced;

        [ShowIf(nameof(usesCustomIcon), nameof(isAdvanced))]
        public string needsBoth = "Visible while both toggles are on";

        [ShowIf(EConditionMode.Any, nameof(usesCustomIcon), nameof(isAdvanced))]
        public string needsEither = "Visible while either toggle is on";

        [EnableIf(nameof(isAdvanced))] public int advancedOnly = 1;

        [ReadOnly] public string neverEditable = "Locked";

        [Title("Widgets", EColor.Blue)]
        [ProgressBar(FullHealth, EColor.Green)]
        public float health = 72f;

        [Percentage(true)] public float ratio = 0.35f;

        [MinMaxSlider(0f, FullHealth)] public Vector2 range = new(20f, 80f);

        [CurveRange(0f, 1f, EColor.Cyan)]
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [EnumToggleButtons] public ESampleMode mode = ESampleMode.Normal;

        [Dropdown(nameof(ShortOptions))]
        public string shortDropdown = "Alpha";

        [Dropdown(nameof(LongOptions))]
        public string searchableDropdown = "Group 0/Entry 00";

        [InlineButton(nameof(Roll), "Roll")]
        public int rolled = 6;

        [ClearButton] public string clearable = "Clear me";

        [CopyButton] public string copyable = "Copy me";

        [Title("Pickers", EColor.Purple)]
        [Tag] public string tagName = "Untagged";

        [Layer] public int layer;

        [SortingLayer] public string sortingLayer = "Default";

        [SceneName] public string scene;

        public Material material;

        [ShaderParam(nameof(material))]
        public string shaderProperty;

        [Title("References", EColor.Teal)]
        [Expandable] public ScriptableObject inlineAsset;

        [SerializeReference] [ReferencePicker] public ISampleAbility ability;

        public InterfaceReference<ISampleAbility> interfaceTarget = new();

        [Title("Collections", EColor.Yellow)]
        public SerializableDictionary<string, int> weights = new();

        public SerializableHashSet<string> uniqueNames = new();

        [OnArraySizeChanged(nameof(OnSlotsResized))]
        public List<string> slots = new();

        [OnValueChanged(nameof(OnTrackedValueChanged))]
        public int trackedValue;

        [ReadOnly] public string lastCallback = "No callback yet";

        private string[] ShortOptions => new[]
        {
            "Alpha",
            "Beta",
            "Gamma"
        };

        // Long enough to cross the threshold where the plain popup becomes the searchable dropdown, and
        // slash-separated so the submenu grouping is visible too.
        private string[] LongOptions
        {
            get
            {
                string[] options = new string[LongOptionCount];
                for (int i = 0; i < LongOptionCount; i++)
                    options[i] = $"Group {i / OptionsPerGroup}/Entry {i:00}";

                return options;
            }
        }

        /// <summary>Demonstrates a plain inspector button, drawn below the fields.</summary>
        [Button("Reset Values")]
        public void ResetValues()
        {
            health = FullHealth;
            trackedValue = 0;
            lastCallback = $"{nameof(ResetValues)} ran";
        }

        /// <summary>
        /// Demonstrates a button in the component header. Not visible here, because an embedded
        /// inspector draws the body but not the header.
        /// </summary>
        [HeaderButton("Ping")]
        public void PingSelf() => lastCallback = $"{nameof(PingSelf)} ran";

        private void Roll() => rolled = Random.Range(MinimumRoll, MaximumRoll);

        private void OnTrackedValueChanged() => lastCallback = $"{nameof(trackedValue)} is now {trackedValue}";

        private void OnSlotsResized(int size) => lastCallback = $"{nameof(slots)} resized to {size}";

        private bool IsEven(int value) => value % 2 == 0;
    }
}