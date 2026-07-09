using Base.AttributePackage.Conditional;
using Base.AttributePackage.Editor.Core;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Hides the field unless the referenced enum member equals one of the given values.</summary>
    public sealed class ShowIfEnumHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
        {
            ShowIfEnumAttribute attribute = context.GetAttribute<ShowIfEnumAttribute>();
            if (attribute == null)
                return true;

            object current = ConditionEvaluator.ResolveEnum(context, attribute.Member);
            if (current == null)
                return true;

            foreach (object value in attribute.Values)
            {
                if (Equals(current, value))
                    return true;
            }

            return false;
        }
    }
}