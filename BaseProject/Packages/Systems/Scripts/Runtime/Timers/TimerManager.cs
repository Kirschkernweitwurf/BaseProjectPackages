using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Base.CorePackage.Timers
{
    /// <summary>
    /// Updates every active <see cref="Timer"/> each frame through the Player Loop,
    /// so timers run without needing any GameObject or component in the scene.
    /// </summary>
    public static class TimerManager
    {
        private static readonly List<Timer> ActiveTimers = new();
        private static bool _isInitialized;

        internal static void Register(Timer timer)
        {
            if (timer == null)
                return;

            EnsureInitialized();

            if (!ActiveTimers.Contains(timer))
                ActiveTimers.Add(timer);
        }

        internal static void Unregister(Timer timer) => ActiveTimers.Remove(timer);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            ActiveTimers.Clear();
            _isInitialized = false;
        }

        private static void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            PlayerLoopSystem rootLoop = PlayerLoop.GetCurrentPlayerLoop();

            PlayerLoopSystem timerLoop = new()
            {
                type = typeof(TimerManager),
                updateDelegate = OnUpdate
            };

            InsertUnderPhase(ref rootLoop, timerLoop, typeof(Update));
            PlayerLoop.SetPlayerLoop(rootLoop);
            _isInitialized = true;
        }

        private static void InsertUnderPhase(ref PlayerLoopSystem loop, PlayerLoopSystem systemToAdd, Type phaseType)
        {
            if (loop.type == phaseType)
            {
                List<PlayerLoopSystem> subSystems = loop.subSystemList != null
                    ? new List<PlayerLoopSystem>(loop.subSystemList)
                    : new List<PlayerLoopSystem>();

                subSystems.Add(systemToAdd);
                loop.subSystemList = subSystems.ToArray();
                return;
            }

            if (loop.subSystemList == null)
                return;

            for (int i = 0; i < loop.subSystemList.Length; i++)
                InsertUnderPhase(ref loop.subSystemList[i], systemToAdd, phaseType);
        }

        private static void OnUpdate()
        {
            if (ActiveTimers.Count == 0)
                return;

            float deltaTime = Time.deltaTime;

            for (int i = ActiveTimers.Count - 1; i >= 0; i--)
                ActiveTimers[i].Tick(deltaTime);
        }
    }
}