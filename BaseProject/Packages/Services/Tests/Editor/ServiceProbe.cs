namespace Base.ServicesPackage.Tests
{
    /// <summary>
    /// A service that is not a <see cref="UnityEngine.MonoBehaviour"/>, so the locator can be tested
    /// without a scene. The label tells two instances of it apart.
    /// </summary>
    public sealed class ServiceProbe : IGameService
    {
        /// <summary>Identifies which instance answered a lookup.</summary>
        public string Label { get; }

        /// <summary>Creates a probe under a label.</summary>
        /// <param name="label">The label that identifies this instance.</param>
        public ServiceProbe(string label) => Label = label;
    }
}