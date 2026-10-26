using System;
using Base.TweeningPackage.Core.Data;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Core
{
    /// <summary>
    /// Extension methods that tween a component straight from code, like <c>transform.TweenScale(1.2f, 0.3f)</c>.
    /// Every tween starts right away from the value the component has when it starts moving, eases with
    /// <see cref="Tweens.DefaultEase"/> and has the component as its <see cref="TweenBase.Target"/>, so
    /// <see cref="Tweens.Kill"/> with the component stops it. Chain <see cref="TweenOptions"/> for the rest.
    /// </summary>
    public static class TweenShortcuts
    {
        private const float JumpArcHeight = 4f;

        /// <summary>Tweens the world position to <paramref name="to"/>.</summary>
        public static Tween<Vector3> TweenMove(this Transform transform, Vector3 to, float duration)
            => Run(transform, () => transform.position, value => transform.position = value, to, duration);

        /// <summary>Tweens the local position to <paramref name="to"/>.</summary>
        public static Tween<Vector3> TweenLocalMove(this Transform transform, Vector3 to, float duration)
            => Run(transform, () => transform.localPosition, value => transform.localPosition = value, to, duration);

        /// <summary>Tweens the local scale to <paramref name="to"/>.</summary>
        public static Tween<Vector3> TweenScale(this Transform transform, Vector3 to, float duration)
            => Run(transform, () => transform.localScale, value => transform.localScale = value, to, duration);

        /// <summary>Tweens the local scale to <paramref name="to"/> on every axis.</summary>
        public static Tween<Vector3> TweenScale(this Transform transform, float to, float duration)
            => transform.TweenScale(Vector3.one * to, duration);

        /// <summary>Turns the world rotation to <paramref name="euler"/> the shortest way.</summary>
        public static Tween<Quaternion> TweenRotate(this Transform transform, Vector3 euler, float duration)
            => Run(transform, () => transform.rotation, value => transform.rotation = value, Quaternion.Euler(euler),
                duration);

        /// <summary>Turns the local rotation to <paramref name="euler"/> the shortest way.</summary>
        public static Tween<Quaternion> TweenLocalRotate(this Transform transform, Vector3 euler, float duration)
            => transform.TweenLocalRotate(Quaternion.Euler(euler), duration);

        /// <summary>Turns the local rotation to <paramref name="to"/> the shortest way.</summary>
        public static Tween<Quaternion> TweenLocalRotate(this Transform transform, Quaternion to, float duration)
            => Run(transform, () => transform.localRotation, value => transform.localRotation = value, to, duration);

        /// <summary>
        /// Pushes the local scale by <paramref name="punch"/> and lets it swing back to where it was.
        /// </summary>
        /// <param name="transform">The transform to punch.</param>
        /// <param name="punch">The biggest push, added to the scale.</param>
        /// <param name="duration">Seconds until the scale is back where it was.</param>
        /// <param name="vibrato">Swings per second, as in DOTween. 1 or less is a single swell and back.</param>
        /// <param name="elasticity">How far the swings go past the start, from 0 (never) to 1 (as far as the
        /// push).</param>
        public static Tween<float> TweenPunchScale(this Transform transform, Vector3 punch, float duration,
            int vibrato = 10, float elasticity = 1f)
            => Punch(transform, () => transform.localScale, value => transform.localScale = value, punch, duration,
                vibrato, elasticity);

        /// <summary>Pushes the local position by <paramref name="punch"/> and lets it swing back.</summary>
        /// <inheritdoc cref="TweenPunchScale"/>
        public static Tween<float> TweenPunchPosition(this Transform transform, Vector3 punch, float duration,
            int vibrato = 10, float elasticity = 1f)
            => Punch(transform, () => transform.localPosition, value => transform.localPosition = value, punch,
                duration, vibrato, elasticity);

        /// <summary>Turns the local rotation by <paramref name="punch"/> degrees and lets it swing back.</summary>
        /// <inheritdoc cref="TweenPunchScale"/>
        public static Tween<float> TweenPunchRotation(this Transform transform, Vector3 punch, float duration,
            int vibrato = 10, float elasticity = 1f)
            => Punch(transform, () => transform.localEulerAngles,
                value => transform.localRotation = Quaternion.Euler(value), punch, duration, vibrato, elasticity);

        /// <summary>
        /// Moves to the world position <paramref name="to"/> in <paramref name="jumps"/> hops of
        /// <paramref name="height"/>.
        /// </summary>
        public static Tween<float> TweenJump(this Transform transform, Vector3 to, float height, int jumps,
            float duration)
            => Jump(transform, () => transform.position, value => transform.position = value, to, height, jumps,
                duration);

        /// <summary>
        /// Moves to the local position <paramref name="to"/> in <paramref name="jumps"/> hops of
        /// <paramref name="height"/>.
        /// </summary>
        public static Tween<float> TweenLocalJump(this Transform transform, Vector3 to, float height, int jumps,
            float duration)
            => Jump(transform, () => transform.localPosition, value => transform.localPosition = value, to, height,
                jumps, duration);

        /// <summary>
        /// Moves the local position through <paramref name="path"/>, from where it is to the last point, at an even
        /// speed. <paramref name="isCurved"/> rounds the corners into a Catmull-Rom curve through every point.
        /// </summary>
        public static Tween<float> TweenLocalPath(this Transform transform, Vector3[] path, float duration,
            bool isCurved = false)
        {
            PathSampler sampler = new(path, isCurved);

            return Progress(transform, () => sampler.Begin(transform.localPosition),
                t => transform.localPosition = sampler.At(t), duration);
        }

        /// <summary>Tweens the anchored position to <paramref name="to"/>.</summary>
        public static Tween<Vector2> TweenAnchorPos(this RectTransform rect, Vector2 to, float duration)
            => Run(rect, () => rect.anchoredPosition, value => rect.anchoredPosition = value, to, duration);

        /// <summary>Tweens the horizontal anchored position to <paramref name="to"/>.</summary>
        public static Tween<float> TweenAnchorPosX(this RectTransform rect, float to, float duration)
            => Run(rect, () => rect.anchoredPosition.x,
                value => rect.anchoredPosition = new Vector2(value, rect.anchoredPosition.y), to, duration);

        /// <summary>Tweens the vertical anchored position to <paramref name="to"/>.</summary>
        public static Tween<float> TweenAnchorPosY(this RectTransform rect, float to, float duration)
            => Run(rect, () => rect.anchoredPosition.y,
                value => rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, value), to, duration);

        /// <summary>Tweens the size delta to <paramref name="to"/>.</summary>
        public static Tween<Vector2> TweenSizeDelta(this RectTransform rect, Vector2 to, float duration)
            => Run(rect, () => rect.sizeDelta, value => rect.sizeDelta = value, to, duration);

        /// <summary>Fades the group's alpha to <paramref name="to"/>.</summary>
        public static Tween<float> TweenFade(this CanvasGroup group, float to, float duration)
            => Run(group, () => group.alpha, value => group.alpha = value, to, duration);

        /// <summary>Fades the graphic's color alpha to <paramref name="to"/>, also for texts.</summary>
        public static Tween<float> TweenFade(this Graphic graphic, float to, float duration)
            => Run(graphic, () => graphic.color.a, value =>
            {
                Color color = graphic.color;
                color.a = value;
                graphic.color = color;
            }, to, duration);

        /// <summary>Tweens the graphic's color to <paramref name="to"/>, also for texts.</summary>
        public static Tween<Color> TweenColor(this Graphic graphic, Color to, float duration)
            => Run(graphic, () => graphic.color, value => graphic.color = value, to, duration);

        /// <summary>Fades the sprite's color alpha to <paramref name="to"/>.</summary>
        public static Tween<float> TweenFade(this SpriteRenderer renderer, float to, float duration)
            => Run(renderer, () => renderer.color.a, value =>
            {
                Color color = renderer.color;
                color.a = value;
                renderer.color = color;
            }, to, duration);

        /// <summary>Tweens the sprite's color to <paramref name="to"/>.</summary>
        public static Tween<Color> TweenColor(this SpriteRenderer renderer, Color to, float duration)
            => Run(renderer, () => renderer.color, value => renderer.color = value, to, duration);

        /// <summary>Stops every tween of <paramref name="target"/>. <see cref="Tweens.Kill"/> for
        /// components.</summary>
        public static int KillTweens(this Component target, bool complete = false) => Tweens.Kill(target, complete);

        private static Tween<T> Run<T>(Object target, Func<T> getter, Action<T> setter, T to, float duration)
        {
            Tween<T> tween = new(to, duration, setter, TweenLerpUtility.Resolve<T>(), Easings.Get(Tweens.DefaultEase),
                target, fromGetter: getter);

            tween.Start();
            return tween;
        }

        // Runs 0 to 1 for effects that compute their own value. The start of the effect is read through begin when
        // the tween starts moving, like any other start value.
        private static Tween<float> Progress(Object target, Action begin, Action<float> apply, float duration)
        {
            Tween<float> tween = new(1f, duration, apply, TweenLerpUtility.LerpFloatUnclamped,
                Easings.Get(Tweens.DefaultEase), target, fromGetter: () =>
                {
                    begin();
                    return 0f;
                });

            tween.Start();
            return tween;
        }

        private static Tween<float> Punch(Object target, Func<Vector3> getter, Action<Vector3> setter, Vector3 punch,
            float duration, int vibrato, float elasticity)
        {
            Vector3 origin = default;
            int swings = Mathf.Max(1, Mathf.FloorToInt(vibrato * duration) - 1);
            float backSwing = Mathf.Clamp01(elasticity);

            // A punch is a shape over time, so it stays linear unless someone asks otherwise.
            return Progress(target, () => origin = getter(),
                    t => setter(origin + punch * Swing(t, swings, backSwing)), duration)
                .SetEase(EEasingType.Linear);
        }

        private static Tween<float> Jump(Object target, Func<Vector3> getter, Action<Vector3> setter, Vector3 to,
            float height, int jumps, float duration)
        {
            Vector3 origin = default;
            int hops = Mathf.Max(1, jumps);

            return Progress(target, () => origin = getter(), t =>
            {
                float hop = t * hops - Mathf.Floor(t * hops);
                float arc = t >= 1f
                    ? 0f
                    : JumpArcHeight * hop * (1f - hop);

                setter(Vector3.LerpUnclamped(origin, to, t) + Vector3.up * (height * arc));
            }, duration);
        }

        // Each swing is half a sine wave. Every swing is a bit weaker than the last, and the ones going back past the
        // start are scaled by the elasticity. Ends exactly at zero.
        private static float Swing(float t, int swings, float backSwing)
        {
            if (t <= 0f || t >= 1f)
                return 0f;

            float x = t * swings;
            int swing = Mathf.Min(Mathf.FloorToInt(x), swings - 1);
            float strength = 1f - (float)swing / swings;
            float direction = swing % 2 == 0
                ? 1f
                : -backSwing;

            return direction * strength * Mathf.Sin((x - swing) * Mathf.PI);
        }
    }
}
