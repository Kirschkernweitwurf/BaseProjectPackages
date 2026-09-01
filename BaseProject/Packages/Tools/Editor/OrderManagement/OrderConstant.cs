using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.OrderManagement
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
    }
}