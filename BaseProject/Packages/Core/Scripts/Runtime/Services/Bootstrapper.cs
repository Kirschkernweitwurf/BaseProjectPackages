using System.Linq;
using Base.AttributePackage;
using Base.UtilityPackage;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.CorePackage.Services
{
    /// <summary>
    /// Bootstrapper class to initialize persistent and scene-specific managers.
    /// </summary>
    [DefaultExecutionOrder(-99)]
    public class Bootstrapper : MonoBehaviour
    {
        private static bool _persistentLoaded;
        [Header("Prefabs to Load")]

        [Required] [SerializeField] private GameObject persistentManagerPrefab;
        [Required] [SerializeField] private GameObject sceneManagerPrefab;
        [Required] [SerializeField] private GameObject gameplayManagerPrefab;

        [Header("Scene Filtering")]

        [NotNullOrEmpty] [SceneName] [SerializeField] private string[] gameplayScenes;

#region Unity Callbacks
        private void Awake()
        {
            // Load persistent managers only once
            if (!_persistentLoaded)
            {
                InstantiationUtility.CleanInstantiate(persistentManagerPrefab, dontDestroy: true);
                _persistentLoaded = true;
            }

            // Load scene managers for every scene
            InstantiationUtility.CleanInstantiate(sceneManagerPrefab, transform);

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
                InstantiationUtility.CleanInstantiate(gameplayManagerPrefab, transform);
        }
#endregion

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics() => _persistentLoaded = false;
#endif
    }
}