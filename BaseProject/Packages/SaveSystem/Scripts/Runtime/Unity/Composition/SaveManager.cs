using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Slots;
using Base.SystemsCorePackage.Services;
using Base.SystemsCorePackage.Services.Shutdown;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Composition
{
    /// <summary>
    /// Main entry point for the save system. Owns the one shared <see cref="ISavableRegistry"/>
    /// (so savables register with the same instance the system reads from) and the slot provider
    /// the UI uses. On shutdown, it waits for any in-flight save before tearing down.
    /// </summary>
    public class SaveManager : GameServiceBehaviour, IShutdownHandler
    {
        [SerializeField] private SaveSystemSettings settings = new();

        public ISaveSystem SaveSystem { get; private set; }
        public ISavableRegistry Savables { get; private set; }
        public ISaveSlotProvider Slots { get; private set; }
        public SaveSlotSelection Selection { get; private set; }

        public bool HasShutDown { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            ShutdownManager.Register(this);

            Bundle bundle = SaveSystemFactory.Create(settings, migrations: null);

            SaveSystem = bundle.System;
            Savables = bundle.Registry;
            Slots = bundle.Slots;
            Selection = bundle.Selection;

            Debug.Log($"SaveSystem ready. Model: {settings.SlotModel}. " +
                             $"Encrypt-on-write: {settings.ShouldEncryptOnWrite()}.", this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!HasShutDown)
                Shutdown();
        }

        public void Shutdown()
        {
            ShutdownManager.Deregister(this);
            HasShutDown = true;

            _ = FlushAndClearAsync();
        }

        private async Awaitable FlushAndClearAsync()
        {
            ISaveSystem system = SaveSystem;
            SaveSystem = null;
            Savables = null;
            Slots = null;
            Selection = null;

            if (system != null)
                await system.FlushAsync();
        }
    }
}