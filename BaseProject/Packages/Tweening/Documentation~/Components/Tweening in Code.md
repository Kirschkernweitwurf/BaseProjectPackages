---
type: Guide
package: Tweening
tags:
  - programmer
  - animation
summary: Tweens, timelines and effects straight from code, as a replacement for DOTween.
---
# Tweening in Code

📖 **Guide** · programmers only

**Tags:** #programmer #animation

Tween any component from code in one line, chain its options, and combine tweens on a timeline. Covers what projects used DOTween for, so new code needs no paid plugin. A [Tween Runner](Tween%20Runner.md) in the scene moves every tween.

## One tween

```csharp
using Base.TweeningPackage.Core;
using Base.TweeningPackage.Core.Data;

rect.TweenAnchorPos(target, 0.3f)
    .SetEase(EEasingType.EaseOutCubic)
    .SetDelay(0.1f)
    .SetLink(gameObject);
```

Every shortcut starts right away, from the value the component has when the tween starts moving (after its delay). It eases with `EaseOutQuad` until set otherwise.

| Shortcut | On | Tweens |
| --- | --- | --- |
| `TweenMove`, `TweenLocalMove` | Transform | World or local position |
| `TweenScale` | Transform | Local scale, a vector or one number for every axis |
| `TweenRotate`, `TweenLocalRotate` | Transform | Rotation to euler angles or a quaternion, the shortest way |
| `TweenPunchScale`, `TweenPunchPosition`, `TweenPunchRotation` | Transform | A push that swings back exactly to where it started |
| `TweenJump`, `TweenLocalJump` | Transform | Hops to a position |
| `TweenLocalPath` | Transform | Through a list of points, straight or as a curve |
| `TweenAnchorPos`, `TweenAnchorPosX`, `TweenAnchorPosY`, `TweenSizeDelta` | RectTransform | Layout values |
| `TweenFade` | CanvasGroup, Graphic, SpriteRenderer | Alpha. Texts are Graphics too. |
| `TweenColor` | Graphic, SpriteRenderer | Color |

Anything else goes through `Tweens.To(getter, setter, to, duration)` for float, Vector2, Vector3 and Color, or `Tweens.Value(from, to, duration, setter)` for a plain number.

## Options

Chain them in any order, right after creating the tween.

| Option | What it does |
| --- | --- |
| `SetEase(type)` | Shapes the motion. `SetEase(EEasingType.EaseOutBack, 2.2f)` sets how far the back easings overshoot. A `Func<float, float>` works too. |
| `SetDelay(seconds)` | Waits before moving. Counts once, not per loop. |
| `SetLoops(count, type)` | Repeats. `-1` is endless. `PingPong` counts each direction as one loop, `Continue` starts each loop where the last ended. |
| `SetUnscaledTime(false)` | Runs on scaled time, so it stops while `Time.timeScale` is 0. Tweens run on unscaled time by default. |
| `SetLink(gameObject)` | Stops the tween quietly once that GameObject is destroyed. |
| `SetTarget(owner)` | Lets `Tweens.Kill(owner)` find it. The tweened component is the target by default. |
| `WhenComplete(action)`, `WhenKilled(action)` | Callbacks for the end. Killed also fires when the tween is stopped early. |

Stop a tween with `tween.Stop()`, or snap it to its end with `tween.Stop(complete: true)`. `Tweens.Kill(target)` or `component.KillTweens()` stops every tween of a target.

## Timelines

A timeline plays several tweens and callbacks as one tween.

```csharp
TweenTimeline pop = Tweens.Timeline()
    .Append(card.TweenScale(1.1f, 0.15f))
    .Join(group.TweenFade(1f, 0.15f))
    .Append(card.TweenScale(1f, 0.2f).SetEase(EEasingType.EaseOutBack))
    .AppendCallback(OnPopped)
    .SetLink(gameObject);
```

| Call | Places |
| --- | --- |
| `Append(tween)` | After everything placed so far |
| `Join(tween)` | At the start of the tween placed last, so both run together |
| `Insert(time, tween)` | At a fixed time |
| `Prepend(tween)` | Before everything, which moves back |
| `AppendInterval(seconds)`, `PrependInterval(seconds)` | A pause |
| `AppendCallback(action)`, `InsertCallback(time, action)` | A call |

- A tween on a timeline belongs to it. Its start value is read when the timeline reaches it, so a second scale tween starts where the first one ended.
- Set a tween's options before placing it: its length is measured when it is placed.
- A tween with endless loops has no end and cannot be placed. Loop the timeline instead: `SetLoops(-1, ELoopType.PingPong)` plays it forth and back for a breathing pulse.
- `Tweens.Delay(seconds, action)` is a timeline with just a pause and a call.

## Coming from DOTween

| DOTween | Base Tweening |
| --- | --- |
| `DOScale`, `DOFade`, `DOAnchorPos`, ... | `TweenScale`, `TweenFade`, `TweenAnchorPos`, ... |
| `DOTween.Sequence()` | `Tweens.Timeline()` |
| `SetUpdate(true)` | Nothing: unscaled is the default. `SetUnscaledTime(false)` for scaled. |
| `OnComplete(...)`, `OnKill(...)` | `WhenComplete(...)`, `WhenKilled(...)` |
| `Kill()`, `Kill(true)` | `Stop()`, `Stop(complete: true)` |
| `DOTween.Kill(target)`, `transform.DOKill()` | `Tweens.Kill(target)`, `transform.KillTweens()` |
| `DOTween.To(...)` | `Tweens.To(...)` |
| `DOVirtual.DelayedCall(...)` | `Tweens.Delay(...)` |
| `LoopType.Yoyo`, `LoopType.Incremental` | `ELoopType.PingPong`, `ELoopType.Continue` |
| `Ease.OutCubic`, `Ease.InOutSine`, ... | `EEasingType.EaseOutCubic`, `EEasingType.EaseInOutSine`, ... |
| `PathType.CatmullRom` | `TweenLocalPath(path, duration, isCurved: true)` |

Punch vibrato means swings per second in both, so ported values keep their feel.

## Related

- [📖 Tweening](Tweening.md)
- [🗞️ Tween Runner](Tween%20Runner.md)
