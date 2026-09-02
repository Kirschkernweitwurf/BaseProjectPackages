using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>A GraphView node that represents one assembly.</summary>
    internal sealed class AssemblyGraphNode : Node
    {
        private const string ActionRowClass = "action-row";
        private const string CleanButtonClass = "clean-button";
        private const string CleanLabel = "Remove unused";
        private const string ClearFocusLabel = "Clear focus";
        private const string FocusButtonClass = "focus-button";
        private const string FocusedClass = "is-focused";
        private const string FocusLabel = "Focus";
        private const string FocusTooltip = "Show only this assembly, what it references, and what "
            + "references it.";
        private const string GoToButtonClass = "go-to-button";
        private const string GoToLabel = "Go to";
        private const string IssuesClass = "has-issues";
        private const string KindLabelClass = "kind-label";
        private const string NodeClass = "assembly-node";
        private const string UnusedHeaderClass = "unused-header";
        private const string UnusedHeaderFormat = "Unused references ({0})";
        private const string UnusedLineClass = "unused-line";

        /// <summary>The port incoming references connect to, meaning assemblies that reference this one.</summary>
        internal Port InputPort { get; }

        /// <summary>The port outgoing references leave from, meaning assemblies this one references.</summary>
        internal Port OutputPort { get; }

        private AssemblyNodeInfo Info { get; }

        private readonly Action<AssemblyNodeInfo> _onFocusRequested;
        private readonly Action<AssemblyNodeInfo> _onCleanupRequested;
        private readonly bool _isFocused;

        /// <summary>Builds a graph node for one assembly, with its ports and action buttons.</summary>
        public AssemblyGraphNode(AssemblyNodeInfo info,
            bool isFocused,
            Action<AssemblyNodeInfo> onFocusRequested,
            Action<AssemblyNodeInfo> onCleanupRequested)
        {
            Info = info;
            _isFocused = isFocused;
            _onFocusRequested = onFocusRequested;
            _onCleanupRequested = onCleanupRequested;

            title = info.Name;
            AddToClassList(NodeClass);

            if (info.HasUnusedReferences)
                AddToClassList(IssuesClass);

            if (isFocused)
                AddToClassList(FocusedClass);

            ApplyColors();

            InputPort = CreatePort(Direction.Input, "in");
            inputContainer.Add(InputPort);

            OutputPort = CreatePort(Direction.Output, "out");
            outputContainer.Add(OutputPort);

            BuildBody();
            RefreshExpandedState();
            RefreshPorts();
        }

        private Port CreatePort(Direction direction, string label)
        {
            Port port = InstantiatePort(Orientation.Horizontal, direction, Port.Capacity.Multi, typeof(bool));
            port.portName = label;
            return port;
        }

        /// <summary>Paints every container opaque and tints the title bar by assembly root name.</summary>
        /// <remarks>
        /// Written as inline styles rather than left to the sheet, because these are the named
        /// children GraphView builds inside a Node and nothing here can put a class on them. The
        /// window rebuilds its nodes when the theme moves, which is what keeps these current.
        /// </remarks>
        private void ApplyColors()
        {
            Color body = EditorPalette.Card;

            mainContainer.style.backgroundColor = body;
            extensionContainer.style.backgroundColor = body;
            titleContainer.style.backgroundColor = AssemblyColorPalette.GetColor(Info.RootName);

            ApplyBorderColor();
        }

        // The widths come from the sheet, so only the color is decided here. Focus wins over issues:
        // a focused node is the one the user just asked to look at.
        private void ApplyBorderColor()
        {
            if (!_isFocused && !Info.HasUnusedReferences)
                return;

            Color border = _isFocused
                ? EditorPalette.Focus
                : EditorPalette.Danger;

            style.borderTopColor = border;
            style.borderBottomColor = border;
            style.borderLeftColor = border;
            style.borderRightColor = border;
        }

        private void BuildBody()
        {
            Label kindLabel = new(Info.Kind.ToString());

            kindLabel.AddToClassList(KindLabelClass);
            kindLabel.AddToClassList(EditorUIClass.Dim);
            extensionContainer.Add(kindLabel);

            extensionContainer.Add(BuildActionRow());

            List<string> unused = CollectUnusedNames();
            if (unused.Count == 0)
                return;

            Label header = new(string.Format(UnusedHeaderFormat, unused.Count));

            header.AddToClassList(UnusedHeaderClass);
            header.AddToClassList(EditorUIClass.Danger);
            extensionContainer.Add(header);

            foreach (string name in unused)
            {
                Label line = new(name);

                line.AddToClassList(UnusedLineClass);
                line.AddToClassList(EditorUIClass.Danger);
                extensionContainer.Add(line);
            }

            if (!Info.IsCleanable)
                return;

            Button cleanButton = new(() => _onCleanupRequested?.Invoke(Info))
            {
                text = CleanLabel
            };

            cleanButton.AddToClassList(CleanButtonClass);
            cleanButton.AddToClassList(EditorUIClass.Button);

            // Destructive, so it takes the danger fill rather than either shared button class.
            cleanButton.style.backgroundColor = EditorPalette.Danger;
            cleanButton.style.color = EditorPalette.AccentText;

            extensionContainer.Add(cleanButton);
        }

        private VisualElement BuildActionRow()
        {
            VisualElement row = new();

            row.AddToClassList(ActionRowClass);

            Button focusButton = new(() => _onFocusRequested?.Invoke(Info))
            {
                text = _isFocused
                    ? ClearFocusLabel
                    : FocusLabel,
                tooltip = FocusTooltip
            };

            focusButton.AddToClassList(FocusButtonClass);
            focusButton.AddToClassList(EditorUIClass.Button);

            // Amber while focused, matching the border of the node it belongs to, so the two read as
            // one state rather than two.
            if (_isFocused)
            {
                focusButton.style.backgroundColor = EditorPalette.Focus;
                focusButton.style.color = EditorPalette.Text;
            }
            else
            {
                focusButton.AddToClassList(EditorUIClass.ButtonSecondary);
            }

            row.Add(focusButton);

            Button goToButton = new(GoTo)
            {
                text = GoToLabel
            };

            goToButton.AddToClassList(GoToButtonClass);
            goToButton.AddToClassList(EditorUIClass.Button);
            goToButton.AddToClassList(EditorUIClass.ButtonSecondary);
            goToButton.SetEnabled(Info.HasAsmdef);
            row.Add(goToButton);

            return row;
        }

        private List<string> CollectUnusedNames()
        {
            List<string> result = new();
            foreach (AssemblyReferenceInfo reference in Info.References)
            {
                if (reference.IsUnused)
                    result.Add(reference.TargetName);
            }

            return result;
        }

        private void GoTo()
        {
            if (!Info.HasAsmdef)
                return;

            AssemblyDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(Info.AsmdefPath);
            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}