using UnityEngine;

namespace Attributes
{
    /// <summary>
    /// Attribute to show a dropdown of all scenes included in the Build Settings in the inspector.
    /// </summary>
    /// <example>
    /// <code>
    /// public class Example : MonoBehaviour
    /// {
    ///     [SceneName]
    ///     public string sceneName;
    /// }
    /// </code>
    /// </example>
    public class SceneNameAttribute : PropertyAttribute { }
}