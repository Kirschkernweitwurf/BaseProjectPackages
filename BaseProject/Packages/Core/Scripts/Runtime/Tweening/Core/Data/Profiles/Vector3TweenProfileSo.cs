using Base.ToolPackage.MenuManagerWindow;
using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data.Profiles
{
    /// <summary>
    /// Profile asset for tweens working on a Vector3, such as position, rotation and scale.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Tweening/Profiles/New Vector3TweenProfile",
        "TPV_Vector3TweenProfile")]
    public sealed class Vector3TweenProfileSo : TweenValueProfileSo<Vector3> { }
}