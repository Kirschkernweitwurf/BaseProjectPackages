using Base.AttributesPackage;
using Base.ServicesPackage;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.CorePackage.Audio.OnEvent
{
    /// <summary>
    /// Plays an <see cref="AudioContainer"/> sound when the UI element is selected.
    /// </summary>
    public class PlayAudioOnSelect : MonoBehaviour, ISelectHandler
    {
        [Required]
        [SerializeField] private AudioContainer selectSound;

        private AudioManager _audioManager;

#region Unity Callbacks
        private void Start() => ServiceLocator.TryGet(out _audioManager);
#endregion

        /// <summary>Plays the configured sound when the element is selected.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event matters.</param>
        public void OnSelect(BaseEventData eventData) => _audioManager.PlaySound(selectSound);
    }
}