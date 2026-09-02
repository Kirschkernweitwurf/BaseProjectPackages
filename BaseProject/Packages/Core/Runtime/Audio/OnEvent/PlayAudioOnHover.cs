using Base.AttributesPackage;
using Base.ServicesPackage;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.CorePackage.Audio.OnEvent
{
    /// <summary>
    /// Plays an <see cref="AudioContainer"/> sound when the UI element is hovered over.
    /// </summary>
    public class PlayAudioOnHover : MonoBehaviour, IPointerEnterHandler
    {
        [Required]
        [SerializeField] private AudioContainer hoverSound;

        private AudioManager _audioManager;

#region Unity Callbacks
        private void Start() => ServiceLocator.TryGet(out _audioManager);
#endregion

        /// <summary>Plays the configured sound when the element is pointed at.</summary>
        /// <param name="eventData">The event system payload. Not read; only the event matters.</param>
        public void OnPointerEnter(PointerEventData eventData) => _audioManager.PlaySound(hoverSound);
    }
}