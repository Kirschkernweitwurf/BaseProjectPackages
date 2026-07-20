using Base.AttributePackage.Editor;
using Base.CorePackage.Tweening.Core;
using UnityEditor;

namespace Base.CorePackage.Editor.Tweening
{
    /// <summary>
    /// Inspector for every tween component. Hides the fields that an assigned profile or settings
    /// asset already provides. Derives from <see cref="AttributePackageEditor"/> so the attribute
    /// pipeline (handlers, inline widgets, [GetComponent]) runs for tween components as well.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TweenBehaviourBase), true)]
    public sealed class TweenBehaviourEditor : AttributePackageEditor
    {
        public override void OnInspectorGUI() => TweenInspectorLayout.Draw(this);
    }
}