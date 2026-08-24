using UnityEngine;
using UnityEngine.UIElements;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// One drawn state. A plain absolutely positioned box rather than a graph node, because nothing here
    /// can be dragged, connected or edited.
    /// </summary>
    internal sealed class StateMachineNodeView : VisualElement
    {
        /// <summary>The state this box stands for.</summary>
        internal string StateName { get; }

        /// <summary>Where the box sits on the canvas, in canvas space.</summary>
        internal Rect Area { get; }

        /// <summary>Builds a box for one state.</summary>
        /// <param name="stateName">The state this box stands for.</param>
        /// <param name="position">Where the box sits on the canvas.</param>
        /// <param name="isInitial">True when the machine started in this state.</param>
        internal StateMachineNodeView(string stateName, Vector2 position, bool isInitial)
        {
            StateName = stateName;
            Area = new Rect(position, new Vector2(StateMachineLayout.NodeWidth, StateMachineLayout.NodeHeight));

            AddToClassList(StateMachineStyle.NodeClass);

            if (isInitial)
                AddToClassList(StateMachineStyle.InitialNodeClass);

            style.position = Position.Absolute;
            style.left = Area.x;
            style.top = Area.y;
            style.width = Area.width;
            style.height = Area.height;

            Label label = new(stateName);
            label.AddToClassList(StateMachineStyle.NodeLabelClass);

            Add(label);
        }

        /// <summary>Marks or unmarks this box as the state the machine currently sits in.</summary>
        /// <param name="isActive">True while the machine is in this state.</param>
        internal void SetActive(bool isActive) => EnableInClassList(StateMachineStyle.ActiveNodeClass, isActive);
    }
}