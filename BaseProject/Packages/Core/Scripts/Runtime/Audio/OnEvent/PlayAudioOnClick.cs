using Base.AttributePackage;
using Base.CorePackage.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.CorePackage.Audio.OnEvent
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

        public void OnPointerClick(PointerEventData eventData) => _audioManager.PlaySound(clickSound);
    }
}