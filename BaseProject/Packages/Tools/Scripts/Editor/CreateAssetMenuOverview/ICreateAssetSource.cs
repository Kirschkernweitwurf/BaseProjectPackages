#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.CreateAssetMenuOverview
{
    /// <summary>
    /// Supplies the set of <see cref="CreateAssetMenuAttribute"/> entries currently defined
    /// in the editor. Implementations decide where that information comes from.
    /// </summary>
    public interface ICreateAssetSource
    {
        /// <summary>Builds a fresh snapshot of all CreateAssetMenu entries.</summary>
        IReadOnlyList<CreateAssetEntry> Collect();
    }
}
#endif