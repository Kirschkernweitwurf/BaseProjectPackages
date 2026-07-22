using System;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Remembers what a single reference field pointed at, independent of session local instance ids.
    /// Raw JSON only stores instance ids, which are meaningless once the play mode scene is torn down.
    /// </summary>
    [Serializable]
    public class PlayModeObjectReference
    {
        public string propertyPath;
        public EPlayModeReferenceKind kind;

        /// <summary>Set for assets only. Assets keep a stable id across play mode.</summary>
        public string globalObjectId;

        /// <summary>Asset path of the owning scene. Set for scene objects only.</summary>
        public string scenePath;

        /// <summary>Absolute for scene objects, instance root relative for prefab internal references.</summary>
        public string namePath;

        /// <summary>Same origin as <see cref="namePath"/>.</summary>
        public string indexPath;

        /// <summary>Full name of the component type, or empty when the reference points at a GameObject.</summary>
        public string componentTypeName;

        /// <summary>Index among components of the same type on one GameObject.</summary>
        public int componentIndex;
    }
}