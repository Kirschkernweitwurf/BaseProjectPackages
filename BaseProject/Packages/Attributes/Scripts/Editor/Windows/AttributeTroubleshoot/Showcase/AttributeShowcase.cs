using System;
using System.Collections.Generic;
using Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples;
using Base.UtilityPackage.Collections;
using Base.UtilityPackage.Serialization;
using UnityEngine;
using UnityEngine.Audio;

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
    /// <para>
    /// This is an asset, not a component, so the three families that need a GameObject are absent: the
    /// scene handles, the hierarchy auto-getters and the scene object constraint. Everything else the
    /// attribute tester shows is here, in the same order and under the same headings.
    /// </para>
    /// </remarks>
    internal sealed class AttributeShowcase : ScriptableObject
    {
        private const float DefaultContextValue = 10f;
        private const float FullHealth = 100f;
        private const int LongOptionCount = 40;
        private const int MaximumRoll = 7;
        private const int MinimumRoll = 1;
        private const int OptionsPerGroup = 10;

        // The section colors are one pastel rainbow, evenly spaced around the hue circle and leveled to
        // the same perceived lightness, so no heading shouts louder than its neighbors and every one of
        // them stays readable on both editor skins.
        [Title("0. Sources", "#FFB8B8", Foldout = true, DefaultExpanded = true)]
        [InfoBox("Assign these first. The pickers further down read their options from these fields. An "
            + "asset can only reference other assets, so the Animator and the Renderer have to come from "
            + "a prefab.")]
        public Animator animator;

        /// <summary>Feeds the mixer parameter and group pickers.</summary>
        public AudioMixer mixer;

        /// <summary>Feeds the shader property and keyword pickers.</summary>
        public Material material;

        /// <summary>Alternative shader source, to show the pickers accept more than a Material.</summary>
        public Renderer sourceRenderer;

        [Title("1. Layout", "#F9BD96", Foldout = true, DefaultExpanded = false)]
        [InfoBox("Info boxes default to Info and sit above their field.")]
        public string infoBoxDefault = "Below a default info box";

        [InfoBox("Warning styling, still above the field.", EInfoBoxType.Warning)]
        public string infoBoxWarning = "Below a warning box";

        [InfoBox("Error styling. Use this when the setup is genuinely broken.", EInfoBoxType.Error)]
        public string infoBoxError = "Below an error box";

        [InfoBox("No icon at all, just the text.", EInfoBoxType.None)]
        public string infoBoxNone = "Below an icon-less box";

        [InfoBox("This one is drawn below its field.", EInfoBoxType.Info, EInfoBoxPosition.Below)]
        public string infoBoxBelow = "Above its own info box";

        [InfoBox("Compact boxes take a single line.", EInfoBoxType.Info, EInfoBoxPosition.Above, true)]
        public string infoBoxCompact = "Below a compact box";

        [InfoBox("Colored through the EColor overload.", EColor.Purple)]
        public string infoBoxColored = "Below a purple box";

        [HorizontalLine]
        public string lineDefault = "Below a default line";

        [HorizontalLine(EColor.Orange, 3f, 12f)]
        public string lineColored = "Below a thick orange line";

        [HorizontalLine("#3FBF7F", 2f)]
        public string lineHex = "Below a hex-colored line";

        /// <summary>No indent, for comparison with the two fields below it.</summary>
        public string indentNone = "Not indented";

        [Indent] public string indentOne = "One level in";

        [Indent(3)] public string indentThree = "Three levels in";

        [Prefix("Speed")] public float prefixed = 3.5f;

        [Suffix("m/s")] public float suffixed = 7f;

        [Prefix("from")]
        [Suffix("meters")]
        public float prefixedAndSuffixed = 12f;

        [GUIColor(EColor.Lime)] public string tintedByPreset = "Lime tinted";

        [GUIColor("#FF7F50")] public string tintedByHex = "Coral tinted";

        [Foldout("Grouped fields")] public string foldoutFirst = "Inside the foldout";

        [Foldout("Grouped fields")] public int foldoutSecond = 2;

        [Foldout("Grouped fields")] public bool foldoutThird = true;

        [Foldout("Another group")] public Vector3 secondGroupFirst = Vector3.one;

        [Foldout("Another group")] public Color secondGroupSecond = Color.cyan;

        [Tab("General", "Settings", Foldout = true)]
        public string tabGeneralName = "General tab";

        [Tab("General", "Settings")] public int tabGeneralCount = 1;

        [Tab("Advanced", "Settings")] public float tabAdvancedThreshold = 0.5f;

        [Tab("Advanced", "Settings")] public bool tabAdvancedVerbose;

        [Tab("Debug", "Settings")] public string tabDebugNote = "Debug tab";

        [Title("2. Conditions", "#DAC883", Foldout = true, DefaultExpanded = false)]
        [InfoBox("Toggle the two bools and the enum below and watch the fields under them appear, "
            + "disappear and grey out.")]
        public bool isEnabled = true;

        /// <summary>Second toggle, used by the multi-condition fields to show All and Any modes.</summary>
        public bool isAdvanced;

        /// <summary>Drives the enum conditions.</summary>
        public ESampleMode mode = ESampleMode.Normal;

        [ShowIf(nameof(isEnabled))] public string showIfSingle = "Shown while enabled";

        [ShowIf(nameof(isEnabled), nameof(isAdvanced))]
        public string showIfAll = "Shown while both are on";

        [ShowIf(EConditionMode.Any, nameof(isEnabled), nameof(isAdvanced))]
        public string showIfAny = "Shown while either is on";

        [ShowIf(nameof(HasAdvancedSetup))] public string showIfProperty = "Driven by a property";

        [ShowIf(nameof(IsFast))] public string showIfMethod = "Driven by a method";

        [HideIf(nameof(isEnabled))] public string hideIfSingle = "Hidden while enabled";

        [HideIf(EConditionMode.Any, nameof(isEnabled), nameof(isAdvanced))]
        public string hideIfAny = "Hidden while either is on";

        [EnableIf(nameof(isEnabled))] public int enableIfSingle = 5;

        [EnableIf(nameof(isEnabled), nameof(isAdvanced))] public int enableIfAll = 10;

        [DisableIf(nameof(isEnabled))] public int disableIfSingle = 15;

        [ShowIfEnum(nameof(mode), ESampleMode.Fast)] public float fastSpeed = 6f;

        [ShowIfEnum(nameof(mode), ESampleMode.Normal, ESampleMode.Fast)]
        public float movingSpeed = 3f;

        [ShowInPlayMode] public string playModeOnly = "Play mode only";

        [HideInPlayMode] public string editModeOnly = "Edit mode only";

        [EnableInPlayMode] public float tunableInPlayMode = 1f;

        [DisableInPlayMode] public float lockedInPlayMode = 2f;

        [ReadOnly] public string alwaysReadOnly = "Look but do not touch";

        [ReadOnlyInPlayMode] public int readOnlyInPlayMode = 3;

        [ReadOnlyInEditMode] public int readOnlyInEditMode = 4;

        [Title("3. Validation", "#C1D07D", Foldout = true, DefaultExpanded = false)]
        [InfoBox("Clear a required field or break a rule to see the box appear under it.")]
        [Required] public Material requiredDefault;

        [Required("Assign the icon or nothing will render.")]
        public Texture2D requiredWithMessage;

        [RequiredIf(nameof(isAdvanced))] public Texture2D requiredIfSingle;

        [RequiredIf(EConditionMode.Any, nameof(isEnabled), nameof(isAdvanced))]
        public Texture2D requiredIfAny;

        [RequiredIf(nameof(isAdvanced), Message = "Advanced mode needs an override asset.")]
        public ScriptableObject requiredIfWithMessage;

        [MustImplement(typeof(Collider))]
        public GameObject mustImplementSingle;

        [MustImplement(typeof(Collider), typeof(Renderer))]
        public GameObject mustImplementMultiple;

        [Max(50f)] public float maxOnly = 20f;

        [MinMax(0f, FullHealth)] public int clampedRange = 50;

        [NotZero] public float notZeroDefault = 1f;

        [NotZero(0.25f)] public float notZeroSmallStep = 0.25f;

        [MaxLength(12)] public string maxLength = "Twelve chars";

        [NotNullOrEmpty] public string notNullOrEmptyDefault = "Not empty";

        [NotNullOrEmpty("Give the profile a name.")]
        public string notNullOrEmptyWithMessage = "Named";

        [PowerOfTwo] public int powerOfTwo = 64;

        [Unique] public List<string> uniqueDefault = new();

        [Unique("Every layer name has to appear once.")]
        public List<string> uniqueWithMessage = new();

        [ValidateInput(nameof(IsEven))] public int validateWithParameter = 4;

        [ValidateInput(nameof(HasMaterial), "Assign the material in section 0 first.")]
        public float validateParameterless = 1f;

        [AssetOnly] public GameObject assetOnly;

        [Title("4. Pickers", "#A6D881", Foldout = true, DefaultExpanded = false)]
        [InfoBox("The ones reading from section 0 fall back to a plain field and say what is missing "
            + "while their source is empty. That fallback is worth seeing once.")]
        [Tag] public string tagDefault = "Untagged";

        [Tag(true)] public string tagOnlyExisting = "Untagged";

        [Layer] public int layerIndex;

        [Layer] public string layerName = "Default";

        /// <summary>Plain mask for comparison. Unity draws this one on its own.</summary>
        public LayerMask layerMask;

        [SortingLayer] public string sortingLayerName = "Default";

        [SortingLayer] public int sortingLayerId;

        [SceneName] public string sceneName;

        [FilePath] public string filePathAny;

        [FilePath("json")] public string filePathJson;

        [FilePath("txt", true)] public string filePathAbsolute;

        [FolderPath] public string folderPathRelative;

        [FolderPath(true)] public string folderPathAbsolute;

        [ResourcesPath] public string resourcesPathAny;

        [ResourcesPath(typeof(Texture2D))] public string resourcesPathTextures;

        [ComponentPicker] public Collider pickedComponent;

        [OpenAsset] public TextAsset openAssetInline;

        [OpenAsset("Edit")] public TextAsset openAssetLabeled;

        [ShowAssetPreview] public Sprite previewDefault;

        [ShowAssetPreview(128)] public Sprite previewLarge;

        [AnimatorParam(nameof(animator))] public string animatorParamAny;

        [AnimatorParam(nameof(animator), AnimatorControllerParameterType.Trigger)]
        public string animatorParamTrigger;

        [AnimatorParam(nameof(animator), AnimatorControllerParameterType.Float)]
        public int animatorParamFloatHash;

        [AnimatorState(nameof(animator))] public string animatorStateName;

        [AnimatorState(nameof(animator))] public int animatorStateHash;

        [MixerParameter(nameof(mixer))] public string mixerParameter;

        [AudioMixerGroup] public AudioMixerGroup mixerGroupAny;

        [AudioMixerGroup(nameof(mixer))] public AudioMixerGroup mixerGroupFromField;

        [ShaderParam(nameof(material))] public string shaderParamAny;

        [ShaderParam(nameof(material), EShaderParamType.Color)] public string shaderParamColor;

        [ShaderParam(nameof(material), EShaderParamType.Texture)] public string shaderParamTexture;

        [ShaderParam(nameof(material), EShaderParamType.Vector)] public string shaderParamVector;

        [ShaderParam(nameof(material), EShaderParamType.Integer)] public string shaderParamInteger;

        [ShaderParam(nameof(sourceRenderer), EShaderParamType.Float)] public int shaderParamFloatId;

        [ShaderKeyword(nameof(material))] public string shaderKeyword;

        [ShaderKeyword(nameof(sourceRenderer))] public string rendererKeyword;

        [Title("5. Widgets", "#89E086", Foldout = true, DefaultExpanded = false)]
        [InfoBox("The last dropdown has forty options, which crosses the threshold where the popup "
            + "becomes a searchable tree.")]
        [Dropdown(nameof(shortOptions))]
        public string dropdownFromField = "Alpha";

        [Dropdown(nameof(PropertyOptions))] public string dropdownFromProperty = "First";

        [Dropdown(nameof(GetMethodOptions))] public int dropdownFromMethod = 10;

        [Dropdown(nameof(SearchableOptions))] public string dropdownSearchable = "Group 0/Entry 00";

        /// <summary>Backing array for the field-driven dropdown above.</summary>
        public string[] shortOptions =
        {
            "Alpha",
            "Beta",
            "Gamma"
        };

        /// <summary>Maximum used by the member-driven progress bar below.</summary>
        public float maxHealth = FullHealth;

        [ProgressBar(FullHealth)] public float progressConstant = 72f;

        [ProgressBar(nameof(maxHealth), EColor.Green)] public float progressFromMember = 40f;

        [ProgressBar(FullHealth, EColor.Red, true)] public float progressReadOnly = 88f;

        [ProgressBar(10f, EColor.Cyan)] public int progressInteger = 6;

        [Percentage] public float percentageField = 0.35f;

        [Percentage(true)] public float percentageSlider = 0.6f;

        [MinMaxSlider(0f, FullHealth)] public Vector2 minMaxSlider = new(20f, 80f);

        [CurveRange(0f, 1f)]
        public AnimationCurve curveSquare = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [CurveRange(0f, -1f, 5f, 1f, EColor.Magenta)]
        public AnimationCurve curveWide = AnimationCurve.EaseInOut(0f, -1f, 5f, 1f);

        [EnumFlags] public ESampleFlags flags = ESampleFlags.Fire;

        [EnumToggleButtons] public ESampleMode toggleButtons = ESampleMode.Normal;

        [InlineButton(nameof(Roll))] public int inlineButtonDefault = 1;

        [InlineButton(nameof(Roll), "Roll")] public int inlineButtonLabeled = 6;

        [ClearButton] public string clearInline = "Clear me";

        [CopyButton] public string copyInline = "Copy me";

        [ClearButton]
        [CopyButton]
        public string clearAndCopy = "Two widgets";

        [Title("6. Callbacks", "#85DEA6", Foldout = true, DefaultExpanded = false)]
        [InfoBox("Edit these and watch the last callback field in section 11 update.")]
        [OnValueChanged(nameof(OnTrackedChanged))]
        public int trackedValue;

        [OnValueChanged(nameof(OnTrackedNameChanged))] public string trackedName = "Start";

        [OnArraySizeChanged(nameof(OnSlotsResized))] public List<string> slots = new();

        [OnArraySizeChanged(nameof(OnTagsResized))] public Material[] palettes = Array.Empty<Material>();

        [Title("7. References and serialization", "#83DBC6", Foldout = true, DefaultExpanded = false)]
        [InfoBox("Everything Unity cannot serialize on its own, plus the two inline editors.")]
        [Expandable] public ScriptableObject expandableCollapsed;

        [Expandable(DefaultExpanded = true)] public ScriptableObject expandableOpen;

        [SerializeReference] [ReferencePicker] public ISampleAbility singleAbility;

        [SerializeReference] [ReferencePicker] public List<ISampleAbility> abilityList = new();

        /// <summary>Restricted to objects implementing the interface, resolved from whatever is dropped.</summary>
        public InterfaceReference<ISampleAbility> interfaceTarget = new();

        /// <summary>The same reference constrained to Components as well, using the two-parameter form.</summary>
        public InterfaceReference<ISampleAbility, Component> interfaceComponent = new();

        /// <summary>A list of interface references, to check the drawer behaves per element.</summary>
        public List<InterfaceReference<ISampleAbility>> interfaceList = new();

        /// <summary>Serializes a Type, unconstrained, so the picker offers everything in the project.</summary>
        public TypeReference anyType = new();

        /// <summary>Narrowed by the generic argument, which a rename cannot break.</summary>
        public TypeReferenceOfBase<ISampleAbility> abilityType = new();

        /// <summary>Narrowed to a Unity type, to show the constraint is not limited to interfaces.</summary>
        public TypeReferenceOfBase<Collider> colliderType = new();

        /// <summary>A list of type references, since arrays take a different path through the renderer.</summary>
        public List<TypeReferenceOfBase<ISampleAbility>> abilityTypes = new();

        /// <summary>A scene by asset rather than by name, so a rename cannot break it.</summary>
        public SceneReference sceneAsset = new();

        /// <summary>A list of scene references, to check the drawer and its warning behave per element.</summary>
        public List<SceneReference> sceneAssets = new();

        /// <summary>Add two rows with the same key to see the duplicate warning.</summary>
        public SerializableDictionary<string, int> stringToInt = new();

        /// <summary>An enum key and an object value, so neither side is a primitive.</summary>
        public SerializableDictionary<ESampleMode, Material> modeToMaterial = new();

        /// <summary>A nested class as the value, so the row grows to fit it.</summary>
        public SerializableDictionary<string, ShowcaseNestedSettings> nestedValues = new();

        /// <summary>Add the same value twice to see the duplicate warning.</summary>
        public SerializableHashSet<string> uniqueStrings = new();

        /// <summary>A set of object references, which is where the two-step delete used to bite.</summary>
        public SerializableHashSet<Material> uniqueMaterials = new();

        /// <summary>Confirms the pipeline descends into a nested type and honors its attributes.</summary>
        public ShowcaseNestedSettings nested = new();

        /// <summary>A list of that nested class, since arrays take a different path.</summary>
        public List<ShowcaseNestedSettings> nestedList = new();

        [Title("8. Lists and tables", "#89D6E4", Foldout = true, DefaultExpanded = false)]
        [InfoBox("Without an attribute an array keeps Unity's own drawer, including its drag handles. "
            + "These two replace it entirely, which is why reordering becomes arrow buttons.")]
        public List<string> plainList = new();

        [ListDrawerSettings] public List<string> listDefault = new();

        [ListDrawerSettings(Searchable = true)] public List<string> listSearchable = new();

        [ListDrawerSettings(PageSize = 5)] public List<string> listPaged = new();

        [ListDrawerSettings(LabelMember = nameof(ShowcaseTableRow.id))]
        public List<ShowcaseTableRow> listLabeled = new();

        [ListDrawerSettings(Searchable = true, PageSize = 5, LabelMember = nameof(ShowcaseTableRow.id))]
        public List<ShowcaseTableRow> listFull = new();

        [ListDrawerSettings(ConfirmDelete = true, LabelMember = nameof(ShowcaseTableRow.id))]
        public List<ShowcaseTableRow> listConfirmDelete = new();

        [ListDrawerSettings(HideReorderButtons = true)] public List<string> listNoReorder = new();

        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        public List<string> listFixedSize = new();

        [ListDrawerSettings(DefaultExpanded = false)] public List<string> listCollapsed = new();

        [Table] public List<ShowcaseTableRow> tableDefault = new();

        [Table(ShowRowIndex = false)] public List<ShowcaseTableRow> tableNoIndex = new();

        [Table(HideAddButton = true, HideRemoveButton = true, DefaultExpanded = false)]
        public List<ShowcaseTableRow> tableFixedSize = new();

        [Table] public ShowcaseTableRow[] tableArray = Array.Empty<ShowcaseTableRow>();

        [Title("9. Size, toggles and widgets", "#A2CCFF", Foldout = true, DefaultExpanded = false)]
        [InfoBox("The size limits switch off the add and remove controls of the list and table drawers, "
            + "since a button that gets clamped back the moment you press it is worse than no button.")]
        [ArraySize(4)]
        public List<string> fixedFour = new();

        [ArraySize(Min = 2, Max = 6)] public List<string> boundedList = new();

        [ArraySize(3)]
        [ListDrawerSettings(LabelMember = nameof(ShowcaseTableRow.id))]
        public List<ShowcaseTableRow> fixedLabeledList = new();

        [ArraySize(3)]
        [Table]
        public ShowcaseTableRow[] fixedTable = Array.Empty<ShowcaseTableRow>();

        [StartExpanded] public ShowcaseNestedSettings expandedNested = new();

        [StartExpanded] public List<string> expandedList = new();

        /// <summary>Drives the field below it, and has no row of its own because of that.</summary>
        public bool useCustomRange;

        [PrefixToggle(nameof(useCustomRange))] public float customRange = 5f;

        /// <summary>Second toggle, for the object reference below.</summary>
        public bool useOverrideIcon;

        [PrefixToggle(nameof(useOverrideIcon))] public Texture2D overrideIcon;

        [LeftToggle] public bool leftToggle = true;

        [ResizableTextArea] public string growingText = "Type here and add lines.";

        [ResizableTextArea(1, 5)] public string shortText = "Small box.";

        [Rate] public int rating = 3;

        [Rate(1, 10)] public int difficulty = 7;

        [ColorPalette(nameof(BrandColors))] public Color brandColor = Color.white;

        [ColorPalette(nameof(BrandColors), AllowCustom = true)]
        public Color brandOrCustom = Color.white;

        [CustomContextMenu("Reset to default", nameof(ResetContextValue))]
        public float contextValue = DefaultContextValue;

        [CustomContextMenu("Scale/Halve", nameof(HalveContextValue))]
        [CustomContextMenu("Scale/Double", nameof(DoubleContextValue))]
        [CustomContextMenu("Reset to default", nameof(ResetContextValue))]
        public float multiContextValue = DefaultContextValue;

        [Title("10. Searching auto-getters", "#C0C3FF", Foldout = true, DefaultExpanded = false)]
        [InfoBox("These search the project, so they only run while the field is empty and their results "
            + "are cached. Clear one to watch it refill. The hierarchy getters are not here: an asset "
            + "has no GameObject for them to search.")]
        [GetScriptableObject]
        public ShowcaseConfigAsset config;

        [Expandable]
        [GetScriptableObject]
        public ShowcaseConfigAsset editableConfig;

        [GetPrefabWithComponent] public Rigidbody prefabBody;

        [GetPrefabWithComponent(typeof(Collider))] public GameObject colliderPrefab;

        [Title("$" + nameof(DynamicSectionTitle), "#D4BDFF", Foldout = true, DefaultExpanded = false)]
        [InfoBox("$" + nameof(DynamicInfo))]
        [Tooltip("The heading above and the box under it are both computed. A string argument starting "
            + "with a dollar names a member to read instead of carrying a literal.")]
        public string dynamicHeadingSubject = "look up";

        [Label("$" + nameof(DynamicLabel))]
        [Tooltip("The label of this field is computed too, so it can say what the value currently means "
            + "rather than repeating the field name.")]
        public int dynamicLabelValue = 3;

        [Label("Renamed in the inspector only")]
        [Tooltip("A plain literal label, for the fields whose good code name and good inspector name are "
            + "not the same word.")]
        public string literalLabelField = "Same field, different label";

        [Required("$" + nameof(DynamicRequiredMessage))]
        [Tooltip("Clear this and the error box names the asset it is missing from.")]
        public Material dynamicRequiredMessage;

        [Required(FixAction = nameof(UseFirstMaterial), FixActionName = "Use first")]
        [Tooltip("The error box carries a button that fills the field, for the failures whose answer is "
            + "always the same.")]
        public Material fixableMaterial;

        [ValidateInput(nameof(ValidateTexture))]
        [Tooltip("The validator returns a result rather than a bool, so it names which of its checks "
            + "failed and whether that is an error or only a warning.")]
        public Texture2D validatedTexture;

        [ValidateInput(nameof(IsPositive), FixAction = nameof(MakePositive), FixActionName = "Flip")]
        [Tooltip("A validator with a fix button of its own. Type a negative number.")]
        public float fixableNumber = 5f;

        [MinMaxSlider(0f, 50f)]
        [Tooltip("Drives the two sliders below it, so their range moves while you drag this.")]
        public Vector2 dynamicBounds = new(2f, 20f);

        [Slider(nameof(dynamicBounds))]
        [Tooltip("A slider taking both bounds from the Vector2 above.")]
        public float boundedBySibling = 10f;

        [Slider(0f, nameof(MaxSpeed), AutoClamp = true)]
        [Unit(UnitAttribute.MetersPerSecond)]
        [Tooltip("A constant lower bound and a member-driven upper one, clamped so the stored value "
            + "cannot sit outside the range even when something else wrote it.")]
        public float clampedSpeed = 4f;

        [Unit(UnitAttribute.Meter)]
        [Tooltip("A unit from the constant vocabulary rather than a literal.")]
        public float lengthInMeters = 2.5f;

        [Unit(UnitAttribute.Degree)]
        [Tooltip("Degrees of arc.")]
        public float angleInDegrees = 45f;

        [Unit("bananas")]
        [Tooltip("A free-text unit, which is the exception rather than the default.")]
        public float customUnit = 1f;

        [DisplayAsString]
        [Tooltip("Drawn as read-only text on one line, collapsing the whole collection instead of "
            + "keeping a foldout and a row per element.")]
        public string[] displayedCollection =
        {
            "hello",
            "world",
            "and",
            "so",
            "on"
        };

        [DisplayAsString(Separator = " | ")]
        [Tooltip("The same with a custom separator between elements.")]
        public List<ESampleMode> displayedModes = new();

        [InlineProperty]
        [Tooltip("A nested type drawn on this row instead of behind a foldout.")]
        public ShowcaseRange inlineRange = new();

        [InlineProperty(LabelWidth = 50f)]
        [Tooltip("The same nested type with wider child labels.")]
        public ShowcaseRange inlineWideLabels = new();

        [Tooltip("The same type without the attribute, for comparison. Three rows and a click.")]
        public ShowcaseRange foldedRange = new();

        [PreviewObject]
        [Tooltip("A preview big enough to judge rather than a thumbnail. Assign a mesh or a prefab and "
            + "drag inside the preview to turn it round.")]
        public GameObject previewedPrefab;

        [PreviewObject(64f, Width = 64f)]
        [Tooltip("The same preview at an explicit size, for a texture where turning it round means "
            + "nothing.")]
        public Texture2D previewedTexture;

        [PreviewObject(160f, Foldout = true, DefaultExpanded = false)]
        [Tooltip("A large preview behind a foldout, so it costs nothing until you open it.")]
        public Mesh previewedMesh;

        [AssetDropdown]
        [Tooltip("A searchable dropdown of every material in the project. The filter comes from the "
            + "field type when none is given.")]
        public Material dropdownMaterial;

        [AssetDropdown("t:Texture2D")]
        [Tooltip("The same with an explicit asset database filter.")]
        public Texture2D dropdownTexture;

        [Title("12. Order and layout", "#EBB6FF", Foldout = true, DefaultExpanded = false)]
        [InfoBox("The three fields at the bottom are declared last but drawn first, because their order "
            + "was given explicitly. Serialization order is still declaration order.")]
        [Horizontal("dimensions")]
        [Tooltip("First cell of a three-field row. The row ends where the group name changes.")]
        public float width = 1f;

        [Horizontal("dimensions")]
        [Tooltip("Second cell of the same row.")]
        public float height = 2f;

        [Horizontal("dimensions")]
        [Tooltip("Third cell of the same row.")]
        public float depth = 3f;

        [Horizontal("weighted", Weight = 3f)]
        [Tooltip("A row where the cells are not equal. This one takes three quarters of the width.")]
        public string weightedName = "Takes most of the row";

        [Horizontal("weighted", Weight = 1f)]
        [Tooltip("And this one the remaining quarter.")]
        public int weightedCount = 1;

        [Horizontal("unlabeled", Weight = 1f, ShowLabel = false)]
        [Tooltip("A cell that drops its label, giving the value the whole width.")]
        public float unlabeledFirst = 0.5f;

        [Horizontal("unlabeled", Weight = 1f, ShowLabel = false)]
        [Tooltip("The second half of that row.")]
        public float unlabeledSecond = 1.5f;

        [Tab("Loadout", "Collapsible tabs", Foldout = true, DefaultExpanded = true)]
        [Tooltip("A tab group under its own collapsible header. Click the bold heading above the tab "
            + "bar to fold the whole group away, tabs and all.")]
        public string collapsibleLoadout = "Primary";

        [Tab("Loadout", "Collapsible tabs")]
        [Tooltip("Second field of the Loadout tab.")]
        public int collapsibleAmmo = 30;

        [Tab("Perks", "Collapsible tabs")]
        [Tooltip("A second tab in the same collapsible group.")]
        public bool collapsiblePerkEnabled = true;

        [Tab("Notes", "Collapsible tabs")]
        [Tooltip("A third tab, so the bar has enough entries to be worth folding.")]
        public string collapsibleNote = "Anything worth remembering";

        [PropertyOrder(-10)]
        [Tooltip("Declared here, drawn at the top of the section. Lower comes first, and an unmarked "
            + "field counts as zero.")]
        public string pinnedToTop = "Declared last, drawn first";

        [PropertyOrder(10)]
        [Tooltip("Pushed to the bottom of the section instead.")]
        public string pushedToBottom = "Declared first, drawn last";

        /// <summary>Read-only property surfaced in the inspector by its attribute.</summary>
        [ShowNativeProperty] public int SlotCount => slots.Count;

        /// <summary>Second native property, to check several of them stack cleanly.</summary>
        [ShowNativeProperty] public string CurrentMode => mode.ToString();

        /// <summary>Bool property, to check the drawer picks the right field type.</summary>
        [ShowNativeProperty] public bool HasAnimator => animator != null;

        /// <summary>Drives the condition that reads a property rather than a field.</summary>
        public bool HasAdvancedSetup => isEnabled && isAdvanced;

        // Read by the heading of the dynamic section, which is what makes that heading say something the
        // reader could not already see from the fields under it.
        private string DynamicSectionTitle => $"11. Dynamic values ({dynamicHeadingSubject})";

        // Read by the info box under that heading.
        private string DynamicInfo => $"Mode {mode}, {slots.Count} slots.";

        // Read by the label of a field, so the label can say what the value currently means.
        private string DynamicLabel => dynamicLabelValue == 1
            ? "One item"
            : $"{dynamicLabelValue} items";

        // Read by a required message, so the box names the asset it is missing from.
        private string DynamicRequiredMessage => $"{name} has no material assigned.";

        // Upper bound of one of the sliders, to show a bound can be a property rather than a field.
        private float MaxSpeed => isAdvanced
            ? 20f
            : 8f;

        // Palette for the two color fields. Instance rather than static on purpose: the member resolver
        // only looks at instance members, so a static source would silently find nothing.
        private Color[] BrandColors => new[]
        {
            new Color(0.16f, 0.20f, 0.27f),
            new Color(0.20f, 0.60f, 0.86f),
            new Color(0.18f, 0.80f, 0.44f),
            new Color(0.95f, 0.77f, 0.06f),
            new Color(0.91f, 0.30f, 0.24f),
            new Color(0.61f, 0.35f, 0.71f)
        };

        // Options for the property-driven dropdown, instance for the same reason.
        private string[] PropertyOptions => new[]
        {
            "First",
            "Second",
            "Third"
        };

        // Long enough to cross the threshold where the plain popup becomes a searchable tree, and
        // slash-separated so the submenu grouping shows up too.
        private string[] SearchableOptions
        {
            get
            {
                string[] options = new string[LongOptionCount];
                for (int i = 0; i < LongOptionCount; i++)
                    options[i] = $"Group {i / OptionsPerGroup}/Entry {i:00}";

                return options;
            }
        }

        [Title("13. Native members", "#FFB2F5", Foldout = true, DefaultExpanded = false)]
        [InfoBox("Everything below this point is drawn after the serialized fields, in declaration "
            + "order. The header controls are the one thing missing: they live in the component header, "
            + "which an embedded inspector does not draw.")]
        [ShowNonSerialized]
        private const int RuntimeConstant = 42;

        [ShowNonSerialized] private string lastCallback = "No callback yet";

        [ShowNonSerialized] private Vector3 runtimeVector = Vector3.forward;

        /// <summary>Plain button with no label, which falls back to the method name.</summary>
        [Button]
        public void RebuildEverything() => Record(nameof(RebuildEverything));

        /// <summary>Button with an explicit label instead of the method name.</summary>
        [Button("Reset To Defaults")]
        public void ResetValues()
        {
            progressConstant = FullHealth;
            trackedValue = 0;
            Record(nameof(ResetValues));
        }

        /// <summary>Button that is only clickable while the editor is playing.</summary>
        [Button("Play Mode Only", Mode = EButtonMode.PlayMode)]
        public void RunDuringPlay() => Record(nameof(RunDuringPlay));

        /// <summary>Button that is only clickable while the editor is stopped.</summary>
        [Button("Edit Mode Only", Mode = EButtonMode.EditMode)]
        public void RunDuringEdit() => Record(nameof(RunDuringEdit));

        /// <summary>Button that asks for confirmation before it runs.</summary>
        [Button("Clear Everything", Confirm = "This clears every list on the asset. Continue?")]
        public void ClearEverything()
        {
            slots.Clear();
            uniqueDefault.Clear();
            uniqueWithMessage.Clear();
            Record(nameof(ClearEverything));
        }

        /// <summary>Header button, drawn in the title bar rather than here.</summary>
        [HeaderButton("Ping")]
        public void Ping() => Record(nameof(Ping));

        /// <summary>Header label, read from a property.</summary>
        [HeaderLabel(Width = 90f)]
        public string HeaderState() => $"{mode} x{slots.Count}";

        private void ResetContextValue()
        {
            contextValue = DefaultContextValue;
            multiContextValue = DefaultContextValue;
            Record(nameof(ResetContextValue));
        }

        private void HalveContextValue()
            => Record($"{nameof(HalveContextValue)} to {multiContextValue *= 0.5f}");

        private void DoubleContextValue()
            => Record($"{nameof(DoubleContextValue)} to {multiContextValue *= 2f}");

        private void UseFirstMaterial()
        {
            fixableMaterial = material;
            Record(nameof(UseFirstMaterial));
        }

        private void MakePositive()
        {
            fixableNumber = Mathf.Abs(fixableNumber);
            Record(nameof(MakePositive));
        }

        // Returns a result rather than a bool, so it can name which check failed and choose between an
        // error and a warning.
        private ValidationResult ValidateTexture()
        {
            if (validatedTexture == null)
                return ValidationResult.Error("No texture assigned.");

            if (!validatedTexture.isReadable)
                return ValidationResult.Warning("The texture is not readable, so it cannot be sampled.");

            return ValidationResult.Valid;
        }

        private ValidationResult IsPositive() => fixableNumber >= 0f
            ? ValidationResult.Valid
            : ValidationResult.Error("The value has to be positive.");

        private bool IsEven(int value) => value % 2 == 0;

        private bool IsFast() => mode == ESampleMode.Fast;

        private bool HasMaterial() => material != null;

        private int[] GetMethodOptions() => new[]
        {
            10,
            20,
            30
        };

        private void Roll()
        {
            inlineButtonLabeled = UnityEngine.Random.Range(MinimumRoll, MaximumRoll);
            Record($"{nameof(Roll)} rolled {inlineButtonLabeled}");
        }

        private void OnTrackedChanged() => Record($"{nameof(trackedValue)} is now {trackedValue}");

        private void OnTrackedNameChanged(string value) => Record($"{nameof(trackedName)} is '{value}'");

        private void OnSlotsResized(int size) => Record($"{nameof(slots)} resized to {size}");

        private void OnTagsResized() => Record($"{nameof(palettes)} changed size");

        // The showcase writes to a field rather than to the console, because this asset lives inside an
        // editor window and a tab that logs every time it is poked would be noise, not information.
        private void Record(string message) => lastCallback = message;
    }
}