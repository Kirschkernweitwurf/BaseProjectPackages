namespace Base.ServicesPackage.PlayTests
{
    /// <summary>
    /// A service that registers through the Unity lifecycle rather than through a direct call.
    /// </summary>
    /// <remarks>
    /// The edit mode suite has to use a <see cref="UnityEngine.ScriptableObject"/> probe, because a
    /// behaviour declared in an editor assembly cannot be attached to a game object. This one can,
    /// which is what makes <see cref="GameServiceBehaviour"/> testable at all.
    /// </remarks>
    internal sealed class ServiceBehaviourProbe : GameServiceBehaviour { }
}