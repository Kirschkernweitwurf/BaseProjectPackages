using System;
using Base.ServicesPackage;
using Base.TweeningPackage.Core.Data;
using UnityEngine;

namespace Base.TweeningPackage.Core
{
    /// <summary>
    /// Entry point for tweens made in code that are not about one component: timelines, plain values, delayed calls,
    /// and stopping tweens by their target. Tweens of components start from the shortcuts in
    /// <see cref="TweenShortcuts"/>. Everything made here starts right away.
    /// </summary>
    public static class Tweens
    {
        /// <summary>
        /// The ease of every tween made here and in <see cref="TweenShortcuts"/> until set otherwise.
        /// </summary>
        public const EEasingType DefaultEase = EEasingType.EaseOutQuad;

        /// <summary>
        /// Starts an empty <see cref="TweenTimeline"/>. Fill it in the same frame; it plays from the next update.
        /// </summary>
        public static TweenTimeline Timeline()
        {
            TweenTimeline timeline = new();
            timeline.Start();
            return timeline;
        }

        /// <summary>
        /// Stops every tween made for <paramref name="target"/>, or tagged with it through
        /// <see cref="TweenOptions.SetTarget{T}"/>.
        /// </summary>
        /// <param name="target">The component or object the tweens belong to.</param>
        /// <param name="complete">True to snap each tween to its end first.</param>
        /// <returns>How many tweens were stopped.</returns>
        public static int Kill(object target, bool complete = false) =>
            ServiceLocator.TryGetOptional(out TweenRunner runner)
                ? runner.Kill(target, complete)
                : 0;

        /// <summary>Calls <paramref name="callback"/> after <paramref name="seconds"/>.</summary>
        public static TweenTimeline Delay(float seconds, Action callback) => Timeline()
            .AppendInterval(seconds)
            .AppendCallback(callback);

        /// <summary>Tweens any float from what <paramref name="getter"/> reads when it starts moving.</summary>
        public static Tween<float> To(Func<float> getter, Action<float> setter, float to, float duration)
            => Run(getter, setter, to, duration);

        /// <summary>Tweens any <see cref="Vector2"/> from what <paramref name="getter"/> reads when it starts
        /// moving.</summary>
        public static Tween<Vector2> To(Func<Vector2> getter, Action<Vector2> setter, Vector2 to, float duration)
            => Run(getter, setter, to, duration);

        /// <summary>Tweens any <see cref="Vector3"/> from what <paramref name="getter"/> reads when it starts
        /// moving.</summary>
        public static Tween<Vector3> To(Func<Vector3> getter, Action<Vector3> setter, Vector3 to, float duration)
            => Run(getter, setter, to, duration);

        /// <summary>Tweens any <see cref="Color"/> from what <paramref name="getter"/> reads when it starts
        /// moving.</summary>
        public static Tween<Color> To(Func<Color> getter, Action<Color> setter, Color to, float duration)
            => Run(getter, setter, to, duration);

        /// <summary>Runs a float from <paramref name="from"/> to <paramref name="to"/> and hands each step to
        /// <paramref name="setter"/>.</summary>
        public static Tween<float> Value(float from, float to, float duration, Action<float> setter)
        {
            Tween<float> tween = new(to, duration, setter, TweenLerpUtility.LerpFloatUnclamped,
                Easings.Get(DefaultEase), targetObj: null, from: from);

            tween.Start();
            return tween;
        }

        private static Tween<T> Run<T>(Func<T> getter, Action<T> setter, T to, float duration)
        {
            Tween<T> tween = new(to, duration, setter, TweenLerpUtility.Resolve<T>(), Easings.Get(DefaultEase),
                targetObj: null, fromGetter: getter);

            tween.Start();
            return tween;
        }
    }
}
