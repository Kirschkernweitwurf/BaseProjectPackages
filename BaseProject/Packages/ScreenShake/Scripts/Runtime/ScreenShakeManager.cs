using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.ScreenShakePackage
{
    /// <summary>
    /// Manages screen shake events and notifies registered listeners.
    /// </summary>
    public static class ScreenShakeManager
    {
        private static readonly List<Action<ScreenShakeProfile>> Listeners = new();

        /// <summary>
        /// Registers a listener to be notified of screen shake events.
        /// </summary>
        /// <param name="callback">The listener callback to register.</param>
        public static void RegisterListener(Action<ScreenShakeProfile> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (!Listeners.Contains(callback))
                Listeners.Add(callback);
        }

        /// <summary>
        /// Unregisters a previously registered listener.
        /// </summary>
        /// <param name="callback">The listener callback to unregister.</param>
        public static void DeregisterListener(Action<ScreenShakeProfile> callback) => Listeners.Remove(callback);

        /// <summary>
        /// Notifies all registered listeners of a screen shake event with the given profile.
        /// </summary>
        /// <param name="profile">The screen shake profile to notify listeners with.</param>
        public static void NotifyShake(ScreenShakeProfile profile)
        {
            for (int i = Listeners.Count - 1; i >= 0; i--)
                Listeners[i].Invoke(profile);
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics() => Listeners.Clear();
#endif
    }
}