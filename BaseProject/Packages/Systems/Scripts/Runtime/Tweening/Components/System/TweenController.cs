using System.Collections.Generic;
using Systems.Services;
using Systems.Tweening.Core;

namespace Systems.Tweening.Components.System
{
    /// <summary>
    /// Generic base controller that tracks active tweens per target.
    /// Provides registration and killing of tweens for derived controllers.
    /// </summary>
    /// <typeparam name="T">The key type used to group tweens (e.g. card, unit).</typeparam>
    public abstract class TweenController<T> : GameServiceBehaviour where T : class
    {
        private readonly Dictionary<T, List<TweenBase>> _activeTweens = new();

        /// <summary>
        /// Tracks active tweens for the specified key so they can be killed on demand.
        /// </summary>
        protected void RegisterTweens(T key, params TweenBase[] tweens)
        {
            if (!_activeTweens.TryGetValue(key, out List<TweenBase> list))
            {
                list = new List<TweenBase>();
                _activeTweens[key] = list;
            }

            foreach (TweenBase t in tweens)
                if (t != null)
                    list.Add(t);
        }

        /// <summary>
        /// Kills all active tweens currently registered for the specified key.
        /// </summary>
        protected void KillTweens(T key)
        {
            if (!_activeTweens.TryGetValue(key, out List<TweenBase> list))
                return;

            foreach (TweenBase t in list)
                t?.Stop();

            list.Clear();
        }
    }
}