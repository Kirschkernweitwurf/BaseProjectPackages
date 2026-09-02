using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Scans loaded scenes on play-mode start and logs an error for every validation issue found by the
    /// registered <see cref="IValidationRule"/> set, including issues inside nested serializable types.
    /// Runs for the first scene and for every additively loaded scene. Editor and development builds only.
    /// </summary>
    internal static class RequiredReferenceValidator
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static readonly List<ReferenceIssue> Buffer = new();

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
                foreach (MonoBehaviour behavior in root.GetComponentsInChildren<MonoBehaviour>(true))
                    Validate(behavior);
            }
        }

        private static void Validate(MonoBehaviour behavior)
        {
            Buffer.Clear();
            ReferenceValidationScanner.Collect(behavior, Buffer);

            foreach (ReferenceIssue issue in Buffer)
                CustomLogger.LogError(ValidationLog.Build(issue), issue.Owner);
        }
#endif
    }
}