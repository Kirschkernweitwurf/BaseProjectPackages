using Base.SystemsCorePackage.Services;
using Base.SystemsCorePackage.Services.Shutdown;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// The main entry point for the save system.
    /// </summary>
    public class SaveManager : GameServiceBehaviour, IShutdownHandler
    {
        [SerializeField] private SaveSystemSettings settings = new();

        public ISaveSystem SaveSystem { get; private set; }

        public bool HasShutDown { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            ShutdownManager.Register(this);

            SaveSystem = SaveSystemFactory.Create(settings);
            CustomLogger.Log($"SaveSystem ready. Encrypt-on-write: {settings.ShouldEncryptOnWrite()}.", this);
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
            SaveSystem = null;
        }
    }
}