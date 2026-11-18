using UnityEngine;

namespace Base.TextPackage
{
    /// <summary>
    /// Zooms a whole part, like a dock of buttons on a main menu, as far as the player's text size grows text, up to
    /// the room the part has. The part keeps its designed layout and just gets bigger, so nothing inside has to shrink,
    /// wrap or cut. It scales around its pivot, so a part pinned to an edge keeps that edge. For a part on a canvas of
    /// its own, <see cref="ScaleCanvasWithTextSize"/> zooms the whole canvas instead.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScaleWithTextSize : MonoBehaviour, ITextZoom
    {
        private const float DesignedZoom = 1f;

        [Min(DesignedZoom)]
        [Tooltip("The largest zoom, the room the part has on screen. At 1.5 it follows the largest text size.")]
        [SerializeField] private float maxZoom = ReadableText.LargestScale;

        private Vector3 _designedScale;

        /// <inheritdoc/>
        public float Zoom => Mathf.Clamp(ReadableText.Scale, DesignedZoom, maxZoom);

#region Unity Callbacks
        private void Awake() => _designedScale = transform.localScale;

        private void OnEnable()
        {
            ReadableText.Changed += Resize;
            Resize();
        }

        private void OnDisable() => ReadableText.Changed -= Resize;
#endregion

        private void Resize() => transform.localScale = _designedScale * Zoom;
    }
}