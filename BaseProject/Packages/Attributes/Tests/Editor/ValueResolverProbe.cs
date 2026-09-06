namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Carries one member of every shape a member reference can name: a public field, a private field,
    /// a field left unset, a property, a method, and two methods that cannot answer.
    /// </summary>
    /// <remarks>
    /// Public and not a Unity object, matching the other reflection probe in this assembly. The
    /// resolver reaches these from the editor assembly, and a nested serializable type is exactly the
    /// case where the owner is a plain object rather than something selectable.
    /// </remarks>
    public sealed class ValueResolverProbe
    {
        /// <summary>The text a method returns.</summary>
        public const string MethodValue = "method";

        /// <summary>Name of the private field, so a test can reference it without a literal.</summary>
        public const string PrivateTextName = nameof(_privateText);

        /// <summary>The text a private field holds.</summary>
        public const string PrivateValue = "private";

        /// <summary>The text a property returns.</summary>
        public const string PropertyValue = "property";

        /// <summary>The text a public field holds.</summary>
        public const string PublicValue = "public";

        /// <summary>A readable property the resolver can read.</summary>
        public string TextProperty => PropertyValue;

        /// <summary>A public field the resolver can read.</summary>
        public string PublicText = PublicValue;

        /// <summary>A field nobody assigned, so reading it yields nothing rather than text.</summary>
        public string MissingText;

        private string _privateText = PrivateValue;

        /// <summary>A method the resolver can call.</summary>
        /// <returns>A fixed piece of text.</returns>
        public string TextMethod() => MethodValue;

        /// <summary>A method that returns nothing, so there is no value to show.</summary>
        public void VoidMethod() { }

        /// <summary>A method that needs an argument, so the resolver cannot call it.</summary>
        /// <param name="value">Ignored.</param>
        /// <returns>The given value.</returns>
        public string MethodWithArgument(string value) => value;
    }
}