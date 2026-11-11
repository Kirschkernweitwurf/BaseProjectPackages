using Base.ServicesPackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.AudioPackage.OnEvent
{
    /// <summary>
    /// Base for components that play an <see cref="AudioContainer"/> sound on a UI event. Looks the
    /// <see cref="AudioManager"/> up on the first sound instead of in Start, because an element that gets selected in
    /// the frame it turns on, like the first row of a settings page, receives the event before its Start runs. An
    /// element whose <see cref="Selectable"/> is not interactable stays silent, so a disabled button does not sound
    /// like it would do something.
    /// </summary>
    public abstract class PlayAudioOnEvent : MonoBehaviour
    {
        private AudioManager _audioManager;
        private Selectable _selectable;

#region Unity Callbacks
        // Optional: an element without a Selectable, like a plain image, always plays.
        private void Awake() => TryGetComponent(out _selectable);
#endregion

        /// <summary>
        /// Plays <paramref name="sound"/> through the audio manager, unless the element is not interactable.
        /// </summary>
        /// <param name="sound">The sound to play.</param>
        protected void Play(AudioContainer sound)
        {
            // The event system still sends pointer and select events to a disabled Selectable.
            if (!IsInteractable())
                return;

            // TryGet reports a missing manager itself.
            if (_audioManager == null
                && !ServiceLocator.TryGet(out _audioManager))
                return;

            _audioManager.PlaySound(sound);
        }

        // Also false while a parent CanvasGroup is not interactable, like a menu that is still fading in.
        private bool IsInteractable() => _selectable == null || _selectable.IsInteractable();
    }
}