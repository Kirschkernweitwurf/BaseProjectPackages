using Base.CorePackage.Tweening.Core;
using UnityEditor;

namespace Base.CorePackage.Editor.Tweening
{
    /// <summary>
    /// Inspector for every tween component. Hides the fields that an assigned profile or settings
    /// asset already provides.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TweenBehaviourBase), true)]
    public sealed class TweenBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI() => TweenInspectorLayout.Draw(serializedObject);
    }
}