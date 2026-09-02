using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Inspectors
{
    /// <summary>
    /// Applies the Attributes package inspector to all ScriptableObject types without an own editor.
    /// </summary>
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    internal sealed class ScriptableObjectAttributeEditor : AttributesPackageEditor { }
}