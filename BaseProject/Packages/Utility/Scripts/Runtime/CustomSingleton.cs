using UnityEngine;
using Base.UtilityPackage.Logging;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Generic singleton base class for creating singleton <see cref="MonoBehaviour"/> components.
    /// Inherit from this to make your component a singleton.
    /// </summary>
    public abstract class CustomSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; protected set; } //reset-ignore

        [SerializeField] private bool dontDestroyOnLoad;

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;

                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                CustomLogger.LogWarning($"Duplicate instance of {typeof(T)} detected. " +
                                        $"Destroying {gameObject.name}.", this);
                Destroy(gameObject);
            }
        }
    }
}