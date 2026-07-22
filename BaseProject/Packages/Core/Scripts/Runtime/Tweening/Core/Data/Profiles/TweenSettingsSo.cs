using Base.ToolPackage.MenuManagerWindow;
using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data.Profiles
{
    /// <summary>
    /// Shared timing and looping asset. Assign it to any number of tweens or profiles to tune
    /// them all from a single place, for example "UI Snappy" or "Card Flip".
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Tweening/New TweenSettings", "TS_TweenSettings")]
    public sealed class TweenSettingsSo : ScriptableObject
    {
        [field: Tooltip("Duration, delay and easing shared by every user of this asset.")]
        [field: SerializeField] public TweenSettings Settings { get; private set; } = new();

        [field: Tooltip("Loop behavior shared by every user of this asset.")]
        [field: SerializeField] public LoopSettings Loop { get; private set; } = new();
    }
}