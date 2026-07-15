using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>A GraphView node that represents one assembly.</summary>
    public sealed class AssemblyGraphNode : Node
    {
        public AssemblyNodeInfo Info { get; }

        public Port InputPort { get; }

        public Port OutputPort { get; }

        private static readonly Color BodyColor = new(0.22f, 0.22f, 0.24f, 1f);

        private readonly Action<AssemblyNodeInfo> _onCleanupRequested;

        public AssemblyGraphNode(AssemblyNodeInfo info, Action<AssemblyNodeInfo> onCleanupRequested)
        {
            Info = info;
            _onCleanupRequested = onCleanupRequested;

            title = info.Name;
            AddToClassList("assembly-node");
            if (info.HasUnusedReferences)
                AddToClassList("has-issues");

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
        private void ApplyColors()
        {
            mainContainer.style.backgroundColor = BodyColor;
            extensionContainer.style.backgroundColor = BodyColor;
            titleContainer.style.backgroundColor = AssemblyColorPalette.GetColor(Info.RootName);
        }

        private void BuildBody()
        {
            Label kindLabel = new(Info.Kind.ToString());
            kindLabel.AddToClassList("kind-label");
            extensionContainer.Add(kindLabel);

            Button goToButton = new(GoTo)
            {
                text = "Go to"
            };

            goToButton.AddToClassList("go-to-button");
            goToButton.SetEnabled(Info.HasAsmdef);
            extensionContainer.Add(goToButton);

            List<string> unused = CollectUnusedNames();
            if (unused.Count == 0)
                return;

            Label header = new($"Unused references ({unused.Count})");
            header.AddToClassList("unused-header");
            extensionContainer.Add(header);

            foreach (string name in unused)
            {
                Label line = new(name);
                line.AddToClassList("unused-line");
                extensionContainer.Add(line);
            }

            if (!Info.IsCleanable)
                return;

            Button cleanButton = new(() => _onCleanupRequested?.Invoke(Info))
            {
                text = "Remove unused"
            };

            cleanButton.AddToClassList("clean-button");
            extensionContainer.Add(cleanButton);
        }

        private List<string> CollectUnusedNames()
        {
            List<string> result = new();
            foreach (AssemblyReferenceInfo reference in Info.References)
                if (reference.IsUnused)
                    result.Add(reference.TargetName);

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