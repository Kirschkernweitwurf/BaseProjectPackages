using Base.ToolPackage.MenuManagerWindow;
using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data.Profiles
{
    /// <summary>
    /// Profile asset for tweens working on a Color, such as image, text and sprite renderer tints.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Tweening/Profiles/New ColorTweenProfile", "TPC_ColorTweenProfile")]
    public sealed class ColorTweenProfileSo : TweenValueProfileSo<Color> { }
}