using Base.AttributesPackage;
using Base.ServicesPackage;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.AudioPackage.OnEvent
{
    /// <summary>
    /// Plays an <see cref="AudioContainer"/> sound when the UI element is clicked.
    /// </summary>
    public class PlayAudioOnClick : MonoBehaviour, IPointerClickHandler
    {
        [Required]
        [SerializeField] private AudioContainer clickSound;

        private AudioManager _audioManager;

#region Unity Callbacks
        private void Start() => ServiceLocator.TryGet(out _audioManager);
#endregion

        /// <summary>Plays the configured sound when the element is clicked.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event matters.</param>
        public void OnPointerClick(PointerEventData eventData) => _audioManager.PlaySound(clickSound);
    }
}