using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.AudioPackage.OnEvent
{
    /// <summary>
    /// Plays an <see cref="AudioContainer"/> sound when the UI element is selected by keyboard or gamepad
    /// navigation, the counterpart of <see cref="PlayAudioOnHover"/>.
    /// </summary>
    public class PlayAudioOnSelect : PlayAudioOnEvent, ISelectHandler
    {
        [Required]
        [SerializeField] private AudioContainer selectSound;

        /// <summary>
        /// Plays the configured sound when the element is selected. A selection by the mouse is skipped: it comes
        /// with a hover or a click that has its own sound, so playing here as well would double it.
        /// </summary>
        /// <param name="eventData">The event system payload. A <see cref="PointerEventData"/> marks a mouse
        /// selection.</param>
        public void OnSelect(BaseEventData eventData)
        {
            if (eventData is PointerEventData)
                return;

            Play(selectSound);
        }
    }
}