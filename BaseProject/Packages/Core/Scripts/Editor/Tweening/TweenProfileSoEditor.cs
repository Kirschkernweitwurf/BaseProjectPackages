using Base.AttributePackage.Editor;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEditor;

namespace Base.CorePackage.Editor.Tweening
{
    /// <summary>
    /// Inspector for every tween profile asset. Hides the inline timing while a settings asset is
    /// assigned. Derives from <see cref="AttributePackageEditor"/> so the attribute pipeline runs.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TweenProfileSo), true)]
    public sealed class TweenProfileSoEditor : AttributePackageEditor
    {
        public override void OnInspectorGUI() => TweenInspectorLayout.Draw(this);
    }
}