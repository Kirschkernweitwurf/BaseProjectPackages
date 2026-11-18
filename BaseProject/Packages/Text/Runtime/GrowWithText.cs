using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.TextPackage
{
    /// <summary>
    /// Gives a row in a layout group, like a settings row, the height its text needs: never less than designed, more
    /// when a wider font, wider spacing or a bigger text size make the text take more lines. A panel grows this way
    /// instead of letting text run into the next row. It moves nothing inside the row; it only tells the layout how
    /// tall the row wants to be and passes text changes on so the panel measures again.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class GrowWithText : UIBehaviour, ILayoutElement, ILayoutGroup
    {
        private const float DesignedHeight = 124f;
        private const float DesignedPadding = 20f;
        private const float NoPreference = -1f;

        // Above a Layout Element's priority of 1, so the height the text needs wins over a fixed one.
        private const int Priority = 2;

        [Tooltip("The part that holds the text, with a layout group that knows its preferred height, like Texts.")]
        [Required] [SerializeField] private RectTransform content;

        [Min(0f)]
        [Tooltip("The height of the row as designed. It never gets smaller than this.")]
        [SerializeField] private float designedHeight = DesignedHeight;

        [Min(0f)]
        [Tooltip("Room above and below the text together, so the text keeps its margin when the row grows.")]
        [SerializeField] private float padding = DesignedPadding;

        private float _height = DesignedHeight;

        /// <inheritdoc/>
        public float minWidth => NoPreference;

        /// <inheritdoc/>
        public float preferredWidth => NoPreference;

        /// <inheritdoc/>
        public float flexibleWidth => NoPreference;

        /// <inheritdoc/>
        public float minHeight => _height;

        /// <inheritdoc/>
        public float preferredHeight => _height;

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
        }

        /// <inheritdoc/>
        public void CalculateLayoutInputVertical() => _height = Mathf.Max(designedHeight,
            LayoutUtility.GetPreferredHeight(content) + padding);

        /// <inheritdoc/>
        public void SetLayoutHorizontal()
        {
        }

        /// <inheritdoc/>
        public void SetLayoutVertical()
        {
        }

        private void MarkDirty() => LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
    }
}