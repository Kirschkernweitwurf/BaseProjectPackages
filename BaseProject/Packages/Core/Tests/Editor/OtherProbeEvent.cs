using Base.CorePackage.EventBus;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// A second event type, so a test can prove that a handler only hears the type it subscribed to.
    /// </summary>
    internal readonly struct OtherProbeEvent : IEvent { }
}