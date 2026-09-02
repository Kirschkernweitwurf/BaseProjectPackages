using Base.AttributesPackage;
using Base.UtilityPackage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.ServicesPackage
{
    /// <summary>
    /// Instantiates the manager prefabs a scene needs: persistent managers once per session, scene managers
    /// for every scene, and gameplay managers only while one of the configured gameplay scenes is loaded.
    /// </summary>
    [DefaultExecutionOrder(-99)]
    public class Bootstrapper : MonoBehaviour
    {
        private static bool _persistentLoaded;

        [Title("Prefabs to Load")]
        [Required] [SerializeField] private GameObject persistentManagerPrefab;
        [Required] [SerializeField] private GameObject sceneManagerPrefab;
        [Required] [SerializeField] private GameObject gameplayManagerPrefab;

        [Title("Scene Filtering")]
        [NotNullOrEmpty] [SceneName] [SerializeField] private string[] gameplayScenes;

#region Unity Callbacks
        private void Awake()
        {
            LoadPersistentManagers();

            InstantiationUtility.CleanInstantiate(sceneManagerPrefab, transform);

            if (IsGameplaySceneLoaded())
                InstantiationUtility.CleanInstantiate(gameplayManagerPrefab, transform);
        }
#endregion

        // Static state survives a disabled domain reload, so it is cleared before every run
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _persistentLoaded = false;

        /// <summary>
        /// Instantiates the persistent managers once per play session and keeps them across scene loads.
        /// </summary>
        private void LoadPersistentManagers()
        {
            if (_persistentLoaded)
                return;

            InstantiationUtility.CleanInstantiate(persistentManagerPrefab, dontDestroy: true);
            _persistentLoaded = true;
        }

        /// <summary>
        /// Checks whether any of the configured gameplay scenes is currently loaded.
        /// </summary>
        /// <returns><c>true</c> if a gameplay scene is loaded; otherwise, <c>false</c>.</returns>
        private bool IsGameplaySceneLoaded()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                string loadedScene = SceneManager.GetSceneAt(i).name;

                foreach (string gameplayScene in gameplayScenes)
                {
                    if (gameplayScene == loadedScene)
                        return true;
                }
            }

            return false;
        }
    }
}