using System.Collections;
using System.Collections.Generic;
using Base.SystemsCorePackage.Audio.Pool;
using Base.SystemsCorePackage.Services;
using UnityEngine;
using Base.UtilityPackage.Logging;
using Random = UnityEngine.Random;
// ReSharper disable MemberCanBePrivate.Global

namespace Base.SystemsCorePackage.Audio
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

        private readonly ActiveSounds _activeSounds = new();

        /// <summary>
        /// Plays a clip from the given container.
        /// </summary>
        /// <param name="container">The audio container holding the clip.</param>
        /// <param name="position">The world position to play the sound at.</param>
        /// <param name="autoStop">If true, the source is released automatically once playback finishes.</param>
        /// <returns>The playing AudioSource, or null if none was available.</returns>
        public AudioSource PlaySound(AudioContainer container, Vector3 position = default, bool autoStop = true)
        {
            EnforceMaxClips(container);

            AudioSource source = audioPoolManager.GetAudioSource(container.audioType);
            if (source == null)
            {
                CustomLogger.LogWarning($"No available audio source for {container.audioType}.", this);
                return null;
            }

            ConfigureSource(source, container, position);
            _activeSounds.Add(container, source);

            if (container.delay > 0)
                StartCoroutine(PlayAfterDelay(source, container.delay));
            else
                source.Play();

            if (autoStop)
                StartCoroutine(ReleaseAfterPlayback(source, container));

            return source;
        }

        /// <summary>
        /// Stops every source currently playing for the given container.
        /// </summary>
        /// <param name="container">The audio container to stop.</param>
        public void StopSound(AudioContainer container)
        {
            if (!TryGetActiveSources(container, "stopping", out IReadOnlyList<AudioSource> sources))
                return;

            foreach (AudioSource source in new List<AudioSource>(sources))
                Release(source);
        }

        /// <summary>
        /// Stops a single playing source and returns it to the pool.
        /// </summary>
        /// <param name="source">The AudioSource to stop.</param>
        public void StopSound(AudioSource source) => Release(source);

        /// <summary>
        /// Fades in every source playing for the given container to a target volume.
        /// </summary>
        /// <param name="container">The AudioContainer to fade in.</param>
        /// <param name="targetVolume">The target volume level.</param>
        /// <param name="duration">Time in seconds to reach the target volume.</param>
        public IEnumerator FadeIn(AudioContainer container, float targetVolume, float duration)
        {
            if (!TryGetActiveSources(container, "fading in", out IReadOnlyList<AudioSource> sources))
                yield break;

            foreach (AudioSource source in new List<AudioSource>(sources))
                yield return FadeIn(source, targetVolume, duration);
        }

        /// <summary>
        /// Fades out every source playing for the given container and releases them.
        /// </summary>
        /// <param name="container">The AudioContainer to fade out.</param>
        /// <param name="duration">Time in seconds to complete the fade-out.</param>
        public IEnumerator FadeOut(AudioContainer container, float duration)
        {
            if (!TryGetActiveSources(container, "fading out", out IReadOnlyList<AudioSource> sources))
                yield break;

            foreach (AudioSource source in new List<AudioSource>(sources))
                yield return FadeOut(source, duration);
        }

        /// <summary>
        /// Fades a single source in from silence to a target volume.
        /// </summary>
        /// <param name="source">The AudioSource to fade in.</param>
        /// <param name="targetVolume">The target volume level.</param>
        /// <param name="duration">Time in seconds to reach the target volume.</param>
        public IEnumerator FadeIn(AudioSource source, float targetVolume, float duration)
        {
            if (source == null)
            {
                CustomLogger.LogWarning("Tried fading in but AudioSource is null.", this);
                yield break;
            }

            source.volume = 0f;
            source.Play();
            yield return AudioFader.To(source, targetVolume, duration);
        }

        /// <summary>
        /// Fades a single source out and returns it to the pool.
        /// </summary>
        /// <param name="source">The AudioSource to fade out.</param>
        /// <param name="duration">Time in seconds to complete the fade-out.</param>
        public IEnumerator FadeOut(AudioSource source, float duration)
        {
            yield return AudioFader.To(source, 0f, duration);
            Release(source);
        }

        /// <summary>
        /// Tweens a single source to a target volume without stopping it.
        /// </summary>
        /// <param name="source">The AudioSource to change.</param>
        /// <param name="targetVolume">The target volume level.</param>
        /// <param name="duration">Time in seconds to reach the target volume.</param>
        public IEnumerator ChangeVolume(AudioSource source, float targetVolume, float duration)
        {
            if (source == null)
            {
                CustomLogger.LogWarning("Tried changing volume but AudioSource is null.", this);
                yield break;
            }

            yield return AudioFader.To(source, targetVolume, duration);
        }

        /// <summary>
        /// Releases the oldest sources until the container is below its play limit.
        /// </summary>
        private void EnforceMaxClips(AudioContainer container)
        {
            if (container.maxClipsPlaying == -1)
                return;

            while (_activeSounds.CountOf(container) >= container.maxClipsPlaying)
            {
                AudioSource oldest = _activeSounds.GetOldest(container);
                if (oldest == null)
                    break;

                Release(oldest);
            }
        }

        /// <summary>
        /// Applies a container's settings to a source before playback.
        /// </summary>
        private void ConfigureSource(AudioSource source, AudioContainer container, Vector3 position)
        {
            source.transform.position = position;
            source.clip = ChooseRandomClip(container.clips);
            source.ignoreListenerPause = container.ignorePause;
            source.volume = container.volume;
            source.loop = container.loop;
            source.pitch = container.randomizePitch
                ? Random.Range(minPitchInclusive, maxPitchInclusive)
                : 1f;
        }

        /// <summary>
        /// Stops a source, returns it to the pool and removes it from tracking.
        /// Safe to call more than once for the same source.
        /// </summary>
        private void Release(AudioSource source)
        {
            if (source == null || !_activeSounds.TryGetContainer(source, out AudioContainer container))
                return;

            source.Stop();
            audioPoolManager.ReleaseAudioSource(container.audioType, source);
            _activeSounds.Remove(source);
        }

        /// <summary>
        /// Looks up the active sources for a container, logging a warning if none are playing.
        /// </summary>
        private bool TryGetActiveSources(AudioContainer container, string action,
            out IReadOnlyList<AudioSource> sources)
        {
            sources = _activeSounds.GetSources(container);
            if (sources is { Count: > 0 })
                return true;

            CustomLogger.LogWarning($"Tried {action} {container.name} but it's not playing.", this);
            return false;
        }

        /// <summary>
        /// Plays a source after a delay, if it still exists.
        /// </summary>
        private static IEnumerator PlayAfterDelay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (source != null)
                source.Play();
        }

        /// <summary>
        /// Releases a source back to the pool once its clip has finished playing.
        /// </summary>
        private IEnumerator ReleaseAfterPlayback(AudioSource source, AudioContainer container)
        {
            float clipLength = source.clip != null ? source.clip.length : 0f;
            yield return new WaitForSeconds(clipLength + container.delay + minimumDelay);
            Release(source);
        }

        /// <summary>
        /// Selects a random clip from an array, or null if it is empty.
        /// </summary>
        private static AudioClip ChooseRandomClip(AudioClip[] clips) =>
            clips is { Length: > 0 } ? clips[Random.Range(0, clips.Length)] : null;
    }
}