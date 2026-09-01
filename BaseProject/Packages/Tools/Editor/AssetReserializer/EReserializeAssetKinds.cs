using System;

namespace Base.ToolPackage.Editor.AssetReserializer
{
    /// <summary>
    /// Which kinds of asset a reserialize run touches. Combined as flags, so one run can cover
    /// several kinds at once.
    /// </summary>
    [Flags]
    internal enum EReserializeAssetKinds : byte
    {
        /// <summary>Nothing is collected.</summary>
        None = 0,

        /// <summary>Prefab assets.</summary>
        Prefabs = 1,

        /// <summary>Scene assets.</summary>
        Scenes = 2,

        /// <summary>ScriptableObject assets.</summary>
        ScriptableObjects = 4,

        /// <summary>Every kind this tool knows.</summary>
        All = 7
    }
}