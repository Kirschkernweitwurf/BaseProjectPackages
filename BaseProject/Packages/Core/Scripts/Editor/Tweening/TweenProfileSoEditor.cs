using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEditor;

namespace Base.CorePackage.Editor.Tweening
{
    /// <summary>
    /// Inspector for every tween profile asset. Hides the inline timing while a settings asset is
    /// assigned.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TweenProfileSo), true)]
    public sealed class TweenProfileSoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI() => TweenInspectorLayout.Draw(serializedObject);
    }
}