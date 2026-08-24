using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.CorePackage.StateMachine
{
    /// <summary>
    /// Keeps track of the machines that are currently running, so tooling can look at them without the
    /// owning objects having to publish anything. The monitor window reads nothing else.
    /// <para>
    /// Entries are weak, so a machine whose owner was destroyed without stopping it disappears on the next
    /// collection instead of being held alive by this list.
    /// </para>
    /// </summary>
    public static class StateMachineRegistry
    {
        private static readonly List<WeakReference<IStateMachineInfo>> Registered = new();
        private static readonly List<IStateMachineInfo> Alive = new();

        /// <summary>The machines that are running right now.</summary>
        /// <returns>The live machines. The list is reused, so copy it to keep it.</returns>
        public static IReadOnlyList<IStateMachineInfo> GetRunning()
        {
            Compact();

            return Alive;
        }

        /// <summary>Adds a machine that has just started.</summary>
        /// <param name="machine">The machine to list.</param>
        internal static void Register(IStateMachineInfo machine)
        {
            if (machine == null || IndexOf(machine) >= 0)
                return;

            Registered.Add(new WeakReference<IStateMachineInfo>(machine));
        }

        /// <summary>Drops a machine that has stopped.</summary>
        /// <param name="machine">The machine to remove.</param>
        internal static void Unregister(IStateMachineInfo machine)
        {
            if (machine == null)
                return;

            int index = IndexOf(machine);

            if (index >= 0)
                Registered.RemoveAt(index);
        }

        private static int IndexOf(IStateMachineInfo machine)
        {
            for (int i = 0; i < Registered.Count; i++)
            {
                if (Registered[i].TryGetTarget(out IStateMachineInfo found) && ReferenceEquals(found, machine))
                    return i;
            }

            return -1;
        }

        private static void Compact()
        {
            Alive.Clear();

            for (int i = Registered.Count - 1; i >= 0; i--)
            {
                if (!Registered[i].TryGetTarget(out _))
                    Registered.RemoveAt(i);
            }

            foreach (WeakReference<IStateMachineInfo> reference in Registered)
            {
                if (reference.TryGetTarget(out IStateMachineInfo machine))
                    Alive.Add(machine);
            }
        }

        // Entering play mode with domain reloading turned off keeps the list from the previous run, which
        // would leave the monitor showing machines that no longer exist.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Registered.Clear();
            Alive.Clear();
        }
    }
}