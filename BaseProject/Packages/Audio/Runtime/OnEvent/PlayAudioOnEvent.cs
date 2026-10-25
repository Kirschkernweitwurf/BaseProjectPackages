using Base.ServicesPackage;
using UnityEngine;

namespace Base.AudioPackage.OnEvent
{
    /// <summary>
    /// Base for components that play an <see cref="AudioContainer"/> sound on a UI event. Looks the
    /// <see cref="AudioManager"/> up on the first sound instead of in Start, because an element that gets selected in
    /// the frame it turns on, like the first row of a settings page, receives the event before its Start runs.
    /// </summary>
    public abstract class PlayAudioOnEvent : MonoBehaviour
    {
        private AudioManager _audioManager;

        /// <summary>Plays <paramref name="sound"/> through the audio manager.</summary>
        /// <param name="sound">The sound to play.</param>
        protected void Play(AudioContainer sound)
        {
            // TryGet reports a missing manager itself.
            if (_audioManager == null
                && !ServiceLocator.TryGet(out _audioManager))
                return;

            _audioManager.PlaySound(sound);
        }
    }
}