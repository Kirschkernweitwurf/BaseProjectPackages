using Base.ToolPackage.MenuManagerWindow;

namespace Base.CorePackage.Tweening.Core.Data.Profiles
{
    /// <summary>
    /// Profile asset for tweens working on a single float, such as fades and fill amounts.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Tweening/Profiles/New FloatTweenProfile", "TPF_FloatTweenProfile")]
    public sealed class FloatTweenProfileSo : TweenValueProfileSo<float> { }
}