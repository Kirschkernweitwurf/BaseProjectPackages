using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.TextPackage
{
    /// <summary>
    /// Zooms a grid of entries, like the cards of a deck menu, as far as the player's text size grows text, up to a
    /// limit. The whole grid is shown bigger, so every entry keeps its designed layout and its own scale animations,
    /// like a card's hover swell; a grid with a fixed number of columns shows fewer of them, so a row keeps about its
    /// designed width and the grid scrolls further down instead.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class ScaleGridWithTextSize : MonoBehaviour, ITextZoom
    {
        private const float DesignedZoom = 1f;

        [Required] [GetComponent]
        [Tooltip("The grid that zooms. Auto-assigned from the same GameObject.")]
        [SerializeField] private GridLayoutGroup grid;

        [Min(DesignedZoom)]
        [Tooltip("The largest zoom. At 1.5 it follows the largest text size.")]
        [SerializeField] private float maxZoom = ReadableText.LargestScale;

        private Vector3 _designedScale;
        private int _designedColumns;

        /// <inheritdoc/>
        public float Zoom => Mathf.Clamp(ReadableText.Scale, DesignedZoom, maxZoom);

#region Unity Callbacks
        private void Awake()
        {
            _designedScale = transform.localScale;
            _designedColumns = grid.constraintCount;
        }

        private void OnEnable()
        {
            ReadableText.Changed += Resize;
            Resize();
        }

        private void OnDisable() => ReadableText.Changed -= Resize;
#endregion

        private void Resize()
        {
            float zoom = Zoom;
            transform.localScale = _designedScale * zoom;

            if (grid.constraint != GridLayoutGroup.Constraint.Flexible)
                grid.constraintCount = Mathf.Max(1, Mathf.FloorToInt(_designedColumns / zoom));
        }
    }
}