using UnityEngine;

namespace Attributes
{
    /// <summary>
    /// Attribute to show a dropdown of all available Input Action Maps in the inspector.
    /// </summary>
    /// <example>
    /// <code>
    /// public class Example : MonoBehaviour
    /// {
    ///     [InputActionMapName]
    ///     public string actionMapName;
    /// }
    /// </code>
    /// </example>
    public class InputActionMapNameAttribute : PropertyAttribute { }
}