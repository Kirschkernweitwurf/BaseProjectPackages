using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.OrderManagement
{
    /// <summary>Central store of all constants. Edited through the Order Manager window.</summary>
    [FilePath(FilePathValue, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class OrderRegistry : ScriptableSingleton<OrderRegistry>
    {
        private const string FilePathValue = "ProjectSettings/UnityConstantsOrderRegistry.asset";

        [SerializeField]
        private string outputDirectory = "Assets/Generated/UnityConstants";

        [SerializeField]
        private string generatedNamespace = "Generated.UnityConstants";

        [SerializeField]
        private string rootClassName = "MenuOrders";

        [SerializeField]
        private List<OrderConstant> constants = new();

        /// <summary>Project relative or absolute folder the generated file is written to.</summary>
        internal string OutputDirectory => outputDirectory;

        /// <summary>Namespace of the generated code.</summary>
        internal string GeneratedNamespace => generatedNamespace;

        /// <summary>Name of the generated root static class. Also used as the file name.</summary>
        internal string RootClassName => rootClassName;

        /// <summary>All configured constants.</summary>
        internal IReadOnlyList<OrderConstant> Constants => constants;

        /// <summary>Writes the in-memory state to disk.</summary>
        internal void Persist() => Save(true);
    }
}