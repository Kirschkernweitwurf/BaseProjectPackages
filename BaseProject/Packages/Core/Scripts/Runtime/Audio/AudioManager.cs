using System.Collections;
using System.Collections.Generic;
using Base.AttributePackage;
using Base.CorePackage.Audio.Pool;
using Base.ServicePackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.Pool;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace Base.CorePackage.Audio
{
    /// <summary>
    /// Manages the playback of sound effects and music. Owns the public play, stop and fade API and
    /// delegates the details to <see cref="AudioPoolManager"/>, <see cref="AudioSourceConfigurator"/>,
    /// <see cref="ActiveSounds"/> and <see cref="AudioFader"/>.
    /// </summary>
    public class AudioManager : GameServiceBehaviour
    {
        private const float MinimumPitch = 0.01f;

        [Title("Settings")]
        [Tooltip("Extra seconds added on top of the clip length before a source is released again.")]
        [Min(0f)] [SerializeField] private float minimumDelay = 0.1f;

        [Tooltip("Lowest pitch a source can get when its container randomizes the pitch.")]
        [MinMax(0.01f, 3f)] [SerializeField] private float minPitchInclusive = 0.95f;

        [Tooltip("Highest pitch a source can get when its container randomizes the pitch.")]
        [MinMax(0.01f, 3f)] [SerializeField] private float maxPitchInclusive = 1.05f;

        [Title("Dependencies")]
        [Required] [SerializeField] private AudioPoolManager audioPoolManager;

        /// <summary>
        /// A configurator carrying the current pitch settings. Built per call so tweaking the pitch range
        /// in play mode takes effect immediately. Being a struct, this does not allocate.
        /// </summary>
        private AudioSourceConfigurator Configurator => new(minPitchInclusive, maxPitchInclusive);

        private readonly ActiveSounds _activeSounds = new();

#region Unity Callbacks
        private void OnEnable() => audioPoolManager.PoolsCleared += OnPoolsCleared;

        private void OnValidate()
        {
            if (maxPitchInclusive < minPitchInclusive)
                maxPitchInclusive = minPitchInclusive;
        }

        private void OnDisable() => audioPoolManager.PoolsCleared -= OnPoolsCleared;
#endregion

        /// <summary>
        /// Plays a clip from the given container.
        /// </summary>
        /// <param name="container">The audio container holding the clip.</param>
        /// <param name="position">The world position to play the sound at.</param>
        /// <param name="autoStop">
        /// If true, the source is released automatically once playback finishes.
        /// Looping containers are never released automatically.
        /// </param>
        /// <returns>The playing AudioSource, or null if none was available.</returns>
        public AudioSource PlaySound(AudioContainer container, Vector3 position = default, bool autoStop = true)
        {
            if (container == null)
            {
                CustomLogger.LogError($"Tried playing a sound but the {nameof(AudioContainer)} is null.", this);
                return null;
            }

            AudioClip clip = container.GetRandomClip();
            if (clip == null)
            {
                CustomLogger.LogError($"{container.name} has no clip assigned, so nothing can play.", container);
                return null;
            }

            EnforceMaxClips(container);

            AudioSource source = audioPoolManager.GetAudioSource(container.AudioType);
            if (source == null)
            {
                CustomLogger.LogWarning($"No available audio source for {container.AudioType}.", this);
                return null;
            }

            Configurator.Apply(source, container, clip, position);
            _activeSounds.Add(container, source);

            if (container.Delay > 0f)
                StartCoroutine(PlayAfterDelay(source, container.Delay));
            else
                source.Play();

            if (autoStop
                && !container.Loop)
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

            List<AudioSource> snapshot = ListPool<AudioSource>.Get();
            snapshot.AddRange(sources);

            foreach (AudioSource source in snapshot)
                Release(source);

            ListPool<AudioSource>.Release(snapshot);
        }

        /// <summary>
        /// Stops a single playing source and returns it to the pool.
        /// </summary>
        /// <param name="source">The AudioSource to stop.</param>
        public void StopSound(AudioSource source)
        {
            if (source == null)
            {
                CustomLogger.LogWarning($"Tried stopping a null {nameof(AudioSource)}.", this);
                return;
            }

            Release(source);
        }

        /// <summary>
        /// Stops every source this manager knows about and returns them to their pools.
        /// </summary>
        public void StopAll()
        {
            List<AudioSource> snapshot = ListPool<AudioSource>.Get();
            _activeSounds.CopyAllSourcesTo(snapshot);

            foreach (AudioSource source in snapshot)
                Release(source);

            ListPool<AudioSource>.Release(snapshot);
        }

        /// <summary>
        /// Whether at least one source is currently playing for the given container.
        /// </summary>
        /// <param name="container">The container to check.</param>
        public bool IsPlaying(AudioContainer container)
        {
            if (container == null)
            {
                CustomLogger.LogWarning($"Tried checking playback but the {nameof(AudioContainer)} is null.", this);
                return false;
            }

            return _activeSounds.CountOf(container) > 0;
        }

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

            List<AudioSource> snapshot = ListPool<AudioSource>.Get();
            snapshot.AddRange(sources);

            try
            {
                foreach (AudioSource source in snapshot)
                    yield return FadeIn(source, targetVolume, duration, container.IgnorePause);
            }
            finally
            {
                ListPool<AudioSource>.Release(snapshot);
            }
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

            List<AudioSource> snapshot = ListPool<AudioSource>.Get();
            snapshot.AddRange(sources);

            try
            {
                foreach (AudioSource source in snapshot)
                    yield return FadeOut(source, duration, container.IgnorePause);
            }
            finally
            {
                ListPool<AudioSource>.Release(snapshot);
            }
        }

        /// <summary>
        /// Fades a single source in from silence to a target volume.
        /// </summary>
        /// <param name="source">The AudioSource to fade in.</param>
        /// <param name="targetVolume">The target volume level.</param>
        /// <param name="duration">Time in seconds to reach the target volume.</param>
        /// <param name="ignoreTimeScale">If true, the fade keeps running while the game is paused.</param>
        public IEnumerator FadeIn(AudioSource source, float targetVolume, float duration,
            bool ignoreTimeScale = false)
        {
            if (source == null)
            {
                CustomLogger.LogWarning($"Tried fading in but the {nameof(AudioSource)} is null.", this);
                yield break;
            }

            source.volume = 0f;
            source.Play();
            yield return AudioFader.To(source, targetVolume, duration, ignoreTimeScale);
        }

        /// <summary>
        /// Fades a single source out and returns it to the pool.
        /// </summary>
        /// <param name="source">The AudioSource to fade out.</param>
        /// <param name="duration">Time in seconds to complete the fade-out.</param>
        /// <param name="ignoreTimeScale">If true, the fade keeps running while the game is paused.</param>
        public IEnumerator FadeOut(AudioSource source, float duration, bool ignoreTimeScale = false)
        {
            if (source == null)
            {
                CustomLogger.LogWarning($"Tried fading out but the {nameof(AudioSource)} is null.", this);
                yield break;
            }

            yield return AudioFader.To(source, 0f, duration, ignoreTimeScale);

            Release(source);
        }

        /// <summary>
        /// Tweens a single source to a target volume without stopping it.
        /// </summary>
        /// <param name="source">The AudioSource to change.</param>
        /// <param name="targetVolume">The target volume level.</param>
        /// <param name="duration">Time in seconds to reach the target volume.</param>
        /// <param name="ignoreTimeScale">If true, the tween keeps running while the game is paused.</param>
        public IEnumerator ChangeVolume(AudioSource source, float targetVolume, float duration,
            bool ignoreTimeScale = false)
        {
            if (source == null)
            {
                CustomLogger.LogWarning($"Tried changing volume but the {nameof(AudioSource)} is null.", this);
                yield break;
            }

            yield return AudioFader.To(source, targetVolume, duration, ignoreTimeScale);
        }

        /// <summary>
        /// Plays a source after a delay, if it still exists.
        /// </summary>
        /// <param name="source">The source to play.</param>
        /// <param name="delay">Seconds to wait before playing.</param>
        private static IEnumerator PlayAfterDelay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (source != null)
                source.Play();
        }

        /// <summary>
        /// Drops all tracking after the pools were cleared, so released sources are never handed out twice.
        /// </summary>
        private void OnPoolsCleared() => _activeSounds.Clear();

        /// <summary>
        /// Releases the oldest sources until the container is below its play limit.
        /// </summary>
        /// <param name="container">The container about to play another clip.</param>
        private void EnforceMaxClips(AudioContainer container)
        {
            if (container.HasUnlimitedClips)
                return;

            while (_activeSounds.CountOf(container) >= container.MaxClipsPlaying)
            {
                AudioSource oldest = _activeSounds.GetOldest(container);
                if (oldest == null)
                    break;

                Release(oldest);
            }
        }

        /// <summary>
        /// Releases a source back to the pool once its clip has finished playing.
        /// The wait accounts for the pitch, because a pitched clip is shorter or longer than its raw length.
        /// </summary>
        /// <param name="source">The playing source.</param>
        /// <param name="container">The container the source plays for.</param>
        private IEnumerator ReleaseAfterPlayback(AudioSource source, AudioContainer container)
        {
            float pitch = Mathf.Max(Mathf.Abs(source.pitch), MinimumPitch);
            float clipLength = source.clip != null
                ? source.clip.length / pitch
                : 0f;

            yield return new WaitForSeconds(clipLength + container.Delay + minimumDelay);

            Release(source);
        }

        /// <summary>
        /// Looks up the active sources for a container, logging if the caller passed nothing
        /// or if nothing is playing.
        /// </summary>
        /// <param name="container">The container to look up.</param>
        /// <param name="action">The action being attempted, used in the log message.</param>
        /// <param name="sources">The sources currently playing for the container.</param>
        /// <returns>True if at least one source is playing.</returns>
        private bool TryGetActiveSources(AudioContainer container, string action,
            out IReadOnlyList<AudioSource> sources)
        {
            sources = null;

            if (container == null)
            {
                CustomLogger.LogError($"Tried {action} but the {nameof(AudioContainer)} is null.", this);
                return false;
            }

            if (_activeSounds.TryGetSources(container, out sources))
                return true;

            CustomLogger.LogWarning($"Tried {action} {container.name} but it's not playing.", this);
            return false;
        }

        /// <summary>
        /// Stops a source, returns it to the pool and removes it from tracking. Stays silent on purpose:
        /// a scene load can destroy a pooled source while a coroutine is still holding it, which is normal.
        /// </summary>
        /// <param name="source">The source to release.</param>
        private void Release(AudioSource source)
        {
            if (!_activeSounds.TryGetContainer(source, out AudioContainer container))
                return;

            _activeSounds.Remove(source);

            if (source == null)
                return;

            source.Stop();
            audioPoolManager.ReleaseAudioSource(container.AudioType, source);
        }
    }
}