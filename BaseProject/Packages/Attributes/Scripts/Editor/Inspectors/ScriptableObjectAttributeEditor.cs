using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Inspectors
{
    /// <summary>
    /// Applies the attribute package inspector to all ScriptableObject types without an own editor.
    /// </summary>
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public sealed class ScriptableObjectAttributeEditor : AttributePackageEditor { }
}