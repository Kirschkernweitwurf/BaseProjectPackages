using Base.SaveSystemPackage.Core;
using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Slots;
using Base.ServicePackage;
using Base.ServicePackage.Shutdown;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Composition
{
    /// <summary>
    /// Main entry point for the save system. Owns the one shared <see cref="ISavableRegistry"/>, so
    /// savables register with the same instance the system reads from, and the slot provider the UI
    /// uses. On shutdown it waits for any in-flight save before releasing the parts.
    /// </summary>
    public sealed class SaveManager : GameServiceBehaviour, IShutdownHandler
    {
        [SerializeField] private SaveSystemSettings settings = new();

        /// <summary>The read and write API. <c>null</c> once the manager has shut down.</summary>
        public ISaveSystem SaveSystem { get; private set; }

        /// <summary>Where savables register themselves.</summary>
        public ISavableRegistry Savables { get; private set; }

        /// <summary>Slot bookkeeping for the configured model.</summary>
        public ISaveSlotProvider Slots { get; private set; }

        /// <summary>The slot the player currently has selected.</summary>
        public SaveSlotSelection Selection { get; private set; }

        /// <inheritdoc/>
        public bool HasShutDown { get; private set; }

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            ShutdownManager.Register(this);

            SaveSystemBundle bundle = SaveSystemFactory.Create(settings);

            SaveSystem = bundle.System;
            Savables = bundle.Registry;
            Slots = bundle.Slots;
            Selection = bundle.Selection;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Shutdown();
        }
#endregion

        /// <inheritdoc/>
        public void Shutdown()
        {
            if (HasShutDown)
                return;

            HasShutDown = true;
            ShutdownManager.Deregister(this);

            _ = FlushAndReleaseAsync();
        }

        private async Awaitable FlushAndReleaseAsync()
        {
            // The references stay reachable until the flush is done. Clearing them first would leave
            // an in-flight save writing into a system nothing can reach or wait on any more.
            if (SaveSystem != null)
                await SaveSystem.FlushAsync();

            SaveSystem = null;
            Savables = null;
            Slots = null;
            Selection = null;
        }
    }
}