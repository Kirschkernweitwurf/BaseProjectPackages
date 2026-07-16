using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data.Profiles
{
    /// <summary>
    /// Base class for profiles that drive a tween of the given value type. Derive a concrete, non
    /// generic asset type from this for every value type that should be authorable as an asset.
    /// </summary>
    /// <typeparam name="T">The value type being tweened.</typeparam>
    public abstract class TweenValueProfileSo<T> : TweenProfileSo
    {
        [field: Tooltip("Value the tween starts from. Ignored by 'To' tweens, which always start "
                        + "at the value captured on Awake.")]
        [field: SerializeField] public T StartValue { get; private set; }

        [field: Tooltip("Value the tween moves to.")]
        [field: SerializeField] public T TargetValue { get; private set; }
    }
}