using System.Collections.Generic;
using Base.CorePackage.StateMachine;
using UnityEngine.UIElements;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// The facts about the selected machine that the drawing cannot carry: where it started, how long it
    /// has been where it is, and which transitions are being evaluated right now, in the order they are
    /// asked. That order is the thing a picture hides and a stuck machine usually turns on.
    /// </summary>
    internal sealed class StateMachineDetailsView : VisualElement
    {
        private const string AnySourceLabel = "any state";
        private const string CurrentLabel = "Current state";
        private const string EmptyValue = "none";
        private const string InitialLabel = "Started in";
        private const string LastTransitionLabel = "Last transition";
        private const string MachineLabel = "Machine";
        private const string NoTransitionsMessage = "Nothing leaves this state, so the machine stays here.";
        private const string PriorityFormat = "priority {0}";
        private const string TimeFormat = "{0:0.00} s";
        private const string TimeLabel = "Time in state";
        private const string TransitionFormat = "{0}  to  {1}";
        private const string TransitionsFormat = "Evaluated from {0}";

        private readonly Label _machineValue = new();
        private readonly Label _initialValue = new();
        private readonly Label _currentValue = new();
        private readonly Label _timeValue = new();
        private readonly Label _lastValue = new();
        private readonly Label _transitionsTitle = new();
        private readonly VisualElement _transitions = new();
        private readonly ScrollView _body = new();

        /// <summary>Builds the pane content.</summary>
        internal StateMachineDetailsView()
        {
            style.flexGrow = 1f;

            _body.style.flexGrow = 1f;

            _body.Add(BuildRow(MachineLabel, _machineValue));
            _body.Add(BuildRow(InitialLabel, _initialValue));
            _body.Add(BuildRow(CurrentLabel, _currentValue));
            _body.Add(BuildRow(TimeLabel, _timeValue));
            _body.Add(BuildRow(LastTransitionLabel, _lastValue));

            _transitionsTitle.AddToClassList(StateMachineStyle.SectionClass);

            _body.Add(_transitionsTitle);
            _body.Add(_transitions);

            Add(_body);

            Show(null);
        }

        /// <summary>Re-reads everything from the given machine.</summary>
        /// <param name="machine">The machine being watched, or null when none is selected.</param>
        internal void Show(IStateMachineInfo machine)
        {
            if (machine == null)
            {
                _machineValue.text = EmptyValue;
                _initialValue.text = EmptyValue;
                _currentValue.text = EmptyValue;
                _timeValue.text = EmptyValue;
                _lastValue.text = EmptyValue;
                _transitionsTitle.text = string.Empty;

                _transitions.Clear();

                return;
            }

            _machineValue.text = machine.Name;
            _initialValue.text = machine.InitialStateName;
            _currentValue.text = machine.CurrentStateName;
            _timeValue.text = string.Format(TimeFormat, machine.TimeInState);
            _lastValue.text = machine.LastTransitionName;
            _transitionsTitle.text = string.Format(TransitionsFormat, machine.CurrentStateName);

            RebuildTransitions(machine);
        }

        private static VisualElement BuildRow(string label, Label value)
        {
            VisualElement row = new();
            row.AddToClassList(StateMachineStyle.FieldRowClass);

            Label caption = new(label);
            caption.AddToClassList(StateMachineStyle.FieldLabelClass);

            value.AddToClassList(StateMachineStyle.FieldValueClass);

            row.Add(caption);
            row.Add(value);

            return row;
        }

        // Any state transitions are asked before the ones leaving the active state, and both groups are
        // already ordered by priority, so listing them in that order is the evaluation order itself.
        private static List<StateMachineEdge> Collect(IStateMachineInfo machine)
        {
            List<StateMachineEdge> evaluated = new();
            string current = machine.CurrentStateName;

            foreach (StateMachineEdge edge in machine.Edges)
            {
                if (edge.IsFromAnyState && edge.To != current)
                    evaluated.Add(edge);
            }

            foreach (StateMachineEdge edge in machine.Edges)
            {
                if (!edge.IsFromAnyState && edge.From == current)
                    evaluated.Add(edge);
            }

            return evaluated;
        }

        private static VisualElement BuildTransitionRow(StateMachineEdge edge)
        {
            VisualElement row = new();
            row.AddToClassList(StateMachineStyle.RowClass);

            Label name = new(string.Format(TransitionFormat, edge.Name, edge.To));
            name.AddToClassList(StateMachineStyle.FieldValueClass);

            Label source = new(edge.IsFromAnyState
                ? AnySourceLabel
                : string.Format(PriorityFormat, edge.Priority));

            source.AddToClassList(StateMachineStyle.FieldLabelClass);

            row.Add(name);
            row.Add(source);

            return row;
        }
        private void RebuildTransitions(IStateMachineInfo machine)
        {
            _transitions.Clear();

            List<StateMachineEdge> evaluated = Collect(machine);

            if (evaluated.Count == 0)
            {
                Label empty = new(NoTransitionsMessage);
                empty.AddToClassList(StateMachineStyle.FieldLabelClass);

                _transitions.Add(empty);

                return;
            }

            foreach (StateMachineEdge edge in evaluated)
                _transitions.Add(BuildTransitionRow(edge));
        }

    }
}