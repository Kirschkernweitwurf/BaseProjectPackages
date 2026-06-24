#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.OrderManagement
{
    /// <summary>Central store of all constants. Edited through the Order Manager window.</summary>
    [FilePath(FilePathValue, FilePathAttribute.Location.ProjectFolder)]
    public sealed class OrderRegistry : ScriptableSingleton<OrderRegistry>
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
        public string OutputDirectory => outputDirectory;

        /// <summary>Namespace of the generated code.</summary>
        public string GeneratedNamespace => generatedNamespace;

        /// <summary>Name of the generated root static class. Also used as the file name.</summary>
        public string RootClassName => rootClassName;

        /// <summary>All configured constants.</summary>
        public IReadOnlyList<OrderConstant> Constants => constants;

        /// <summary>Writes the in-memory state to disk.</summary>
        public void Persist() => Save(true);
    }
}
#endif