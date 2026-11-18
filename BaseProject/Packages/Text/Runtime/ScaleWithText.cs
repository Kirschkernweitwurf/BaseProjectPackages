using Base.AttributesPackage;
using UnityEngine;

namespace Base.TextPackage
{
    /// <summary>
    /// Grows icons that sit beside a text, like the signal, wifi and battery icons next to the clock of a status bar,
    /// as far as that text really grows on screen with the player's text size, so they keep their size to it. A text
    /// in a fixed box that can't grow keeps its icons at their size too, like a clock in a fixed slot; following the
    /// size the text asked for pushed them oddly far out there. The Xbox Accessibility Guidelines ask icons to scale
    /// with text. It scales around its pivot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScaleWithText : MonoBehaviour
    {
        [Tooltip("The text the icons sit beside, like the clock.")]
        [Required] [SerializeField] private LocalizedTextBox text;

        private Vector3 _designedScale;

#region Unity Callbacks
        private void Awake() => _designedScale = transform.localScale;

        private void OnEnable()
        {
            text.Fitted += Resize;
            Resize();
        }

        private void OnDisable() => text.Fitted -= Resize;
#endregion

        private void Resize() => transform.localScale = _designedScale * text.ShownSizeGrowth;
    }
}