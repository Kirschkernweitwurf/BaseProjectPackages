using System.Linq;
using Base.AttributePackage;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.SystemsCorePackage.Services
{
    /// <summary>
    /// Bootstrapper class to initialize persistent and scene-specific managers.
    /// </summary>
    [DefaultExecutionOrder(-99)]
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Prefabs to Load")]

        [SerializeField] private GameObject persistentManagerPrefab;
        [SerializeField] private GameObject sceneManagerPrefab;
        [SerializeField] private GameObject gameplayManagerPrefab;

        [Header("Scene Filtering")]

        [SceneName] [SerializeField] private string[] gameplayScenes;

        private static bool _persistentLoaded;

#region Unity Callbacks
        private void Awake()
        {
            // Load persistent managers only once
            if (!_persistentLoaded)
            {
                CleanInstantiate(persistentManagerPrefab, true);
                _persistentLoaded = true;
            }

            // Load scene managers for every scene
            CleanInstantiate(sceneManagerPrefab);

            // Load gameplay managers for gameplay scenes only
            bool isGameplaySceneLoaded = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!gameplayScenes.Contains(scene.name))
                    continue;

                isGameplaySceneLoaded = true;
                break;
            }

            if (isGameplaySceneLoaded)
                CleanInstantiate(gameplayManagerPrefab);
        }
#endregion

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics() => _persistentLoaded = false;
#endif

        /// <summary>
        /// Instantiates a prefab and optionally marks it to not be destroyed on load.
        /// Also cleans up the name by removing "(Clone)".
        /// </summary>
        private void CleanInstantiate(GameObject prefab, bool dontDestroy = false)
        {
            if (!prefab)
                return;

            GameObject instance = Instantiate(prefab);
            if (dontDestroy)
                DontDestroyOnLoad(instance);
            else
                instance.transform.SetParent(transform);

            instance.name = instance.name.Replace("(Clone)", string.Empty);
        }
    }
}