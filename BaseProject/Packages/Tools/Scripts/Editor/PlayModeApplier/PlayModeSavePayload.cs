using System;
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Holds everything captured from one object at the moment play mode ends.
    /// Both a scene location and a prefab location are always recorded, because play mode gives no reliable
    /// way to tell an object that was instantiated at runtime from one that came with the scene.
    /// The destination is a choice in the window rather than a guess made here.
    /// </summary>
    [Serializable]
    public class PlayModeSavePayload
    {
        public string displayName;
        public EPlayModeApplyTarget applyTarget;
        public string json;
        public List<PlayModeObjectReference> objectReferences = new();

        /// <summary>Full name of the component type.</summary>
        public string componentTypeName;

        /// <summary>Index among components of the same type on one GameObject.</summary>
        public int componentIndex;

        /// <summary>Asset path of the scene the object sat in during play mode.</summary>
        public string scenePath;

        /// <summary>Hierarchy path inside the scene, scene root included.</summary>
        public string sceneNamePath;

        /// <summary>Sibling index path inside the scene, scene root included.</summary>
        public string sceneIndexPath;

        /// <summary>Guessed source prefab. Empty when no single prefab matched the name.</summary>
        public string sourcePrefabGuid;

        /// <summary>Hierarchy path inside the prefab, prefab root excluded.</summary>
        public string prefabNamePath;

        /// <summary>Sibling index path inside the prefab, prefab root excluded.</summary>
        public string prefabIndexPath;
    }
}
