using Base.AttributesPackage.Editor.Inspectors;
using Base.TweeningPackage.Core.Data.Profiles;
using UnityEditor;

namespace Base.TweeningPackage.Editor
{
    /// <summary>
    /// Inspector for every tween profile asset. Hides the inline timing while a settings asset is
    /// assigned. Derives from <see cref="AttributesPackageEditor"/> so the attribute pipeline runs.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TweenProfileSo), true)]
    internal sealed class TweenProfileSoEditor : AttributesPackageEditor
    {
        public override void OnInspectorGUI() => TweenInspectorLayout.Draw(this);
    }
}