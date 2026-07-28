using System.Collections.Generic;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Supplies the set of scripts that currently define a custom execution order.
    /// Implementations decide where that information comes from.
    /// </summary>
    public interface IExecutionOrderSource
    {
        /// <summary>Builds a fresh snapshot of all entries with a custom order.</summary>
        IReadOnlyList<ExecutionOrderEntry> Collect();
    }
}