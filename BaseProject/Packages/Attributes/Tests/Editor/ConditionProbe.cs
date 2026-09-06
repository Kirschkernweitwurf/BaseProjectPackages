namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Carries one member of every shape a condition can be pointed at: a public field, a private
    /// field, a property, a method, and a member that is not a boolean at all.
    /// </summary>
    /// <remarks>
    /// Public because the condition evaluator reaches these through reflection from another assembly.
    /// </remarks>
    public sealed class ConditionProbe
    {
        /// <summary>Name of the private field, so a test can point a condition at it by name.</summary>
        public const string PrivateFlagName = nameof(_privateFlag);

        /// <summary>A property the condition can read.</summary>
        public bool PropertyFlag { get; set; }

        /// <summary>A public field the condition can read.</summary>
        public bool PublicFlag;

        /// <summary>A member that is not a boolean, which a condition cannot read.</summary>
        public int Count;

        private bool _privateFlag;

        /// <summary>Sets the private field, so a test can drive it from outside.</summary>
        /// <param name="value">The value to store.</param>
        public void SetPrivateFlag(bool value) => _privateFlag = value;

        /// <summary>A method the condition can call.</summary>
        /// <returns>Whatever the public field currently holds.</returns>
        public bool MethodFlag() => PublicFlag;

        /// <summary>A method that takes an argument, which a condition cannot call.</summary>
        /// <param name="value">Ignored.</param>
        /// <returns>The given value.</returns>
        public bool MethodWithArgument(bool value) => value;
    }
}