using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.TextPackage
{
    /// <summary>
    /// Zooms a whole screen canvas, like a HUD, as far as the player's text size grows text, up to the room the
    /// screen has. It shrinks the canvas's reference resolution, so every part grows together: parts pinned to a corner
    /// keep their corner and what hangs under another part, like a list under a bar, keeps its
    /// gap. Texts on the canvas keep their designed size within it.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class ScaleCanvasWithTextSize : MonoBehaviour, ITextZoom
    {
        private const float DesignedZoom = 1f;

        [Required] [GetComponent]
        [Tooltip("The scaler whose reference resolution shrinks. Auto-assigned from the same GameObject.")]
        [SerializeField] private CanvasScaler scaler;

        [Min(DesignedZoom)]
        [Tooltip("The largest zoom, the room the screen has. Parts in opposite corners must still not meet at it.")]
        [SerializeField] private float maxZoom = 1.25f;

        private Vector2 _designedResolution;

        /// <inheritdoc/>
        public float Zoom => Mathf.Clamp(ReadableText.Scale, DesignedZoom, maxZoom);

#region Unity Callbacks
        private void Awake() => _designedResolution = scaler.referenceResolution;

        private void OnEnable()
        {
            ReadableText.Changed += Resize;
            Resize();
        }

        private void OnDisable() => ReadableText.Changed -= Resize;
#endregion

        private void Resize() => scaler.referenceResolution = _designedResolution / Zoom;
    }
}