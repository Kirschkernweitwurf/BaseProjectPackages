using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Applies the attribute package inspector to all MonoBehaviour types without an own editor.
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public sealed class MonoBehaviourAttributeEditor : AttributePackageEditor { }
}