using Base.AttributesPackage.Editor.Inspectors;
using Base.TweeningPackage.Core;
using UnityEditor;

namespace Base.TweeningPackage.Editor
{
    /// <summary>
    /// Inspector for every tween component. Hides the fields that an assigned profile or settings
    /// asset already provides. Derives from <see cref="AttributesPackageEditor"/> so the attribute
    /// pipeline (handlers, inline widgets, [GetComponent]) runs for tween components as well.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TweenBehaviourBase), true)]
    internal sealed class TweenBehaviourEditor : AttributesPackageEditor
    {
        public override void OnInspectorGUI() => TweenInspectorLayout.Draw(this);
    }
}