using System.Linq;
using Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems.Services
{
    /// <summary>
    /// Bootstrapper class to initialize persistent and scene-specific managers.
    /// </summary>
    [DefaultExecutionOrder(-99)]
    public class Bootstrapper : MonoBehaviour
    {
        private static bool _persistentLoaded;

        [Header("Prefabs to Load")]
        [SerializeField] private GameObject persistentPrefab;
        [SerializeField] private GameObject gameplayPrefab;

        [Header("Scene Filtering")]
        [SceneName, SerializeField] private string[] gameplayScenes;

        private void Awake()
        {
            // Load persistent managers only once
            if (!_persistentLoaded)
            {
                CleanInstantiate(persistentPrefab, true);
                _persistentLoaded = true;
            }

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

            if (!isGameplaySceneLoaded)
                return;

            CleanInstantiate(gameplayPrefab);
        }

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

            instance.name = instance.name.Replace("(Clone)", "");
        }
    }
}