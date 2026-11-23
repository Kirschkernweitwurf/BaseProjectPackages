using System;
using System.Collections.Generic;
using Base.AttributesPackage;
using Base.UtilityPackage.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace Base.TextPackage
{
    /// <summary>
    /// Gives a text box the style of its kind for the current language: font, material, size and line spacing in
    /// one step, so no frame shows the new font at the old size. Auto sizing is off unless the style allows it;
    /// every box of a kind has the same size and the text fit check makes sure translations fit. A style with more
    /// than one line evens out its lines through a <see cref="LineBalancer"/>, the way <see cref="ELineFit"/> asks.
    /// A readable font and extra spacing the player picked (<see cref="ReadableText"/>) go on top of the style.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedTextBox : MonoBehaviour
    {
        private const string EmptyStyleText = "Tried to apply an empty text style.";
        private const float Invisible = 0f;
        private const float NoScale = 1f;
        private const string OverflowFormat = "{0} overflows its box in {1}: '{2}'.";

        // Panel texts with more lines than this are paragraphs, which get the paragraph spacing.
        private const int ParagraphMinLines = 2;
        private const float SpacingUnit = 0.01f;

        // Common lowercase letters, so a font's width is judged by running text, not by one odd glyph.
        private const string WidthSample = "aeinorst";

        [GetComponent] [SerializeField] private TMP_Text text;

        [Tooltip("The style of this box, one per language, from the Text Styles table.")]
        [SerializeField] private LocalizedTextStyle style = new();

        [Tooltip("This box limits its lines on purpose, so a longer text ends in ... without a warning, whatever the "
            + "style says. For a short look at a text that can be read in full elsewhere.")]
        [SerializeField] private bool canCutLongText;

        [Tooltip("How a text of several lines uses the box. Balance evens out the lines, for short texts like cards. "
            + "Pretty keeps them full and only fixes a short last line, for paragraphs in a box of fixed width. "
            + "Fill leaves the lines as they break, each full before the next starts, for chat bubbles.")]
        [SerializeField] private ELineFit lineFit;

        [Tooltip("Lets long words break at the end of a line, with a hyphen, in languages that have patterns for it.")]
        [SerializeField] private bool isHyphenated;

        [HideInInspector]
        [SerializeField] private LineBalancer balancer = new();

        // The box's own spacing and wrapping before any style changed them. Serialized, so a copy of a live box, like
        // a tooltip panel cloned for a second entry, keeps the designed values instead of taking the grown ones.
        [HideInInspector]
        [SerializeField] private bool hasDesignedLayout;

        [HideInInspector]
        [SerializeField] private float designedCharacterSpacing;

        [HideInInspector]
        [SerializeField] private float designedWordSpacing;

        [HideInInspector]
        [SerializeField] private float designedParagraphSpacing;

        [HideInInspector]
        [SerializeField] private TextWrappingModes designedWrapping;

        private TextStyle _appliedStyle;
        private float _requestedSize;
        private float _readableSize;
        private float _designedSize;
        private float _largeViewScale = NoScale;
        private readonly List<CanvasGroup> _fades = new();
        private TextSizeGroup _group;
        private ITextZoom _zoom;
        private string _groupedText;
        private bool _isBalanced;
        private bool _isLayoutDirty;

        /// <summary>
        /// Raised after the text was rendered and fitted into its box, so its shown size is known: after a new style,
        /// new reading settings or a new text.
        /// </summary>
        public event Action Fitted;

        /// <summary>The Text Styles entry of this box, for editor tools that pick the style themselves.</summary>
        public TableEntryReference StyleEntry => style.TableEntryReference;

        /// <summary>The style applied last or null before the first. Groups pair their boxes by it.</summary>
        public TextStyle AppliedStyle => _appliedStyle;

        /// <summary>
        /// True when this box limits its lines on purpose and ends a longer text in "…", as a short look at a text that
        /// can be read in full elsewhere.
        /// </summary>
        public bool CanCutLongText => canCutLongText;

        /// <summary>True when long words may break at the end of a line, with a hyphen.</summary>
        public bool IsHyphenated => isHyphenated;

        /// <summary>
        /// How much bigger the box would have to be shown to bring its text back to the size the player picked: 1
        /// when nothing shrank. A card's large view grows by it.
        /// </summary>
        public float LargeViewScale => text.fontSize > 0f
            ? Mathf.Max(NoScale, _requestedSize / text.fontSize)
            : NoScale;

        /// <summary>
        /// How much bigger the text shows than designed, after it was fitted into its box: the player's text size as
        /// far as the box lets it grow, 1 at 100 %. Icons beside the text, like a status bar's next to its clock,
        /// follow it, so they keep their size to the text even where a fixed box keeps it small.
        /// </summary>
        public float ShownSizeGrowth => _designedSize > 0f
            ? text.fontSize / _designedSize
            : NoScale;

        /// <summary>
        /// How much bigger the box would have to be shown to bring its text back to its readable minimum: 1 when the
        /// text is at least that big on screen. A card's text panel shows only when its hover cannot cover this.
        /// </summary>
        public float ReadableViewScale => text.fontSize > 0f
            ? Mathf.Max(NoScale, _readableSize / (text.fontSize * ZoomOf()))
            : NoScale;

        /// <summary>
        /// How much wider a line of this box's text runs than designed: the player's text size, readable font and wider
        /// spacing together, measured on common letters; 1 in the designed look. A box that widens with its text, like
        /// a tooltip panel, follows it, so its lines keep about as many words as designed.
        /// </summary>
        public float TextWidthGrowth => _appliedStyle == null
            ? NoScale
            : WidthOf(text.font, _requestedSize, text.characterSpacing)
                / WidthOf(_appliedStyle.Font, _appliedStyle.FontSize, designedCharacterSpacing);

#region Unity Callbacks
        // Waiting for the asset keeps the switch to a new language in one frame.
        private void Awake()
        {
            style.WaitForCompletion = true;

            if (isHyphenated)
                text.textPreprocessor = TextHyphenator.Shared;
        }

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            style.AssetChanged += Apply;
            ReadableText.Changed += OnReadableTextChanged;
            FindGroup();
            FindFades();
        }

        // Pooled cards move between hands and menus, so the group they share a size with can change.
        private void OnTransformParentChanged()
        {
            FindGroup();
            FindFades();
        }

        private void LateUpdate()
        {
            if (!_isLayoutDirty)
                return;

            _isLayoutDirty = false;
            WarnIfOverflowing();
        }

        private void OnDisable()
        {
            ReadableText.Changed -= OnReadableTextChanged;
            style.AssetChanged -= Apply;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }
#endregion

        /// <summary>Applies <paramref name="textStyle"/> at once, also for the text fit check.</summary>
        public void Apply(TextStyle textStyle)
        {
            if (textStyle == null)
            {
                CustomLogger.LogWarning(EmptyStyleText, this);
                return;
            }

            _appliedStyle = textStyle;
            KeepDesignedLayout();
            TextSpacing spacing = SpacingFor(textStyle);
            ApplyFont(textStyle, spacing);
            ApplySpacing(spacing);

            // A frame keeps its lines: a one-line frame shrinks instead of breaking a word onto a second line.
            text.textWrappingMode = textStyle.IsFrame && textStyle.MaxLines == 1
                ? TextWrappingModes.NoWrap
                : designedWrapping;

            // Cutting is a last resort for short labels; longer texts must fit, the text fit check makes sure.
            text.overflowMode = textStyle.CanCutWithEllipsis || canCutLongText
                ? TextOverflowModes.Ellipsis
                : TextOverflowModes.Overflow;

            // A single line has nothing to even out, so most labels skip the extra layout passes. A box that cuts its
            // text wants every word it can fit and even lines would fit fewer. Fill wants the lines as they break.
            _isBalanced = textStyle.MaxLines > 1
                && !canCutLongText
                && lineFit != ELineFit.Fill;
            balancer.Invalidate();
            MarkGroupDirty();
        }

        /// <summary>
        /// Fits the text with its own style alone, as if it had no group and returns the size it settled on.
        /// </summary>
        public float FitAlone()
        {
            if (_appliedStyle == null)
                return text.fontSize;

            ApplyFont(_appliedStyle, SpacingFor(_appliedStyle));
            text.ForceMeshUpdate();
            return text.fontSize;
        }

        /// <summary>
        /// Tells the box that it is always shown bigger on hover or focus, by up to <paramref name="scale"/>, like a
        /// card in the hand. A frame may then shrink below its readable minimum, by that much, since the large view
        /// brings it back. 1 means no large view.
        /// </summary>
        public void SetLargeView(float scale)
        {
            float clamped = Mathf.Max(NoScale, scale);
            if (Mathf.Approximately(clamped, _largeViewScale))
                return;

            _largeViewScale = clamped;
            if (_appliedStyle != null)
                Apply(_appliedStyle);
        }

        /// <summary>Shows the text at <paramref name="size"/>, the size its group shares.</summary>
        public void LockSize(float size)
        {
            text.enableAutoSizing = false;
            text.fontSize = size;
        }

        // Wider spacing helps reading running text, where letters crowd; short texts and frames keep their designed
        // spacing, so they keep their box.
        private static TextSpacing SpacingFor(TextStyle textStyle) => !textStyle.IsFrame
            && textStyle.MaxLines > ParagraphMinLines
                ? ReadableText.Spacing
                : TextSpacing.None;

        // The width the sample letters take in font at size, spacing included. A letter the font lacks counts as none.
        private static float WidthOf(TMP_FontAsset font, float size, float characterSpacing)
        {
            float advance = 0f;

            foreach (char letter in WidthSample)
            {
                if (font.characterLookupTable.TryGetValue(letter, out TMP_Character character)
                    || (font.TryAddCharacters(letter.ToString())
                        && font.characterLookupTable.TryGetValue(letter, out character)))
                    advance += character.glyph.metrics.horizontalAdvance * character.glyph.scale;
            }

            float emScale = size / font.faceInfo.pointSize * font.faceInfo.scale;

            return advance * emScale + characterSpacing * SpacingUnit * size * WidthSample.Length;
        }

        // A readable font the player picked takes the designed font's place, scaled to the same letter height.
        private void ApplyFont(TextStyle textStyle, TextSpacing spacing)
        {
            TMP_FontAsset font = textStyle.Font;
            Material material = textStyle.Material;
            float sizeFactor = NoScale;
            float lineSpacing = textStyle.LineSpacing;
            bool isSwapped = ReadableText.TryGetSwap(font, material, out ReadableFontSwap swap);

            if (isSwapped)
            {
                font = swap.ReadableFont;
                material = font.material;
                sizeFactor = swap.SizeFactor;
                lineSpacing = textStyle.LineSpacing / sizeFactor + swap.LineSpacingOffset;
            }

            // A part of the screen that zooms with the text size carries that much of the growth: the text keeps its
            // designed size within it and may shrink as far as the zoom grows it back on screen.
            float zoom = ZoomOf();
            float scaled = ReadableText.GetScaledSize(textStyle.FontSize, textStyle.MinReadableSize,
                ReadableText.Scale / zoom);
            float size = scaled * sizeFactor;
            _designedSize = textStyle.FontSize * sizeFactor;
            _requestedSize = size;
            _readableSize = textStyle.MinReadableSize * sizeFactor;
            text.font = font;
            text.fontSharedMaterial = material;
            text.fontSize = size;
            text.lineSpacing = lineSpacing + spacing.Lines;

            // A frame keeps its box, so text the player made bigger shrinks back into it: never below readable or
            // below it only as far as its large view brings it back.
            float floor = textStyle.IsFrame && (isSwapped || ReadableText.IsEnlarged)
                ? textStyle.MinReadableSize * sizeFactor / (_largeViewScale * zoom)
                : textStyle.AutoSizeMin * sizeFactor;
            text.enableAutoSizing = floor > 0f;

            if (text.enableAutoSizing)
                TextStyle.SetAutoSizeRange(text, floor, size);
        }

        // The box's own spacing and wrapping, like a title's tighter letters, are kept before any style changes them.
        private void KeepDesignedLayout()
        {
            if (hasDesignedLayout)
                return;

            designedCharacterSpacing = text.characterSpacing;
            designedWordSpacing = text.wordSpacing;
            designedParagraphSpacing = text.paragraphSpacing;
            designedWrapping = text.textWrappingMode;
            hasDesignedLayout = true;
        }

        private void ApplySpacing(TextSpacing spacing)
        {
            text.characterSpacing = designedCharacterSpacing + spacing.Characters;
            text.wordSpacing = designedWordSpacing + spacing.Words;
            text.paragraphSpacing = designedParagraphSpacing + spacing.Paragraphs;
        }

        private void OnReadableTextChanged()
        {
            if (_appliedStyle == null)
                return;

            Apply(_appliedStyle);
        }

        private float ZoomOf() => _zoom == null
            ? NoScale
            : _zoom.Zoom;

        private void FindGroup()
        {
            _group = GetComponentInParent<TextSizeGroup>();
            _zoom = GetComponentInParent<ITextZoom>(true);
        }

        private void MarkGroupDirty()
        {
            if (_group == null)
                return;

            _groupedText = text.text;
            _group.MarkDirty();
        }

        private void OnTextChanged(Object changed)
        {
            if (changed != text)
                return;

            // Only a new text needs the group to compare again; a new size alone comes from the group itself.
            if (_group != null
                && text.text != _groupedText)
                MarkGroupDirty();

            if (_isBalanced)
                balancer.Balance(text, lineFit);

            if (Debug.isDebugBuild)
                _isLayoutDirty = true;

            Fitted?.Invoke();
        }

        private void WarnIfOverflowing()
        {
            if (canCutLongText
                || IsHidden()
                || !IsOverflowingNow())
                return;

            CustomLogger.LogWarning(string.Format(OverflowFormat, name, LocalizationSettings.SelectedLocale,
                text.text), this);
        }

        // The last mesh can be older than the box: a text shown for the first time is built before its layout group
        // sized the box, which prefabs save as zero. Only a text that seems to overflow is built again, so the check
        // stays cheap. The rebuild reports a changed text, which must not start another check.
        private bool IsOverflowingNow()
        {
            if (!TextOverflow.IsOverflowing(text))
                return false;

            text.ForceMeshUpdate();
            _isLayoutDirty = false;
            return TextOverflow.IsOverflowing(text);
        }

        // The overflow check asks the fades above the text, since a renderer learns of a fade only after it ran.
        private void FindFades()
        {
            if (Debug.isDebugBuild)
                GetComponentsInParent(true, _fades);
        }

        // A text nobody can see, like a header squeezed while it grows in faded out, shows no cut.
        private bool IsHidden()
        {
            foreach (CanvasGroup fade in _fades)
            {
                if (fade == null)
                    continue;

                if (fade.alpha <= Invisible)
                    return true;

                if (fade.ignoreParentGroups)
                    return false;
            }

            return false;
        }
    }
}