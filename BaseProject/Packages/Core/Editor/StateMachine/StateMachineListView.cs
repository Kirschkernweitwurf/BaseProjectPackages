using System;
using System.Collections.Generic;
using Base.CorePackage.StateMachine;
using UnityEngine.UIElements;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// Lists the machines that are running right now. Rows are rebuilt only when the set of machines
    /// changes; the state each one is in is refreshed on every poll, so the list itself reads as a live
    /// summary before anything is even selected.
    /// </summary>
    internal sealed class StateMachineListView : VisualElement
    {
        private readonly Dictionary<IStateMachineInfo, Label> _stateLabels = new();
        private readonly List<IStateMachineInfo> _machines = new();
        private readonly ScrollView _list = new();

        /// <summary>Raised when the user picks a different machine.</summary>
        internal event Action<IStateMachineInfo> SelectionChanged;

        /// <summary>The machine currently picked, or null when the list is empty.</summary>
        internal IStateMachineInfo Selected { get; private set; }

        /// <summary>Builds the list.</summary>
        internal StateMachineListView()
        {
            style.flexGrow = 1f;

            _list.style.flexGrow = 1f;

            Add(_list);
        }

        /// <summary>
        /// Replaces the listed machines. Does nothing when the same machines are still running, so the
        /// selection survives a poll.
        /// </summary>
        /// <param name="machines">The machines running right now.</param>
        internal void SetMachines(IReadOnlyList<IStateMachineInfo> machines)
        {
            if (Matches(machines))
                return;

            _machines.Clear();
            _machines.AddRange(machines);

            Rebuild();
        }

        /// <summary>Re-reads the state each listed machine is in.</summary>
        internal void RefreshStates()
        {
            foreach (KeyValuePair<IStateMachineInfo, Label> pair in _stateLabels)
                pair.Value.text = pair.Key.CurrentStateName;
        }

        private bool Matches(IReadOnlyList<IStateMachineInfo> machines)
        {
            if (machines.Count != _machines.Count)
                return false;

            for (int i = 0; i < machines.Count; i++)
            {
                if (!ReferenceEquals(machines[i], _machines[i]))
                    return false;
            }

            return true;
        }

        private void Rebuild()
        {
            _list.Clear();
            _stateLabels.Clear();

            foreach (IStateMachineInfo machine in _machines)
                _list.Add(BuildRow(machine));

            // A machine that stopped takes the selection with it, so the window falls back to the first
            // one still running rather than showing a drawing of something that is gone.
            if (Selected != null && _machines.Contains(Selected))
            {
                ApplySelection(Selected);
                return;
            }

            Select(_machines.Count > 0
                ? _machines[0]
                : null);
        }

        private VisualElement BuildRow(IStateMachineInfo machine)
        {
            VisualElement row = new();
            row.AddToClassList(StateMachineStyle.MachineRowClass);

            Label title = new(machine.Name);
            title.AddToClassList(StateMachineStyle.MachineTitleClass);

            Label state = new(machine.CurrentStateName);
            state.AddToClassList(StateMachineStyle.MachineStateClass);

            row.Add(title);
            row.Add(state);

            row.RegisterCallback<MouseDownEvent>(_ => Select(machine));

            _stateLabels[machine] = state;

            return row;
        }

        private void Select(IStateMachineInfo machine)
        {
            if (ReferenceEquals(machine, Selected))
                return;

            ApplySelection(machine);

            SelectionChanged?.Invoke(machine);
        }

        private void ApplySelection(IStateMachineInfo machine)
        {
            Selected = machine;

            for (int i = 0; i < _machines.Count; i++)
            {
                _list[i].EnableInClassList(StateMachineStyle.MachineRowSelectedClass,
                    ReferenceEquals(_machines[i], machine));
            }
        }
    }
}