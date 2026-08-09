using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Adds a type picker to a <c>[SerializeReference]</c> field so the concrete implementation can be
    /// chosen in the inspector. Unity serializes polymorphic managed references but offers no way to
    /// pick or swap the type, which leaves such fields uneditable without a custom editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReferencePickerAttribute : PropertyAttribute
    {
        /// <summary>Label shown while no instance is assigned.</summary>
        public const string NullLabel = "None";
    }
}
