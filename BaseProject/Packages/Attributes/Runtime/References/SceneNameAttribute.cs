using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a dropdown of all scenes included in the Build Settings. On a string field it stores the
    /// scene name, on an int field the build index.
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
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SceneNameAttribute : PropertyAttribute { }
}