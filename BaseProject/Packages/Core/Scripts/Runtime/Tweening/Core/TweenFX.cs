using System;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Parameters;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// ReSharper disable MemberCanBePrivate.Global

namespace Base.CorePackage.Tweening.Core
{
    /// <summary>
    /// High-level tween factories for common Unity components.
    /// All methods create, start, and return a tween configured with appropriate interpolation.
    /// Supports both traditional parameter calls and structured data-based overloads.
    /// </summary>
    /// <remarks>
    /// Parameter convention: target component first, target value next, settings last.
    /// </remarks>
    public static class TweenFX
    {
        /// <summary>
        /// Tweens a transform's local scale to the specified value.
        /// </summary>
        public static TweenBase ScaleTo(Transform target, Vector3 targetScale, float duration,
            EEasingType easing = EEasingType.EaseOutQuad, float delay = 0f)
        {
            if (target == null)
                return null;

            Tween<Vector3> tween = new(targetScale,
                duration,
                setter: v => target.localScale = v,
                TweenLerpUtility.LerpVector3Unclamped,
                Easings.Get(easing),
                target,
                delay,
                fromGetter: () => target.localScale);

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a transform's local scale using structured tween data.
        /// </summary>
        public static TweenBase ScaleTo(Transform target, Vector3 targetScale, TweenData data)
            => ScaleTo(target, targetScale, data.Duration, data.Easing, data.Delay);

        /// <summary>
        /// Tweens a transform's position to the specified target.
        /// For UI (<see cref="RectTransform"/>) in local mode, uses <see cref="RectTransform.anchoredPosition"/>.
        /// </summary>
        public static TweenBase MoveTo(Transform target, Vector3 targetPosition, float duration,
            EEasingType easing = EEasingType.EaseOutQuad, bool local = false, float delay = 0f)
        {
            if (target == null)
                return null;

            RectTransform rectTransform = target as RectTransform;
            bool isUI = rectTransform != null;

            Func<Vector3> fromGetter;
            Action<Vector3> setter;

            if (local)
            {
                if (isUI)
                {
                    fromGetter = () => rectTransform.anchoredPosition;
                    setter = v => rectTransform.anchoredPosition = new Vector2(v.x, v.y);
                }
                else
                {
                    fromGetter = () => target.localPosition;
                    setter = v => target.localPosition = v;
                }
            }
            else
            {
                fromGetter = () => target.position;
                setter = v => target.position = v;
            }

            Tween<Vector3> tween = new(targetPosition,
                duration,
                setter,
                TweenLerpUtility.LerpVector3Unclamped,
                Easings.Get(easing),
                target,
                delay,
                fromGetter);

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a transform's position using structured tween data.
        /// </summary>
        public static TweenBase MoveTo(Transform target, Vector3 targetPosition, TweenData data, bool local = false)
            => MoveTo(target, targetPosition, data.Duration, data.Easing, local, data.Delay);

        /// <summary>
        /// Tweens a transform's rotation to the specified target rotation.
        /// </summary>
        public static TweenBase RotateTo(Transform target, Quaternion targetRotation, float duration,
            EEasingType easing = EEasingType.EaseOutQuad, bool local = false, float delay = 0f)
        {
            if (target == null)
                return null;

            Func<Quaternion> fromGetter;
            Action<Quaternion> setter;

            if (local)
            {
                fromGetter = () => target.localRotation;
                setter = v => target.localRotation = v;
            }
            else
            {
                fromGetter = () => target.rotation;
                setter = v => target.rotation = v;
            }

            Tween<Quaternion> tween = new(targetRotation,
                duration,
                setter,
                TweenLerpUtility.LerpQuaternionUnclamped,
                Easings.Get(easing),
                target,
                delay,
                fromGetter);

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a transform's rotation using structured tween data.
        /// </summary>
        public static TweenBase RotateTo(Transform target, Quaternion targetRotation, TweenData data,
            bool local = false) => RotateTo(target, targetRotation, data.Duration, data.Easing, local, data.Delay);

        /// <summary>
        /// Tweens a transform's rotation (as Euler angles) using structured tween data.
        /// </summary>
        public static TweenBase RotateTo(Transform target, Vector3 targetEuler, TweenData data, bool local = false)
        {
            Quaternion targetQuat = Quaternion.Euler(targetEuler);
            return RotateTo(target, targetQuat, data.Duration, data.Easing, local, data.Delay);
        }

        /// <summary>
        /// Tweens a <see cref="CanvasGroup"/> alpha to the target value.
        /// </summary>
        public static TweenBase FadeTo(CanvasGroup canvasGroup, float targetAlpha, float duration,
            EEasingType easing = EEasingType.Linear, float delay = 0f)
        {
            if (canvasGroup == null)
                return null;

            Tween<float> tween = new(targetAlpha,
                duration,
                setter: v => canvasGroup.alpha = v,
                TweenLerpUtility.LerpFloatUnclamped,
                Easings.Get(easing),
                canvasGroup,
                delay,
                fromGetter: () => canvasGroup.alpha);

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a <see cref="CanvasGroup"/> alpha using structured tween data.
        /// </summary>
        public static TweenBase FadeTo(CanvasGroup canvasGroup, FadeTweenData data) => FadeTo(canvasGroup,
            data.TargetAlpha, data.TweenData.Duration, data.TweenData.Easing,
            data.TweenData.Delay);

        /// <summary>
        /// Tweens the color of a UI <see cref="Graphic"/> to the target color.
        /// </summary>
        public static TweenBase ColorTo(Graphic graphic, Color targetColor, float duration,
            EEasingType easing = EEasingType.EaseOutQuad, float delay = 0f)
        {
            if (graphic == null)
                return null;

            Tween<Color> tween = new(targetColor,
                duration,
                setter: v => graphic.color = v,
                TweenLerpUtility.LerpColorUnclamped,
                Easings.Get(easing),
                graphic,
                delay,
                fromGetter: () => graphic.color);

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a UI <see cref="Graphic"/> color using structured tween data.
        /// </summary>
        public static TweenBase ColorTo(Graphic graphic, Color targetColor, TweenData data)
            => ColorTo(graphic, targetColor, data.Duration, data.Easing, data.Delay);

        /// <summary>
        /// Tweens a <see cref="SpriteRenderer"/> color to the target value.
        /// </summary>
        public static TweenBase ColorTo(SpriteRenderer renderer, Color targetColor, float duration,
            EEasingType easing = EEasingType.EaseOutQuad, float delay = 0f)
        {
            if (renderer == null)
                return null;

            Tween<Color> tween = new(targetColor,
                duration,
                setter: v => renderer.color = v,
                TweenLerpUtility.LerpColorUnclamped,
                Easings.Get(easing),
                renderer,
                delay,
                fromGetter: () => renderer.color);

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a <see cref="SpriteRenderer"/> color using structured tween data.
        /// </summary>
        public static TweenBase ColorTo(SpriteRenderer renderer, Color targetColor, TweenData data)
            => ColorTo(renderer, targetColor, data.Duration, data.Easing, data.Delay);

        /// <summary>
        /// Tweens a color value from its current value (via getter) to the target value.
        /// </summary>
        public static TweenBase ColorTo(Func<Color> fromGetter, Action<Color> setter, Color targetValue, float duration,
            EEasingType easing = EEasingType.Linear, float delay = 0f, Object targetObj = null)
        {
            if (setter == null)
                return null;

            Tween<Color> tween = new(targetValue,
                duration,
                setter,
                TweenLerpUtility.LerpColorUnclamped,
                Easings.Get(easing),
                targetObj,
                delay,
                fromGetter);

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a color value from its current value (via getter) to the target value.
        /// </summary>
        public static TweenBase ColorTo(Func<Color> fromGetter, Action<Color> setter, Color targetValue, TweenData data,
            Object targetObj = null) => ColorTo(fromGetter, setter, targetValue, data.Duration, data.Easing, data.Delay,
            targetObj);

        /// <summary>
        /// Creates a shake animation by applying randomized offsets to the transform's position.
        /// For UI elements, the motion is applied to <see cref="RectTransform.anchoredPosition"/>.
        /// </summary>
        public static TweenBase Shake(Transform target, float strength, float duration,
            EEasingType easing = EEasingType.EaseOutQuad, float delay = 0f)
        {
            if (target == null)
                return null;

            RectTransform rectTransform = target as RectTransform;
            bool isUI = rectTransform != null;
            Vector3 original = isUI
                ? rectTransform.anchoredPosition
                : target.localPosition;

            Tween<float> tween = new(1f,
                duration,
                setter: _ =>
                {
                    float angle = Random.value * Mathf.PI * 2f;
                    float offset = Random.value * strength;
                    Vector3 delta = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * offset;

                    if (isUI)
                        rectTransform!.anchoredPosition = new Vector2(original.x + delta.x, original.y + delta.y);
                    else
                        target.localPosition = original + delta;
                },
                TweenLerpUtility.LerpFloatUnclamped,
                Easings.Get(easing),
                target,
                delay,
                fromGetter: () => 0f);

            // Restore the original position whenever the shake ends (either naturally or via Stop).
            tween.OnKill += RestorePosition;

            tween.Start();

            return tween;

            void RestorePosition(TweenBase killedTween)
            {
                if (isUI)
                    rectTransform!.anchoredPosition = new Vector2(original.x, original.y);
                else
                    target.localPosition = original;

                killedTween.OnKill -= RestorePosition;
            }
        }

        /// <summary>
        /// Creates a shake animation using structured tween data.
        /// </summary>
        public static TweenBase Shake(Transform target, ShakeTweenData data) => Shake(target, data.Strength,
            data.TweenData.Duration, data.TweenData.Easing, data.TweenData.Delay);

        /// <summary>
        /// Tweens a float value from its current value (via getter) to the target value.
        /// </summary>
        public static TweenBase FadeFloatTo(Func<float> fromGetter, Action<float> setter, float targetValue,
            float duration, EEasingType easing = EEasingType.Linear, float delay = 0f, Object targetObj = null)
        {
            if (setter == null)
                return null;

            Tween<float> tween = new(targetValue,
                duration,
                setter,
                TweenLerpUtility.LerpFloatUnclamped,
                Easings.Get(easing),
                targetObj,
                delay,
                fromGetter);

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a float value from its current value (via getter) to the target value.
        /// </summary>
        public static TweenBase FadeFloatTo(Func<float> fromGetter, Action<float> setter, float targetValue,
            TweenData data, Object targetObj = null) => FadeFloatTo(fromGetter, setter, targetValue, data.Duration,
            data.Easing, data.Delay, targetObj);

        /// <summary>
        /// Tweens an int value from its current value (via getter) to the target value.
        /// </summary>
        /// <remarks>
        /// Internally tweens as float and rounds each update.
        /// </remarks>
        public static TweenBase FadeIntTo(Func<int> fromGetter, Action<int> setter, int targetValue, float duration,
            EEasingType easing = EEasingType.Linear, float delay = 0f, Object targetObj = null)
        {
            if (setter == null)
                return null;

            Tween<float> tween = new(targetValue,
                duration,
                setter: v => setter(Mathf.RoundToInt(v)),
                TweenLerpUtility.LerpFloatUnclamped,
                Easings.Get(easing),
                targetObj,
                delay,
                fromGetter: () => fromGetter?.Invoke() ?? 0);

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Convenience overload: fades an int with structured tween data.
        /// </summary>
        public static TweenBase FadeIntTo(Func<int> fromGetter, Action<int> setter, int targetValue, TweenData data,
            Object targetObj = null) => FadeIntTo(fromGetter, setter, targetValue, data.Duration, data.Easing,
            data.Delay, targetObj);
    }
}