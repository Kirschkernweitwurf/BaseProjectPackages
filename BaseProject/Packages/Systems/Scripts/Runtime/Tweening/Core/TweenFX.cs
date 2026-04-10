using System;
using Systems.Tweening.Core.Data;
using Systems.Tweening.Core.Data.Parameters;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
// ReSharper disable MemberCanBePrivate.Global

namespace Systems.Tweening.Core
{
    /// <summary>
    /// High-level tween factories for common Unity components.
    /// All methods create, start, and return a tween configured with appropriate interpolation.
    /// Supports both traditional parameter calls and structured data-based overloads.
    /// </summary>
    public static class TweenFX
    {
        #region --- ScaleTo ---

        /// <summary>
        /// Tweens a transform's local scale to the specified value.
        /// </summary>
        public static TweenBase ScaleTo(Transform target, Vector3 targetScale, float duration,
            EEasingType easing = EEasingType.EaseOutQuad, float delay = 0f)
        {
            if (target == null)
                return null;

            Tween<Vector3> tween = new(
                to: targetScale,
                duration: duration,
                setter: v => target.localScale = v,
                lerpFunc: TweenLerpUtility.LerpVector3Unclamped,
                ease: Easings.Get(easing),
                targetObj: target,
                delay: delay,
                fromGetter: () => target.localScale
            );

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a transform's local scale using structured tween data.
        /// </summary>
        public static TweenBase ScaleTo(Transform target, Vector3 targetScale, TweenData data)
        {
            return ScaleTo(target, targetScale, data.Duration, data.Easing, data.Delay);
        }

        #endregion

        #region --- MoveTo ---

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

            Tween<Vector3> tween = new(
                to: targetPosition,
                duration: duration,
                setter: setter,
                lerpFunc: TweenLerpUtility.LerpVector3Unclamped,
                ease: Easings.Get(easing),
                targetObj: target,
                delay: delay,
                fromGetter: fromGetter
            );

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a transform's position using structured tween data.
        /// </summary>
        public static TweenBase MoveTo(Transform target, Vector3 targetPosition, TweenData data, bool local = false)
        {
            return MoveTo(target, targetPosition, data.Duration, data.Easing, local, data.Delay);
        }

        #endregion

        #region --- RotateTo ---

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

            Tween<Quaternion> tween = new(
                to: targetRotation,
                duration: duration,
                setter: setter,
                lerpFunc: TweenLerpUtility.LerpQuaternionUnclamped,
                ease: Easings.Get(easing),
                targetObj: target,
                delay: delay,
                fromGetter: fromGetter
            );

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a transform's rotation using structured tween data.
        /// </summary>
        public static TweenBase RotateTo(Transform target, Quaternion targetRotation, TweenData data, bool local = false)
        {
            return RotateTo(target, targetRotation, data.Duration, data.Easing, local, data.Delay);
        }

        /// <summary>
        /// Tweens a transform's rotation using structured tween data.
        /// </summary>
        public static TweenBase RotateTo(Transform target, Vector3 targetRotation, TweenData data, bool local = false)
        {
            Quaternion targetQuat = Quaternion.Euler(targetRotation);
            return RotateTo(target, targetQuat, data.Duration, data.Easing, local, data.Delay);
        }

        #endregion

        #region --- FadeTo ---

        /// <summary>
        /// Tweens a <see cref="CanvasGroup"/> alpha to the target value.
        /// </summary>
        public static TweenBase FadeTo(CanvasGroup canvasGroup, float targetAlpha, float duration,
            EEasingType easing = EEasingType.Linear, float delay = 0f)
        {
            if (canvasGroup == null)
                return null;

            Tween<float> tween = new(
                to: targetAlpha,
                duration: duration,
                setter: v => canvasGroup.alpha = v,
                lerpFunc: TweenLerpUtility.LerpFloatUnclamped,
                ease: Easings.Get(easing),
                targetObj: canvasGroup,
                delay: delay,
                fromGetter: () => canvasGroup.alpha
            );

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a <see cref="CanvasGroup"/> alpha using structured tween data.
        /// </summary>
        public static TweenBase FadeTo(CanvasGroup canvasGroup, FadeTweenData data)
        {
            return FadeTo(canvasGroup, data.TargetAlpha, data.TweenData.Duration, data.TweenData.Easing,
                data.TweenData.Delay);
        }

        #endregion

        #region --- ColorTo ---

        /// <summary>
        /// Tweens the color of a UI <see cref="Graphic"/> to the target color.
        /// </summary>
        public static TweenBase ColorTo(Graphic graphic, Color targetColor, float duration,
            EEasingType easing = EEasingType.EaseOutQuad, float delay = 0f)
        {
            if (graphic == null)
                return null;

            Tween<Color> tween = new(
                to: targetColor,
                duration: duration,
                setter: v => graphic.color = v,
                lerpFunc: TweenLerpUtility.LerpColorUnclamped,
                ease: Easings.Get(easing),
                targetObj: graphic,
                delay: delay,
                fromGetter: () => graphic.color
            );

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a UI <see cref="Graphic"/> color using structured tween data.
        /// </summary>
        public static TweenBase ColorTo(Color targetColor, Graphic graphic, TweenData data)
        {
            return ColorTo(graphic, targetColor, data.Duration, data.Easing, data.Delay);
        }

        /// <summary>
        /// Tweens a <see cref="SpriteRenderer"/> color to the target value.
        /// </summary>
        public static TweenBase ColorTo(Color targetColor, SpriteRenderer renderer, float duration,
            EEasingType easing = EEasingType.EaseOutQuad, float delay = 0f)
        {
            if (renderer == null)
                return null;

            Tween<Color> tween = new(
                to: targetColor,
                duration: duration,
                setter: v => renderer.color = v,
                lerpFunc: TweenLerpUtility.LerpColorUnclamped,
                ease: Easings.Get(easing),
                targetObj: renderer,
                delay: delay,
                fromGetter: () => renderer.color
            );

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a <see cref="SpriteRenderer"/> color using structured tween data.
        /// </summary>
        public static TweenBase ColorTo(Color targetColor, SpriteRenderer renderer, TweenData data)
        {
            return ColorTo(targetColor, renderer, data.Duration, data.Easing, data.Delay);
        }

        /// <summary>
        /// Tweens a color value from its current value (via getter) to the target value.
        /// </summary>
        public static TweenBase ColorTo(Func<Color> fromGetter, Action<Color> setter, Color targetValue, float duration,
            EEasingType easing = EEasingType.Linear, float delay = 0f, Object targetObj = null)
        {
            if (setter == null)
                return null;

            Tween<Color> tween = new(
                to: targetValue,
                duration: duration,
                setter: setter,
                lerpFunc: TweenLerpUtility.LerpColorUnclamped,
                ease: Easings.Get(easing),
                targetObj: targetObj,
                delay: delay,
                fromGetter: fromGetter
            );

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a color value from its current value (via getter) to the target value.
        /// </summary>
        public static TweenBase ColorTo(Func<Color> fromGetter, Action<Color> setter, Color targetValue, TweenData data,
            Object targetObj = null)
        {
            return ColorTo(fromGetter, setter, targetValue, data.Duration, data.Easing, data.Delay, targetObj);
        }

        #endregion

        #region --- Shake ---

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
            Vector3 original = isUI ? rectTransform.anchoredPosition : target.localPosition;

            Tween<float> tween = new(
                to: 1f,
                duration: duration,
                setter: _ =>
                {
                    float angle = UnityEngine.Random.value * Mathf.PI * 2f;
                    float offset = UnityEngine.Random.value * strength;
                    Vector3 delta = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * offset;

                    if (isUI)
                        rectTransform!.anchoredPosition = new Vector2(original.x + delta.x, original.y + delta.y);
                    else
                        target.localPosition = original + delta;
                },
                lerpFunc: TweenLerpUtility.LerpFloatUnclamped,
                ease: Easings.Get(easing),
                targetObj: target,
                delay: delay,
                fromGetter: () => 0f
            );

            tween.OnComplete += SetPosition;

            tween.Start();

            return tween;

            void SetPosition(TweenBase completedTween)
            {
                if (isUI)
                    rectTransform!.anchoredPosition = new Vector2(original.x, original.y);
                else
                    target.localPosition = original;

                completedTween.OnComplete -= SetPosition;
            }
        }

        /// <summary>
        /// Creates a shake animation using structured tween data.
        /// </summary>
        public static TweenBase Shake(Transform target, ShakeTweenData data)
        {
            return Shake(target, data.Strength, data.TweenData.Duration, data.TweenData.Easing, data.TweenData.Delay);
        }

        #endregion

        #region --- Value Fade ---

        /// <summary>
        /// Tweens a float value from its current value (via getter) to the target value.
        /// </summary>
        public static TweenBase FadeFloatTo(Func<float> fromGetter, Action<float> setter, float targetValue,
            float duration, EEasingType easing = EEasingType.Linear, float delay = 0f, Object targetObj = null)
        {
            if (setter == null)
                return null;

            Tween<float> tween = new(
                to: targetValue,
                duration: duration,
                setter: setter,
                lerpFunc: TweenLerpUtility.LerpFloatUnclamped,
                ease: Easings.Get(easing),
                targetObj: targetObj,
                delay: delay,
                fromGetter: fromGetter
            );

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Tweens a float value from its current value (via getter) to the target value.
        /// </summary>
        public static TweenBase FadeFloatTo(Func<float> fromGetter, Action<float> setter, float targetValue,
            TweenData data, Object targetObj = null)
        {
            return FadeFloatTo(fromGetter, setter, targetValue, data.Duration, data.Easing, data.Delay, targetObj);
        }

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

            Tween<float> tween = new(
                to: targetValue,
                duration: duration,
                setter: v => setter(Mathf.RoundToInt(v)),
                lerpFunc: TweenLerpUtility.LerpFloatUnclamped,
                ease: Easings.Get(easing),
                targetObj: targetObj,
                delay: delay,
                fromGetter: () => fromGetter?.Invoke() ?? 0
            );

            tween.Start();
            return tween;
        }

        /// <summary>
        /// Convenience overload: fades an int with structured tween data.
        /// </summary>
        public static TweenBase FadeIntTo(Func<int> fromGetter, Action<int> setter, int targetValue, TweenData data,
            Object targetObj = null)
        {
            return FadeIntTo(fromGetter, setter, targetValue, data.Duration, data.Easing, data.Delay, targetObj);
        }

        #endregion
    }
}