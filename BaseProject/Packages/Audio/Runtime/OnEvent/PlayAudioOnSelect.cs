using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.AudioPackage.OnEvent
{
    /// <summary>
    /// Plays an <see cref="AudioContainer"/> sound when the UI element is selected.
    /// </summary>
    public class PlayAudioOnSelect : PlayAudioOnEvent, ISelectHandler
    {
        [Required]
        [SerializeField] private AudioContainer selectSound;

        /// <summary>Plays the configured sound when the element is selected.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event matters.</param>
        public void OnSelect(BaseEventData eventData) => Play(selectSound);
    }
}