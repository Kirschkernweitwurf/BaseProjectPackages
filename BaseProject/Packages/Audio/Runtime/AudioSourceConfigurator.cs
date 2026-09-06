using UnityEngine;
using Random = UnityEngine.Random;

namespace Base.AudioPackage
{
    /// <summary>
    /// Applies an <see cref="AudioContainer"/>'s settings to an <see cref="AudioSource"/> before playback.
    /// Kept as a struct so the <see cref="AudioManager"/> can create one per call without allocating.
    /// </summary>
    internal readonly struct AudioSourceConfigurator
    {
        private readonly float _minPitchInclusive;
        private readonly float _maxPitchInclusive;

        /// <summary>
        /// Creates a configurator with the pitch range used for randomized playback.
        /// </summary>
        /// <param name="minPitchInclusive">Lowest pitch a randomized source can get.</param>
        /// <param name="maxPitchInclusive">Highest pitch a randomized source can get.</param>
        public AudioSourceConfigurator(float minPitchInclusive, float maxPitchInclusive)
        {
            _minPitchInclusive = minPitchInclusive;
            _maxPitchInclusive = maxPitchInclusive;
        }

        /// <summary>
        /// Writes the container's playback settings onto the source and moves it into position.
        /// </summary>
        /// <param name="source">The pooled source about to play.</param>
        /// <param name="container">The container that defines the playback settings.</param>
        /// <param name="clip">The clip already picked for this playback.</param>
        /// <param name="position">The world position to play the sound at.</param>
        internal void Apply(AudioSource source, AudioContainer container, AudioClip clip, Vector3 position)
        {
            source.transform.position = position;
            source.clip = clip;
            source.ignoreListenerPause = container.IgnorePause;
            source.volume = container.Volume;
            source.loop = container.Loop;
            source.pitch = container.RandomizePitch
                ? Random.Range(_minPitchInclusive, _maxPitchInclusive)
                : 1f;
        }
    }
}