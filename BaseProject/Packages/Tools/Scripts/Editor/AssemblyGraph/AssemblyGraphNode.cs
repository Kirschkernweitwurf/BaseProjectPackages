using System;
using System.Collections.Generic;
using Base.AssemblyGraphPackage.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>A GraphView node that represents one assembly.</summary>
    public sealed class AssemblyGraphNode : Node
    {
        public AssemblyNodeInfo Info { get; }

        public Port InputPort { get; }

        public Port OutputPort { get; }

        private readonly Action<AssemblyNodeInfo> _onCleanupRequested;

        public AssemblyGraphNode(AssemblyNodeInfo info, Action<AssemblyNodeInfo> onCleanupRequested)
        {
            Info = info;
            _onCleanupRequested = onCleanupRequested;

            title = info.Name;
            AddToClassList("assembly-node");
            AddKindClass();

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

        private void AddKindClass()
        {
            switch (Info.Kind)
            {
                case EAssemblyKind.Project:
                    AddToClassList("kind-project");
                    break;
                case EAssemblyKind.Package:
                    AddToClassList("kind-package");
                    break;
                default:
                    AddToClassList("kind-library");
                    break;
            }

            if (Info.HasUnusedReferences)
                AddToClassList("has-issues");
        }

        private void BuildBody()
        {
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

            Button cleanButton = new(() => _onCleanupRequested?.Invoke(Info))
            {
                text = "Remove unused"
            };

            cleanButton.AddToClassList("clean-button");
            cleanButton.SetEnabled(Info.HasAsmdef);
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