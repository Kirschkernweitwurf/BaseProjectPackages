namespace Base.ServicePackage.Shutdown
{
    /// <summary>
    /// Interface for handling shutdown procedures.
    /// <para/>
    /// Classes implementing this interface can register with the <see cref="ShutdownManager"/>
    /// to perform cleanup tasks when the application is quitting.
    /// </summary>
    public interface IShutdownHandler
    {
        /// <summary>
        /// Whether <see cref="Shutdown"/> already ran. Guards against running the cleanup twice when
        /// the handler is also torn down through <c>OnDestroy</c>.
        /// </summary>
        bool HasShutDown { get; }

        /// <summary>
        /// Method to be called during application shutdown. <br/>
        /// Precedes the destruction of game objects. <br/>
        /// Implement cleanup logic here.
        /// </summary>
        void Shutdown();
    }
}