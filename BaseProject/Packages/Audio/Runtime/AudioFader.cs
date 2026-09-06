using System.Collections;
using UnityEngine;

namespace Base.AudioPackage
{
    /// <summary>
    /// Provides reusable volume tweening for <see cref="AudioSource"/>s.
    /// </summary>
    public static class AudioFader
    {
        /// <summary>
        /// Gradually changes a source's volume to a target value over a duration.
        /// Stops safely if the source is destroyed mid-tween.
        /// </summary>
        /// <param name="source">The AudioSource to tween.</param>
        /// <param name="targetVolume">The volume to reach.</param>
        /// <param name="duration">Time in seconds to reach the target volume.</param>
        /// <param name="ignoreTimeScale">If true, the fade keeps running while the game is paused.</param>
        public static IEnumerator To(AudioSource source, float targetVolume, float duration,
            bool ignoreTimeScale = false)
        {
            if (source == null)
                yield break;

            targetVolume = Mathf.Clamp01(targetVolume);

            if (duration <= 0f)
            {
                source.volume = targetVolume;
                yield break;
            }

            float startVolume = source.volume;
            float elapsed = 0f;

            while (source != null
                   && elapsed < duration)
            {
                elapsed += ignoreTimeScale
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;

                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            if (source != null)
                source.volume = targetVolume;
        }
    }
}