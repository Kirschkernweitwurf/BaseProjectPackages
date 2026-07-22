using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.ComponentClipboard
{
    /// <summary>
    /// One planned paste of a single clipboard entry. Built by
    /// <see cref="ComponentOperations.BuildPastePlan"/> so the window preview and the actual paste
    /// can never disagree.
    /// </summary>
    public class ComponentPasteStep
    {
        /// <summary>Clipboard entry this step pastes.</summary>
        public ComponentClipboardEntry Entry { get; }

        /// <summary>Resolved component type, null when the type no longer exists.</summary>
        public Type Type { get; }

        /// <summary>Components that get overwritten. Empty means a new component is added.</summary>
        public IReadOnlyList<Component> Targets => _targets;

        /// <summary>Returns true when the entry type could be resolved.</summary>
        public bool IsValid => Type != null;

        /// <summary>Returns true when this step overwrites instead of adding.</summary>
        public bool IsOverwrite => _targets.Count > 0;

        private readonly List<Component> _targets;

        /// <summary>Creates a step.</summary>
        /// <param name="entry">Clipboard entry to paste.</param>
        /// <param name="type">Resolved component type, or null when unresolvable.</param>
        /// <param name="targets">Components to overwrite, empty to add a new component.</param>
        public ComponentPasteStep(ComponentClipboardEntry entry, Type type, List<Component> targets)
        {
            Entry = entry;
            Type = type;
            _targets = targets ?? new List<Component>();
        }
    }
}