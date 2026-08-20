using Base.AttributePackage;
using Base.ServicePackage;
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

        public void OnPointerEnter(PointerEventData eventData) => _audioManager.PlaySound(hoverSound);
    }
}