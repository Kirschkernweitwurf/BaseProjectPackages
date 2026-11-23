using Base.AttributesPackage;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.TextPackage
{
    /// <summary>
    /// Widens a box of fixed width, like the tooltip panel, as far as the player's text size, readable font and wider
    /// spacing widen its text, so its lines keep about as many words as designed and a long word still fits. The width
    /// depends on the reading settings only, never on the text, so every box of the kind keeps one width and none jumps
    /// from one text to the next. A text that only wraps once it reaches a width, like a tooltip text, keeps its own
    /// width below that and wraps at the grown one. A chat bubble also ends at its longest line once it wraps. It only
    /// tells the layout how wide the box wants to be, above a Layout Element, which keeps the designed width for the
    /// editor.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class WidenWithTextSize : UIBehaviour, ILayoutElement
    {
        private const float MaxGrowth = 1.5f;
        private const float NoPreference = -1f;

        // A box exactly as wide as its measured text can still wrap its last letter, so a short text gets a bit more.
        private const float ShortTextSlack = 0.1f;

        // Above a Layout Element's priority of 1, so the grown width wins over the designed one.
        private const int Priority = 2;

        [Tooltip("The text whose size the width follows, like the body of a tooltip.")]
        [Required] [SerializeField] private LocalizedTextBox text;

        [Min(0f)]
        [Tooltip("The width in the designed look. It grows by as much as a line of the text does.")]
        [SerializeField] private float designedWidth;

        [Min(0f)]
        [Tooltip("Optional: the widest the box gets, however much its text grows, like the room a chat bubble has "
            + "before it reaches the other side. 0 lets it grow as far as the text does.")]
        [SerializeField] private float maxWidth;

        [Tooltip("Optional: a text that keeps its own width while it is shorter and wraps at the grown width, like a "
            + "tooltip text. Empty gives the box the grown width always.")]
        [SerializeField] private TMP_Text shortText;

        [Tooltip("Ends the box at the short text's longest line once it wraps, like a chat bubble. Off keeps a wrapped "
            + "text at the grown width, like a tooltip. Pair it with the Fill line fit, since even lines leave room.")]
        [SerializeField] private bool isFitToLongestLine;

        private float _width;

        /// <inheritdoc/>
        public float minWidth => _width;

        /// <inheritdoc/>
        public float preferredWidth => _width;

        /// <inheritdoc/>
        public float flexibleWidth => NoPreference;

        /// <inheritdoc/>
        public float minHeight => NoPreference;

        /// <inheritdoc/>
        public float preferredHeight => NoPreference;

        /// <inheritdoc/>
        public float flexibleHeight => NoPreference;

        /// <inheritdoc/>
        public int layoutPriority => Priority;

#region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            MarkDirty();
        }

        protected override void OnDisable()
        {
            MarkDirty();
            base.OnDisable();
        }
#endregion

        /// <inheritdoc/>
        public void CalculateLayoutInputHorizontal()
        {
            // Capped, so a tooltip at the biggest reading settings grows taller instead of covering half the screen.
            _width = designedWidth * Mathf.Min(text.TextWidthGrowth, MaxGrowth);

            if (maxWidth > 0f)
                _width = Mathf.Min(_width, maxWidth);

            if (shortText != null)
                _width = Mathf.Min(_width, GetShortTextWidth());
        }

        /// <inheritdoc/>
        public void CalculateLayoutInputVertical()
        {
        }

        // The width the text takes on one line, like the tooltip panel measures it before it caps it. Fit to its
        // longest line, it wraps at the box's width first and TextMeshPro measures its widest line.
        private float GetShortTextWidth()
        {
            float wrapWidth = isFitToLongestLine
                ? _width
                : float.PositiveInfinity;
            return shortText.GetPreferredValues(shortText.text, wrapWidth, float.PositiveInfinity).x
                + shortText.fontSize * ShortTextSlack;
        }

        private void MarkDirty() => LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
    }
}