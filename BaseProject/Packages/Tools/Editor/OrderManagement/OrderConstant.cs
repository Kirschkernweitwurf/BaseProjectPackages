using System;
using UnityEngine;

namespace Base.ToolsPackage.Editor.OrderManagement
{
    /// <summary>Single named constant emitted into the generated class.</summary>
    [Serializable]
    internal sealed class OrderConstant
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private int value;

        [SerializeField] [TextArea]
        private string comment;

        /// <summary>Identifier used for the generated constant.</summary>
        internal string Name => name;

        /// <summary>Value assigned to the generated constant.</summary>
        internal int Value => value;

        /// <summary>Optional text emitted as an XML summary above the constant.</summary>
        internal string Comment => comment;

        /// <summary>Creates a constant. Unity fills the fields when one is read back from disk.</summary>
        /// <param name="name">Identifier used for the generated constant.</param>
        /// <param name="value">Value assigned to the generated constant.</param>
        /// <param name="comment">Optional text emitted as an XML summary above it.</param>
        internal OrderConstant(string name, int value, string comment = null)
        {
            this.name = name;
            this.value = value;
            this.comment = comment;
        }
    }
}