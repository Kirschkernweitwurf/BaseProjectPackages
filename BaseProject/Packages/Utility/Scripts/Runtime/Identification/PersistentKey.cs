using System;

namespace Base.UtilityPackage.Identification
{
    /// <summary>
    /// A stable, human-authored key that survives being written to disk and read back again.
    /// Used wherever a persisted entry must be matched to the code that owns it, such as a
    /// settings key or a save entry. A validated value type rather than a raw string:
    /// construction enforces the format in one place, the type documents intent at every call
    /// site, and value equality makes it a safe dictionary key. The underlying string is what is
    /// written to disk, so the persisted format is unchanged and remains independent of code
    /// structure.
    /// </summary>
    /// <remarks>
    /// A valid key is non-null, non-empty, free of leading or trailing whitespace, free of control
    /// characters, and free of path separators ('/', '\') and quotes so it stays safe to compose
    /// into storage keys. The recommended pattern is to expose one <c>static readonly <see cref="PersistentKey"/></c>
    /// per owner as that owner's single source of truth.
    /// </remarks>
    public readonly struct PersistentKey : IEquatable<PersistentKey>
    {
        /// <summary>Maximum length of a key, guarding against accidental large keys.</summary>
        private const int MaxLength = 128;

        /// <summary>The underlying string, or <c>null</c> for an uninitialized key.</summary>
        public string Value { get; }

        /// <summary>True for the <c>default</c> value, which carries no key.</summary>
        public bool IsEmpty => string.IsNullOrEmpty(Value);

        /// <param name="value">The key text.</param>
        /// <exception cref="ArgumentException">The value does not satisfy the key format.</exception>
        public PersistentKey(string value)
        {
            if (!TryValidate(value, out string error))
                throw new ArgumentException(error, nameof(value));

            Value = value;
        }

        /// <summary>
        /// Creates a key without throwing. Use at trust boundaries such as parsing user input.
        /// </summary>
        public static bool TryCreate(string value, out PersistentKey key)
        {
            if (!TryValidate(value, out _))
            {
                key = default;
                return false;
            }

            key = new PersistentKey(value);
            return true;
        }

        /// <summary>Reports whether a string would be accepted as a key.</summary>
        public static bool IsValid(string value) => TryValidate(value, out _);

        public bool Equals(PersistentKey other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is PersistentKey other && Equals(other);

        public override int GetHashCode() =>
            Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public static bool operator ==(PersistentKey left, PersistentKey right) => left.Equals(right);

        public static bool operator !=(PersistentKey left, PersistentKey right) => !left.Equals(right);

        public override string ToString() => Value ?? string.Empty;

        private static bool TryValidate(string value, out string error)
        {
            if (string.IsNullOrEmpty(value))
            {
                error = "A PersistentKey must not be null or empty.";
                return false;
            }

            if (value.Length > MaxLength)
            {
                error = $"A PersistentKey must be at most {MaxLength} characters long.";
                return false;
            }

            if (value[0] == ' ' || value[^1] == ' ' || char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
            {
                error = "A PersistentKey must not have leading or trailing whitespace.";
                return false;
            }

            foreach (char c in value)
            {
                if (char.IsControl(c) || c is '/' or '\\' or '"' or '\'')
                {
                    error = "A PersistentKey must not contain control characters, path separators or quotes.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}