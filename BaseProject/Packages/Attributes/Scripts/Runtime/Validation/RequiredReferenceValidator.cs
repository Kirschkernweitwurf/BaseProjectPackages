using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.AttributePackage
{
    /// <summary>
    /// Scans loaded scenes on play-mode start and logs an error for every <see cref="RequiredAttribute"/>
    /// reference that is null, including references inside nested serializable types. Runs for the first
    /// scene and for every additively loaded scene. Editor and development builds only.
    /// </summary>
    public static class RequiredReferenceValidator
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static readonly List<MissingRequiredReference> Buffer = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                    Validate(behaviour);
            }
        }

        private static void Validate(MonoBehaviour behaviour)
        {
            Buffer.Clear();
            RequiredReferenceScanner.Collect(behaviour, Buffer);

            foreach (MissingRequiredReference missing in Buffer)
            {
                string message = missing.Message
                    ?? $"Required reference '{missing.Path}' of type {missing.FieldType.Name} is null.";

                CustomLogger.LogError(message, missing.Component);
            }
        }
#endif
    }
}