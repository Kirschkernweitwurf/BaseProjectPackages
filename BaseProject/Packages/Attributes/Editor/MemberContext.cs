using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Everything a handler needs about a single inspected member, passed by reference.
    /// </summary>

    // Load bearing and concrete on purpose. This is the argument every handler in the pipeline takes,
    // so it is a parameter object, not a service. An interface in front of it would box a readonly
    // struct on every handler call, once per member per repaint, to abstract a set of fields.
    internal readonly struct MemberContext
    {
        /// <summary>The serialized property being drawn.</summary>
        internal readonly SerializedProperty Property;

        /// <summary>The reflected field behind the property, or null if it could not be resolved.</summary>
        internal readonly FieldInfo Field;

        /// <summary>The primary inspected Unity object.</summary>
        internal readonly Object Target;

        /// <summary>
        /// The type that declares <see cref="Field"/>. Equals the target type for top-level members and
        /// the nested serializable type when the pipeline descends into it.
        /// </summary>
        internal readonly Type DeclaringType;

        /// <summary>
        /// The managed instance that owns <see cref="Field"/>. Equals <see cref="Target"/> for top-level
        /// members and the nested instance when the pipeline descends into it. Used for reflection-based
        /// conditions and validation.
        /// </summary>
        internal readonly object DeclaringObject;

        /// <summary>The active editor, for access to serializedObject, targets and Repaint.</summary>
        internal readonly UnityEditor.Editor Editor;

        /// <summary>The object reference value captured before the field was drawn this frame.</summary>
        internal readonly Object ObjectReferenceBefore;

        // Only EffectiveLabel reads this. A drawer that wants to know asks for the label it should
        // pass rather than for the flag behind it.
        private readonly bool _showLabel;

        /// <summary>Creates a context for a single member.</summary>
        /// <param name="showLabel">False while the member is drawn without its label.</param>
        internal MemberContext(SerializedProperty property,
            FieldInfo field,
            Object target,
            Type declaringType,
            object declaringObject,
            UnityEditor.Editor editor,
            Object objectReferenceBefore,
            bool showLabel = true)
        {
            Property = property;
            Field = field;
            Target = target;
            DeclaringType = declaringType;
            DeclaringObject = declaringObject;
            Editor = editor;
            ObjectReferenceBefore = objectReferenceBefore;
            _showLabel = showLabel;
        }

        /// <summary>Returns the field attribute of the given type, or null. Cached per field.</summary>
        internal T GetAttribute<T>() where T : Attribute => ReflectionCache.GetAttribute<T>(Field);

        /// <summary>
        /// Human-readable label for the member. A <see cref="LabelAttribute"/> replaces the name Unity
        /// derives from the field, and a member reference in it is resolved.
        /// </summary>
        internal string DisplayName
        {
            get
            {
                LabelAttribute label = GetAttribute<LabelAttribute>();

                return label == null
                    ? ObjectNames.NicifyVariableName(Property.name)
                    : ValueResolver.Text(this, label.Text);
            }
        }

        /// <summary>
        /// Label and tooltip for the member. Drawers that build their own header have to use this rather
        /// than <see cref="DisplayName"/>, or the field silently loses its tooltip.
        /// </summary>
        internal GUIContent Label => new(DisplayName, GetAttribute<TooltipAttribute>()?.tooltip);

        /// <summary>
        /// The label actually passed to the field. Empty inside a horizontal cell that asked to hide it,
        /// so the value gets the whole width rather than sharing it with a label nobody needs twice.
        /// </summary>
        internal GUIContent EffectiveLabel => _showLabel
            ? Label
            : GUIContent.none;

        /// <summary>
        /// Finds a sibling property by name, relative to this member's path. Resolves top-level members
        /// for top-level fields and members of the same nested object when descended.
        /// </summary>
        internal SerializedProperty FindSiblingProperty(string member)
        {
            string path = Property.propertyPath;
            int separator = path.LastIndexOf('.');
            string siblingPath = separator < 0
                ? member
                : path.Substring(0, separator + 1) + member;

            return Editor.serializedObject.FindProperty(siblingPath);
        }
    }
}