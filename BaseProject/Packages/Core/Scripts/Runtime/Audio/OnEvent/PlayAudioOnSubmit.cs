using Base.AttributePackage;
using Base.CorePackage.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.CorePackage.Audio.OnEvent
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

        public void OnSubmit(BaseEventData eventData) => _audioManager.PlaySound(submitSound);
    }
}