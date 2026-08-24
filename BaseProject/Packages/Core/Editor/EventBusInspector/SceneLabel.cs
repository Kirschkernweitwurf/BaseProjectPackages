using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// Names a Unity object the way this window names one everywhere: a component by the game object
    /// and scene it sits in, anything else by its own name.
    /// </summary>
    internal static class SceneLabel
    {
        private const string SceneFormat = "{0} ({1})";

        /// <summary>
        /// Describes a Unity object for a table cell or a dropdown entry.
        /// </summary>
        /// <param name="instance">The object to name. Must not be destroyed.</param>
        /// <returns>The label to show.</returns>
        /// <remarks>
        /// A destroyed component throws on <see cref="Component.gameObject"/>, so the caller has to
        /// establish that the object is alive before asking for a label.
        /// </remarks>
        internal static string Describe(Object instance)
        {
            if (instance is not Component component)
                return instance.name;

            GameObject host = component.gameObject;
            string scene = host.scene.name;

            return string.IsNullOrEmpty(scene)
                ? host.name
                : string.Format(SceneFormat, host.name, scene);
        }
    }
}