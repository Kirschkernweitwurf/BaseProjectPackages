#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.OrderManagement
{
    /// <summary>Single named constant emitted into the generated class.</summary>
    [Serializable]
    public sealed class OrderConstant
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private int value;

        [SerializeField] [TextArea]
        private string comment;

        /// <summary>Identifier used for the generated constant.</summary>
        public string Name => name;

        /// <summary>Value assigned to the generated constant.</summary>
        public int Value => value;

        /// <summary>Optional text emitted as an XML summary above the constant.</summary>
        public string Comment => comment;
    }
}
#endif