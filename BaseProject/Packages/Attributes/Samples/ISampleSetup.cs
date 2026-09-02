namespace Base.AttributesPackage.Samples
{
    /// <summary>
    /// Implemented by a component sample that needs a hierarchy around it before it means anything.
    /// </summary>
    /// <remarks>
    /// A sample for a same-object getter needs nothing: <c>RequireComponent</c> puts what it looks for on
    /// the sample itself. A sample for a parent or child getter cannot say what it needs that way, so it
    /// builds it here and the reference pane calls this straight after adding the component.
    /// <para>
    /// The method runs on the throwaway preview object and on the copy the reader creates in the scene,
    /// so it must build the hierarchy and nothing else. Anything it creates under or above the sample is
    /// destroyed with it.
    /// </para>
    /// </remarks>
    internal interface ISampleSetup
    {
        /// <summary>Builds the parents and children this sample needs.</summary>
        void BuildSample();
    }
}