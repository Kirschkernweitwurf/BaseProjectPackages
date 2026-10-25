using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.AudioPackage.OnEvent
{
    /// <summary>
    /// Plays an <see cref="AudioContainer"/> sound when the UI element is hovered over.
    /// </summary>
    public class PlayAudioOnHover : PlayAudioOnEvent, IPointerEnterHandler
    {
        [Required]
        [SerializeField] private AudioContainer hoverSound;

        /// <summary>Plays the configured sound when the element is pointed at.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event matters.</param>
        public void OnPointerEnter(PointerEventData eventData) => Play(hoverSound);
    }
}