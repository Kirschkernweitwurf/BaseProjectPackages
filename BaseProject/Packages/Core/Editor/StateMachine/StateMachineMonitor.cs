using System.Collections.Generic;
using Base.CorePackage.StateMachine;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// Watches the <see cref="StateMachine{TContext}"/> instances running in play mode. The list of
    /// machines is on the left, the selected one is drawn in the middle, and what the drawing cannot carry
    /// is underneath it.
    /// <para>
    /// Nothing here can be edited. A machine is defined in code, so this window reads it rather than
    /// authoring it, which is what keeps the picture from ever disagreeing with what actually runs.
    /// </para>
    /// </summary>
    internal sealed class StateMachineMonitor : EditorWindow
    {
        private const float DetailsHeight = 220f;
        private const string DetailsPaneTitle = "DETAILS";
        private const string EditModeBody = "Machines register themselves when they start, so enter play mode "
            + "to watch them. Nothing needs to be set up first.";
        private const string EditModeTitle = "Not playing";
        private const string GraphPaneTitle = "MACHINE";
        private const string IdleBody = "No state machine is running. One appears here as soon as something "
            + "calls Start on it.";
        private const string IdleTitle = "Nothing running";
        private const string ListPaneTitle = "MACHINES";
        private const float ListWidth = 220f;
        private const string MachineCountFormat = "{0} running";
        private const string MenuPath = "Tools/Base Packages/Gameplay/State Machine Monitor";
        private const string MissingSheetMessage = "The state machine monitor style sheet was not found, "
            + "so the window is drawn unstyled.";
        private const string NoMachineStatus = "Nothing to watch.";
        private const double PollInterval = 0.1d;
        private const string ShapeFormat = "{0} states, {1} transitions";
        private const string SingleMachineLabel = "1 running";
        private const string WindowTitle = "State Machine Monitor";

        private static readonly Vector2 MinWindowSize = new(880f, 520f);

        private readonly List<IStateMachineInfo> _machines = new();

        private StateMachineCanvas _canvas;
        private StateMachineDetailsView _details;
        private StateMachineListView _list;
        private StateMachineMessageView _message;
        private StateMachinePane _detailsPane;
        private StateMachinePane _graphPane;
        private StateMachinePane _listPane;
        private ScrollView _canvasScroll;
        private Label _status;
        private Label _statusChip;
        private IStateMachineInfo _drawn;
        private double _nextPoll;

#region Unity Callbacks
        private void OnEnable() => EditorApplication.update += OnEditorUpdate;

        private void OnDisable() => EditorApplication.update -= OnEditorUpdate;

        private void CreateGUI()
        {
            rootVisualElement.Add(BuildPanes());
            rootVisualElement.Add(BuildStatusBar());

            // After the tree exists, so the first paint reaches every element rather than only the
            // root. The theme is polled from here on, so a color changed in the settings page lands
            // without reopening the window.
            if (!StateMachineStyle.Apply(rootVisualElement))
                CustomLogger.LogWarning(MissingSheetMessage, this);

            Poll(true);
        }
#endregion

        /// <summary>Opens or focuses the window.</summary>
        [DynamicMenuItem(MenuPath)]
        internal static void Open()
        {
            StateMachineMonitor window = GetWindow<StateMachineMonitor>();

            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = MinWindowSize;
            window.Show();
        }

        private VisualElement BuildPanes()
        {
            _list = new StateMachineListView();
            _list.SelectionChanged += OnMachineSelected;

            _listPane = new StateMachinePane(ListPaneTitle);
            _listPane.Body.Add(_list);

            _canvas = new StateMachineCanvas();

            _canvasScroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal)
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            _canvasScroll.Add(_canvas);

            _message = new StateMachineMessageView();

            _graphPane = new StateMachinePane(GraphPaneTitle);
            _graphPane.Body.Add(_canvasScroll);
            _graphPane.Body.Add(_message);

            _details = new StateMachineDetailsView();

            _detailsPane = new StateMachinePane(DetailsPaneTitle);
            _detailsPane.Body.Add(_details);

            TwoPaneSplitView right = new(1, DetailsHeight, TwoPaneSplitViewOrientation.Vertical);

            right.Add(_graphPane);
            right.Add(_detailsPane);

            TwoPaneSplitView split = new(0, ListWidth, TwoPaneSplitViewOrientation.Horizontal);

            split.Add(_listPane);
            split.Add(right);
            split.style.flexGrow = 1f;

            return split;
        }

        private VisualElement BuildStatusBar()
        {
            VisualElement bar = new();
            bar.AddToClassList(StateMachineStyle.StatusClass);

            _status = new Label(string.Empty);
            _status.AddToClassList(StateMachineStyle.StatusTextClass);

            _statusChip = StateMachineStyle.Chip(string.Empty, StateMachineStyle.ChipGoodClass);

            bar.Add(_status);
            bar.Add(_statusChip);

            return bar;
        }

        // Polled rather than subscribed to, because a machine can start and stop many times per second and
        // the window has no way to hold on to one of them. Ten times a second is well under what a person
        // can read and far above what costs anything.
        private void OnEditorUpdate()
        {
            if (_canvas == null || EditorApplication.timeSinceStartup < _nextPoll)
                return;

            _nextPoll = EditorApplication.timeSinceStartup + PollInterval;

            Poll(false);
        }

        private void Poll(bool force)
        {
            _machines.Clear();
            _machines.AddRange(StateMachineRegistry.GetRunning());

            _list.SetMachines(_machines);
            _list.RefreshStates();

            IStateMachineInfo selected = _list.Selected;

            // The drawing only changes when the machine does. Everything else about it moves every frame
            // and is written into the elements that are already there.
            if (force || !ReferenceEquals(selected, _drawn))
            {
                _drawn = selected;

                _canvas.Show(selected);
                UpdateGraphHeader(selected);
            }

            _canvas.UpdateLive(selected);
            _details.Show(selected);

            UpdateEmptyState(selected);
            UpdateStatus(selected);
        }

        private void OnMachineSelected(IStateMachineInfo machine)
        {
            _drawn = machine;

            _canvas.Show(machine);
            _canvas.UpdateLive(machine);

            _details.Show(machine);

            UpdateGraphHeader(machine);
            UpdateEmptyState(machine);
            UpdateStatus(machine);
        }

        private void UpdateGraphHeader(IStateMachineInfo machine) => _graphPane.SetNote(machine == null
            ? string.Empty
            : string.Format(ShapeFormat, machine.StateNames.Count, machine.Edges.Count));

        private void UpdateEmptyState(IStateMachineInfo machine)
        {
            _listPane.SetNote(_machines.Count == 1
                ? SingleMachineLabel
                : string.Format(MachineCountFormat, _machines.Count));

            if (machine != null)
            {
                _canvasScroll.style.display = DisplayStyle.Flex;
                _message.style.display = DisplayStyle.None;

                return;
            }

            if (EditorApplication.isPlaying)
                _message.Show(StateMachineMessageView.IdleGlyph, IdleTitle, IdleBody);
            else
                _message.Show(StateMachineMessageView.IdleGlyph, EditModeTitle, EditModeBody);

            _canvasScroll.style.display = DisplayStyle.None;
            _message.style.display = DisplayStyle.Flex;
        }

        // The chip is kept and rewritten rather than rebuilt, because this runs ten times a second and a
        // fresh label each time would be pure garbage.
        private void UpdateStatus(IStateMachineInfo machine)
        {
            _status.text = machine == null
                ? NoMachineStatus
                : machine.Name;

            _statusChip.text = machine == null
                ? string.Empty
                : machine.CurrentStateName;

            _statusChip.style.display = machine == null
                ? DisplayStyle.None
                : DisplayStyle.Flex;
        }
    }
}