#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Single managed menu entry. Path, grouping, and asset file name are stored, priority is derived at runtime.</summary>
    [Serializable]
    public sealed class MenuEntry
    {
        [SerializeField]
        private string id;

        [SerializeField]
        private string path;

        [SerializeField]
        private bool enabled = true;

        [SerializeField]
        private EMenuEntryKind kind;

        [SerializeField]
        private string createFileName;

        /// <summary>Stable identity derived from the marked method or type.</summary>
        public string Id
        {
            get => id;
            set => id = value;
        }

        /// <summary>Full menu path.</summary>
        public string Path
        {
            get => path;
            set => path = value;
        }

        /// <summary>Whether the entry is registered.</summary>
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        /// <summary>Kind of the entry.</summary>
        public EMenuEntryKind Kind
        {
            get => kind;
            set => kind = value;
        }

        /// <summary>File name used when creating an asset, without extension. Only used for asset entries.</summary>
        public string CreateFileName
        {
            get => createFileName;
            set => createFileName = value;
        }

        /// <summary>Derived priority. Set to int.MinValue when the entry is not registered.</summary>
        public int Priority
        {
            get => priority;
            set => priority = value;
        }

        /// <summary>True when no matching code was found during the last scan.</summary>
        public bool Missing
        {
            get => missing;
            set => missing = value;
        }

        [NonSerialized]
        private int priority = int.MinValue;

        [NonSerialized]
        private bool missing;

        /// <summary>Required by serialization.</summary>
        public MenuEntry() { }

        /// <summary>Creates a new entry.</summary>
        public MenuEntry(string id, string path, EMenuEntryKind kind)
        {
            this.id = id;
            this.path = path;
            this.kind = kind;
        }
    }
}
#endif