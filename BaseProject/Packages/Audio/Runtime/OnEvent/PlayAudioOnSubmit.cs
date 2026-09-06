using Base.AttributesPackage;
using Base.ServicesPackage;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.AudioPackage.OnEvent
{
    /// <summary>
    /// Plays an <see cref="AudioContainer"/> sound when the UI element is submitted (e.g., when a button is pressed).
    /// </summary>
    public class PlayAudioOnSubmit : MonoBehaviour, ISubmitHandler
    {
        [Required]
        [SerializeField] private AudioContainer submitSound;

        private AudioManager _audioManager;

#region Unity Callbacks
        private void Start() => ServiceLocator.TryGet(out _audioManager);
#endregion

        /// <summary>Plays the configured sound when the element is submitted.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event matters.</param>
        public void OnSubmit(BaseEventData eventData) => _audioManager.PlaySound(submitSound);
    }
}