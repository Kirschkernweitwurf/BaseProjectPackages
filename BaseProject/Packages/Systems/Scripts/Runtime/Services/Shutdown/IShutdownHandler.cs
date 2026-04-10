namespace Systems.Shutdown
{
    /// <summary>
    /// Interface for handling shutdown procedures.
    /// <para/>
    /// Classes implementing this interface can register with the <see cref="ShutdownManager"/>
    /// to perform cleanup tasks when the application is quitting.
    /// </summary>
    public interface IShutdownHandler
    {
        bool HasShutDown { get; }

        /// <summary>
        /// Method to be called during application shutdown. <br/>
        /// Precedes the destruction of game objects. <br/>
        /// Implement cleanup logic here.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Called when the handler is being registered.
        /// </summary>
        void Register() => ShutdownManager.Register(this);

        /// <summary>
        /// Called when the handler is being deregistered.
        /// </summary>
        void Deregister() => ShutdownManager.Deregister(this);
    }
}