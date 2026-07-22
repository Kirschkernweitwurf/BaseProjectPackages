using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Generic singleton base class for creating singleton <see cref="MonoBehaviour"/> components.
    /// Inherit from this to make your component a singleton.
    /// </summary>
    public abstract class CustomSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad;

        public static T Instance { get; protected set; } //reset-ignore

#region Unity Callbacks
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
                CustomLogger.LogWarning(
                    $"Duplicate instance of {typeof(T).Name} detected. Destroying {gameObject.name}.", this);

                Destroy(gameObject);
            }
        }
#endregion
    }
}