using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Greys out a <see cref="PrefixToggleAttribute"/> field while its checkbox is off, so the toggle
    /// reads as switching the value on rather than as an unrelated bool sitting next to it.
    /// </summary>
    internal sealed class PrefixToggleEnableHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
        {
            SerializedProperty toggle = PrefixToggleState.ResolveToggle(context);

            return toggle == null || toggle.boolValue;
        }
    }
}