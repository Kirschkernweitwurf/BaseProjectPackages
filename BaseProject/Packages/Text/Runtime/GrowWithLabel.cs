using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.TextPackage
{
    /// <summary>
    /// Gives a button or chip the width its label needs: never narrower than designed, wider when a bigger font, wider
    /// spacing or a bigger text size make the label wider, so the label keeps the size the player picked instead of
    /// shrinking. The button's own layout group measures label and padding together; this only lets that width win
    /// over the button's Layout Element, which keeps the designed width.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
    public sealed class GrowWithLabel : UIBehaviour, ILayoutElement
    {
        private const float NoPreference = -1f;

        // Above a Layout Element's priority of 1, so the width the label needs wins over a fixed one.
        private const int Priority = 2;

        [Tooltip("The button's own layout group, which knows how wide label and padding are together. Auto-assigned "
            + "from the same GameObject.")]
        [Required] [GetComponent] [SerializeField] private HorizontalOrVerticalLayoutGroup content;

        [Min(0f)]
        [Tooltip("The width as designed. The button never gets narrower than this.")]
        [SerializeField] private float designedWidth;

        /// <inheritdoc/>
        public float minWidth => GetWidth();

        /// <inheritdoc/>
        public float preferredWidth => GetWidth();

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
        }

        /// <inheritdoc/>
        public void CalculateLayoutInputVertical()
        {
        }

        // Read when the parent asks, after the layout group measured its children this pass.
        private float GetWidth() => Mathf.Max(designedWidth, content.preferredWidth);

        private void MarkDirty() => LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
    }
}