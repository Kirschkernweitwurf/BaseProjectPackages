using UnityEditor;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Writes the project-level execution order for a script. This mirrors the value shown
    /// in Project Settings and never touches the script's source code.
    /// </summary>
    public static class ExecutionOrderWriter
    {
        /// <summary>Stores the given execution order on the script's importer.</summary>
        public static void SetProjectOrder(MonoScript script, int order)
        {
            if (script == null)
                return;

            MonoImporter.SetExecutionOrder(script, order);
        }
    }
}