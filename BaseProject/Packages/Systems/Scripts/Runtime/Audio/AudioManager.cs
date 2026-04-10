using System.Collections;
using System.Collections.Generic;
using Systems.Audio.Pool;
using Systems.Services;
using UnityEngine;
using Utility.Logging;
using Random = UnityEngine.Random;
// ReSharper disable MemberCanBePrivate.Global

namespace Systems.Audio
{
    /// <summary>
    /// Manages the playback of sound effects and music.
    /// </summary>
    public class AudioManager : GameServiceBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minimumDelay = 0.1f;
        [SerializeField] private float minPitchInclusive = 0.95f;
        [SerializeField] private float maxPitchInclusive = 1.05f;

        [Header("Dependencies")]
        [SerializeField] private AudioPoolManager audioPoolManager;

        private readonly Dictionary<AudioContainer, List<AudioSource>> _activeSounds = new();

        /// <summary>
        /// Selects a random audio clip from an array.
        /// </summary>
        /// <param name="clips">Array of audio clips to choose from.</param>
        /// <returns>A randomly chosen AudioClip, or null if the array is empty.</returns>
        private static AudioClip ChooseRandomAudio(AudioClip[] clips)
        {
            return clips is { Length: > 0 } ? clips[Random.Range(0, clips.Length)] : null;
        }

        /// <summary>
        /// Plays an audio clip from the given AudioContainer.
        /// </summary>
        /// <param name="soundContainer">The audio container holding the clip.</param>
        /// <param name="position">The position to play the sound at.</param>
        /// <param name="autoStop">If true, automatically stops and releases the audio source.</param>
        public AudioSource PlaySound(AudioContainer soundContainer, Vector3 position = default, bool autoStop = true)
        {
            if (!_activeSounds.ContainsKey(soundContainer))
                _activeSounds[soundContainer] = new List<AudioSource>();

            List<AudioSource> sources = _activeSounds[soundContainer];

            AudioSource audioSource;

            if (soundContainer.maxClipsPlaying != -1 && sources.Count >= soundContainer.maxClipsPlaying)
            {
                // Reuse oldest AudioSource
                audioSource = sources[0];
                sources.RemoveAt(0);

                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioPoolManager.ReleaseAudioSource(soundContainer.audioType, audioSource);
                }
            }

            audioSource = audioPoolManager.GetAudioSource(soundContainer.audioType);
            if (!audioSource)
            {
                CustomLogger.LogWarning($"No available audio source for {soundContainer.audioType}.", this);
                return null;
            }

            audioSource.transform.position = position;
            audioSource.clip = ChooseRandomAudio(soundContainer.clips);
            audioSource.ignoreListenerPause = soundContainer.ignorePause;
            audioSource.volume = soundContainer.volume;
            audioSource.loop = soundContainer.loop;
            audioSource.pitch = soundContainer.randomizePitch
                ? Random.Range(minPitchInclusive, maxPitchInclusive)
                : 1.0f;

            sources.Add(audioSource);

            if (soundContainer.delay > 0)
                StartCoroutine(PlaySoundAfterDelay(audioSource, soundContainer.delay));
            else
                audioSource.Play();

            if (autoStop)
                StartCoroutine(ReleaseAudioAfterTime(audioSource,
                    audioSource.clip.length + soundContainer.delay,
                    soundContainer.audioType, soundContainer));

            return audioSource;
        }

        /// <summary>
        /// Stops a currently playing sound.
        /// </summary>
        /// <param name="soundContainer">The audio container associated with the sound.</param>
        public void StopSound(AudioContainer soundContainer)
        {
            if (!_activeSounds.TryGetValue(soundContainer, out List<AudioSource> sources) || sources.Count == 0)
            {
                CustomLogger.LogWarning($"Tried stopping {soundContainer.name} but it's not playing.", this);
                return;
            }

            foreach (AudioSource source in new List<AudioSource>(sources))
            {
                if (source == null)
                    continue;

                StopSound(source, soundContainer);
            }
        }

        public void StopSound(AudioSource audioSource, AudioContainer audioContainer)
        {
            if (audioSource == null || !_activeSounds.TryGetValue(audioContainer, out List<AudioSource> sources)
                                    || !sources.Contains(audioSource))
            {
                CustomLogger.LogWarning($"Tried stopping {audioContainer.name} but it's not playing.", this);
                return;
            }

            audioSource.Stop();
            audioPoolManager.ReleaseAudioSource(audioContainer.audioType, audioSource);
            sources.Remove(audioSource);

            if (sources.Count == 0)
                _activeSounds.Remove(audioContainer);
        }

        /// <summary>
        /// Gradually fades out an audio source over a specified duration.
        /// </summary>
        /// <param name="audioContainer">The AudioContainer holding the audio source to fade out.</param>
        /// <param name="duration">Time in seconds to complete the fade-out.</param>
        public IEnumerator FadeOut(AudioContainer audioContainer, float duration)
        {
            if (!_activeSounds.TryGetValue(audioContainer, out List<AudioSource> sources) || sources.Count == 0)
            {
                CustomLogger.LogWarning($"Tried fading out {audioContainer.name} but it's not playing.", this);
                yield break;
            }

            foreach (AudioSource audioSource in sources)
            {
                float startVolume = audioSource.volume;

                while (audioSource.volume > 0)
                {
                    audioSource.volume -= startVolume * Time.deltaTime / duration;
                    yield return null;
                }

                audioSource.Stop();
                audioSource.volume = startVolume;
                audioPoolManager.ReleaseAudioSource(audioContainer.audioType, audioSource);
            }

            sources.Clear();
            _activeSounds.Remove(audioContainer);
        }

        /// <summary>
        /// Gradually fades in an audio source to a target volume over a specified duration.
        /// </summary>
        /// <param name="audioContainer">The AudioContainer holding the audio source to fade in.</param>
        /// <param name="targetVolume">The target volume level.</param>
        /// <param name="duration">Time in seconds to reach the target volume.</param>
        public IEnumerator FadeIn(AudioContainer audioContainer, float targetVolume, float duration)
        {
            if (!_activeSounds.TryGetValue(audioContainer, out List<AudioSource> sources) || sources.Count == 0)
            {
                CustomLogger.LogWarning($"Tried fading in {audioContainer.name} but it's not playing.", this);
                yield break;
            }

            foreach (AudioSource audioSource in sources)
            {
                audioSource.volume = 0;
                audioSource.Play();

                while (audioSource.volume < targetVolume)
                {
                    audioSource.volume += Time.deltaTime / duration;
                    yield return null;
                }

                audioSource.volume = targetVolume;
            }
        }

        /// <summary>
        /// Gradually fades out an audio source over a specified duration.
        /// </summary>
        /// <param name="audioSource">The AudioSource to fade out.</param>
        /// <param name="duration">Time in seconds to complete the fade-out.</param>
        public IEnumerator FadeOut(AudioSource audioSource, float duration)
        {
            if (audioSource == null || !audioSource.isPlaying)
                yield break;

            float startVolume = audioSource.volume;

            while (audioSource != null && audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime / duration;
                yield return null;
            }

            if (audioSource == null)
                yield break;

            audioSource.Stop();
            audioSource.volume = startVolume; // Reset volume for reuse
        }

        /// <summary>
        /// Gradually fades in an audio source to a target volume over a specified duration.
        /// </summary>
        /// <param name="audioSource">The AudioSource to fade in.</param>
        /// <param name="targetVolume">The target volume level.</param>
        /// <param name="duration">Time in seconds to reach the target volume.</param>
        public IEnumerator FadeIn(AudioSource audioSource, float targetVolume, float duration)
        {
            if (audioSource == null)
            {
                CustomLogger.LogWarning("Tried fading in but AudioSource is null.", this);
                yield break;
            }

            audioSource.volume = 0;
            audioSource.Play();

            while (audioSource != null && audioSource.volume < targetVolume)
            {
                audioSource.volume += Time.deltaTime / duration;
                yield return null;
            }

            if (audioSource != null)
                audioSource.volume = targetVolume;
        }

        public IEnumerator ChangeVolume(AudioSource audioSource, float targetVolume, float duration)
        {
            if (audioSource == null)
            {
                CustomLogger.LogWarning("Tried changing volume but AudioSource is null.", this);
                yield break;
            }

            float startVolume = audioSource.volume;
            float progress = 0f;
            while (audioSource != null && Mathf.Abs(audioSource.volume - targetVolume) > 0.01f)
            {
                progress += Time.deltaTime / duration;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, progress);
                yield return null;
            }

            if (audioSource != null)
                audioSource.volume = targetVolume;
        }

        /// <summary>
        /// Plays an audio source after a specified delay.
        /// </summary>
        /// <param name="audioSource">The AudioSource to play.</param>
        /// <param name="delay">The delay in seconds before playback.</param>
        private IEnumerator PlaySoundAfterDelay(AudioSource audioSource, float delay)
        {
            yield return new WaitForSeconds(delay);
            audioSource?.Play();
        }

        /// <summary>
        /// Releases an audio source back to the pool after a delay.
        /// </summary>
        /// <param name="audioSource">The AudioSource to be released.</param>
        /// <param name="delay">Time in seconds before the audio source is released.</param>
        /// <param name="audioType">The type of audio being released.</param>
        /// <param name="container">The container from which the audio source originated.</param>
        private IEnumerator ReleaseAudioAfterTime(AudioSource audioSource, float delay, EAudioType audioType,
            AudioContainer container)
        {
            yield return new WaitForSeconds(delay + minimumDelay);

            if (audioSource == null)
                yield break;

            if (_activeSounds.TryGetValue(container, out List<AudioSource> list))
            {
                list.Remove(audioSource);
                if (list.Count == 0)
                    _activeSounds.Remove(container);
            }

            audioPoolManager.ReleaseAudioSource(audioType, audioSource);
        }
    }
}