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

        /// <summary>Snapshot of the active timers, so callbacks may start or stop timers while ticking.</summary>
        private static readonly List<Timer> TickBuffer = new();

        private static bool _isInitialized;

        /// <summary>Adds a timer to the update list. Called by <see cref="Timer.Start"/>.</summary>
        internal static void Register(Timer timer)
        {
            EnsureInitialized();

            if (!ActiveTimers.Contains(timer))
                ActiveTimers.Add(timer);
        }

        /// <summary>Removes a timer from the update list. Called by <see cref="Timer.Stop"/>.</summary>
        internal static void Unregister(Timer timer) => ActiveTimers.Remove(timer);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            ActiveTimers.Clear();
            TickBuffer.Clear();
            _isInitialized = false;
        }

        private static void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            PlayerLoopSystem rootLoop = PlayerLoop.GetCurrentPlayerLoop();

            // A stale entry survives when domain reload is turned off, so drop it before inserting.
            RemoveExistingSystem(ref rootLoop);

            PlayerLoopSystem timerLoop = new()
            {
                type = typeof(TimerManager),
                updateDelegate = OnUpdate
            };

            InsertUnderPhase(ref rootLoop, timerLoop, typeof(Update));
            PlayerLoop.SetPlayerLoop(rootLoop);
            _isInitialized = true;
        }

        private static void RemoveExistingSystem(ref PlayerLoopSystem loop)
        {
            if (loop.subSystemList == null)
                return;

            List<PlayerLoopSystem> subSystems = new(loop.subSystemList);

            if (subSystems.RemoveAll(system => system.type == typeof(TimerManager)) > 0)
                loop.subSystemList = subSystems.ToArray();

            for (int i = 0; i < loop.subSystemList.Length; i++)
                RemoveExistingSystem(ref loop.subSystemList[i]);
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

            // Tick a copy, a callback is allowed to register or unregister timers.
            TickBuffer.AddRange(ActiveTimers);

            float deltaTime = Time.deltaTime;

            foreach (Timer timer in TickBuffer)
                timer.Tick(deltaTime);

            TickBuffer.Clear();
        }
    }
}