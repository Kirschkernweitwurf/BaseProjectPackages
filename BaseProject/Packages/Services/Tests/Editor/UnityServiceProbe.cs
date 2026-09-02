using UnityEngine;

namespace Base.ServicesPackage.Tests
{
    /// <summary>
    /// A service that is a Unity object, so destroying it produces the reference Unity reports as
    /// null while the locator still holds the entry.
    /// </summary>
    /// <remarks>
    /// A <see cref="ScriptableObject"/> rather than a <see cref="MonoBehaviour"/>, because a behaviour
    /// declared in an editor assembly cannot be attached to a game object at all. Both are a
    /// <see cref="Object"/>, which is the only thing the liveness check looks at, so the destroyed
    /// reference behaves the same either way.
    /// </remarks>
    public sealed class UnityServiceProbe : ScriptableObject, IGameService { }
}