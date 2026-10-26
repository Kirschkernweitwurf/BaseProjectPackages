using System;
using Base.TweeningPackage.Core.Data;
using UnityEngine;

namespace Base.TweeningPackage.Core
{
    /// <summary>
    /// Fluent options for every tween and timeline, chained right after creating it:
    /// <c>rect.TweenAnchorPos(target, 0.3f).SetEase(EEasingType.EaseOutCubic).SetLink(gameObject)</c>.
    /// Each call returns the tween, typed as it came in, so the chain keeps going.
    /// </summary>
    public static class TweenOptions
    {
        /// <summary>Waits <paramref name="seconds"/> after the start before moving.</summary>
        public static T SetDelay<T>(this T tween, float seconds) where T : TweenBase
        {
            tween.ApplyDelay(seconds);
            return tween;
        }

        /// <summary>
        /// Plays the tween <paramref name="count"/> times, or endlessly with -1. With
        /// <see cref="ELoopType.PingPong"/> every direction counts as one loop, so 2 goes there and back.
        /// <see cref="ELoopType.Continue"/> starts each loop where the last one ended.
        /// </summary>
        public static T SetLoops<T>(this T tween, int count, ELoopType type = ELoopType.Restart) where T : TweenBase
        {
            tween.ApplyLoops(count, type);
            return tween;
        }

        /// <summary>
        /// Runs on unscaled time (true, the default) or on scaled time (false), which stops while
        /// <see cref="Time.timeScale"/> is 0.
        /// </summary>
        public static T SetUnscaledTime<T>(this T tween, bool isUnscaled = true) where T : TweenBase
        {
            tween.ApplyUnscaled(isUnscaled);
            return tween;
        }

        /// <summary>Stops the tween quietly once <paramref name="gameObject"/> is destroyed.</summary>
        public static T SetLink<T>(this T tween, GameObject gameObject) where T : TweenBase
        {
            tween.ApplyLink(gameObject);
            return tween;
        }

        /// <summary>Makes <see cref="Tweens.Kill"/> with <paramref name="target"/> find the tween.</summary>
        public static T SetTarget<T>(this T tween, object target) where T : TweenBase
        {
            tween.ApplyTarget(target);
            return tween;
        }

        /// <summary>Calls <paramref name="callback"/> when the tween completes, naturally or snapped to its
        /// end.</summary>
        public static T WhenComplete<T>(this T tween, Action callback) where T : TweenBase
        {
            if (callback != null)
                tween.OnComplete += _ => callback();

            return tween;
        }

        /// <summary>Calls <paramref name="callback"/> when the tween ends for any reason.</summary>
        public static T WhenKilled<T>(this T tween, Action callback) where T : TweenBase
        {
            if (callback != null)
                tween.OnKill += _ => callback();

            return tween;
        }
    }
}
