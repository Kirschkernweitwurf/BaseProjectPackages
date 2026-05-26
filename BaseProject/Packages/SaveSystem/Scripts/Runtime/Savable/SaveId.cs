using System;

namespace Base.SaveSystemPackage.Savable
{
    /// <summary>
    /// The stable key that links a saved entry to the <see cref="ISavable"/> that owns it.
    /// A validated value type rather than a raw string: construction enforces the format in one
    /// place, the type documents intent at every call site, and value equality makes it a safe
    /// dictionary key. The underlying string is what is written to disk, so the persisted format
    /// is unchanged and remains independent of code structure.
    /// </summary>
    /// <remarks>
    /// A valid id is non-null, non-empty, free of leading or trailing whitespace, free of control
    /// characters, and free of path separators ('/', '\') and quotes so it stays safe to compose
    /// into storage keys. The recommended pattern is to expose one <c>static readonly SaveId</c>
    /// per savable as that savable's single source of truth.
    /// </remarks>
    public readonly struct SaveId : IEquatable<SaveId>
    {
        /// <summary>Maximum length of an id, guarding against accidental large keys.</summary>
        public const int MaxLength = 128;

        private readonly string _value;

        /// <summary>The underlying string, or <c>null</c> for an uninitialized id.</summary>
        public string Value => _value;

        /// <summary>True for the <c>default</c> value, which carries no id.</summary>
        public bool IsEmpty => string.IsNullOrEmpty(_value);

        /// <param name="value">The id text.</param>
        /// <exception cref="ArgumentException">The value does not satisfy the id format.</exception>
        public SaveId(string value)
        {
            if (!TryValidate(value, out string error))
                throw new ArgumentException(error, nameof(value));

            _value = value;
        }

        /// <summary>
        /// Creates an id without throwing. Use at trust boundaries such as parsing user input.
        /// </summary>
        public static bool TryCreate(string value, out SaveId id)
        {
            if (!TryValidate(value, out _))
            {
                id = default;
                return false;
            }

            id = new SaveId(value);
            return true;
        }

        /// <summary>Reports whether a string would be accepted as an id.</summary>
        public static bool IsValid(string value) => TryValidate(value, out _);

        public bool Equals(SaveId other) => string.Equals(_value, other._value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is SaveId other && Equals(other);

        public override int GetHashCode() =>
            _value is null ? 0 : StringComparer.Ordinal.GetHashCode(_value);

        public static bool operator ==(SaveId left, SaveId right) => left.Equals(right);

        public static bool operator !=(SaveId left, SaveId right) => !left.Equals(right);

        public override string ToString() => _value ?? string.Empty;

        private static bool TryValidate(string value, out string error)
        {
            if (string.IsNullOrEmpty(value))
            {
                error = "A SaveId must not be null or empty.";
                return false;
            }

            if (value.Length > MaxLength)
            {
                error = $"A SaveId must be at most {MaxLength} characters long.";
                return false;
            }

            if (value[0] == ' ' || value[^1] == ' ' || char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
            {
                error = "A SaveId must not have leading or trailing whitespace.";
                return false;
            }

            foreach (char c in value)
            {
                if (char.IsControl(c) || c is '/' or '\\' or '"' or '\'')
                {
                    error = "A SaveId must not contain control characters, path separators or quotes.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
