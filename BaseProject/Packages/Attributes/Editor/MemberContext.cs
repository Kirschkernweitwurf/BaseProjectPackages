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
        public readonly SerializedProperty Property;

        /// <summary>The reflected field behind the property, or null if it could not be resolved.</summary>
        public readonly FieldInfo Field;

        /// <summary>The primary inspected Unity object.</summary>
        public readonly Object Target;

        /// <summary>
        /// The type that declares <see cref="Field"/>. Equals the target type for top-level members and
        /// the nested serializable type when the pipeline descends into it.
        /// </summary>
        public readonly Type DeclaringType;

        /// <summary>
        /// The managed instance that owns <see cref="Field"/>. Equals <see cref="Target"/> for top-level
        /// members and the nested instance when the pipeline descends into it. Used for reflection-based
        /// conditions and validation.
        /// </summary>
        public readonly object DeclaringObject;

        /// <summary>The active editor, for access to serializedObject, targets and Repaint.</summary>
        public readonly UnityEditor.Editor Editor;

        /// <summary>The object reference value captured before the field was drawn this frame.</summary>
        public readonly Object ObjectReferenceBefore;

        /// <summary>False while the member is drawn without its label, inside a horizontal cell.</summary>
        public readonly bool ShowLabel;

        /// <summary>Creates a context for a single member.</summary>
        /// <param name="showLabel">False while the member is drawn without its label.</param>
        public MemberContext(SerializedProperty property,
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
            ShowLabel = showLabel;
        }

        /// <summary>Returns the field attribute of the given type, or null. Cached per field.</summary>
        public T GetAttribute<T>() where T : Attribute => ReflectionCache.GetAttribute<T>(Field);

        /// <summary>
        /// Human-readable label for the member. A <see cref="LabelAttribute"/> replaces the name Unity
        /// derives from the field, and a member reference in it is resolved.
        /// </summary>
        public string DisplayName
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
        public GUIContent Label => new(DisplayName, GetAttribute<TooltipAttribute>()?.tooltip);

        /// <summary>
        /// The label actually passed to the field. Empty inside a horizontal cell that asked to hide it,
        /// so the value gets the whole width rather than sharing it with a label nobody needs twice.
        /// </summary>
        public GUIContent EffectiveLabel => ShowLabel
            ? Label
            : GUIContent.none;

        /// <summary>
        /// Finds a sibling property by name, relative to this member's path. Resolves top-level members
        /// for top-level fields and members of the same nested object when descended.
        /// </summary>
        public SerializedProperty FindSiblingProperty(string member)
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