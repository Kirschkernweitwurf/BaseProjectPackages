using System.Collections.Generic;
using Systems.Services;
using UnityEngine;

namespace Systems.Shutdown
{
    /// <summary>
    /// Manages the orderly shutdown of registered classes when the application is quitting,
    /// ensuring that each service has the opportunity to perform the necessary cleanup.
    /// <para/>
    /// Listens to <see cref="Application.quitting"/> event to trigger shutdown, which executes
    /// before any game objects are destroyed.
    /// <para/>
    /// Classes must implement <see cref="IShutdownHandler"/> to register with this manager.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ShutdownManager : GameServiceBehaviour
    {
        private static readonly List<IShutdownHandler> ShutdownHandlers = new();
        private static bool _isShuttingDown;

        protected override void Awake()
        {
            base.Awake();

            Application.quitting += ExecuteShutdown;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Application.quitting -= ExecuteShutdown;
        }

        /// <summary>
        /// Adds a handler to the shutdown list if it implements <see cref="IShutdownHandler"/>.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        public static void Register(IShutdownHandler handler)
        {
            if (handler == null || ShutdownHandlers.Contains(handler))
                return;

            ShutdownHandlers.Add(handler);
        }

        /// <summary>
        /// Removes a handler from the shutdown list.
        /// </summary>
        /// <param name="handler">The handler to deregister.</param>
        public static void Deregister(IShutdownHandler handler) => ShutdownHandlers.Remove(handler);

        /// <summary>
        /// Calls <see cref="IShutdownHandler.Shutdown"/> on all registered handlers in reverse order of registration.
        /// </summary>
        private static void ExecuteShutdown()
        {
            if (_isShuttingDown)
                return;

            _isShuttingDown = true;

            // Reverse order for dependency safety
            for (int i = ShutdownHandlers.Count - 1; i >= 0; i--)
            {
                IShutdownHandler handler = ShutdownHandlers[i];
                if (handler == null || handler.HasShutDown)
                    continue;

                handler.Shutdown();
            }

            ShutdownHandlers.Clear();
        }
    }
}