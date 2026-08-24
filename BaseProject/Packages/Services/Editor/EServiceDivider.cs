namespace Base.ServicePackage.Editor
{
    /// <summary>
    /// The draggable lines between the resizable columns of the service table.
    /// </summary>
    internal enum EServiceDivider : byte
    {
        /// <summary>The line between the Instance and Location columns.</summary>
        InstanceLocation = 0,

        /// <summary>No line is currently being dragged.</summary>
        None = 1,

        /// <summary>The line between the Service and Instance columns.</summary>
        ServiceInstance = 2
    }
}