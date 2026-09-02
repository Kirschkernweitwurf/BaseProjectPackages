using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Carries one field per validation rule, in both the shape that passes and the shape that fails,
    /// so a rule can be pointed at a real field with a real value rather than at a stub.
    /// </summary>
    /// <remarks>
    /// Public fields, because the rules read them through reflection from another assembly and a rule
    /// is meant to run against whatever a component happens to expose.
    /// </remarks>
    public sealed class ValidationProbe
    {
        /// <summary>The message the custom variant reports.</summary>
        public const string CustomMessage = "needs an asset of its own";

        /// <summary>An object reference that has to be filled in.</summary>
        [Required] public GameObject RequiredAsset;

        /// <summary>An object reference with its own message.</summary>
        [Required(CustomMessage)] public GameObject RequiredWithMessage;

        /// <summary>A field carrying no rule at all.</summary>
        public GameObject Unmarked;

        /// <summary>Not an object reference, so the required rule has nothing to check.</summary>
        [Required] public int RequiredNumber;

        /// <summary>A string that has to carry something.</summary>
        [NotNullOrEmpty] public string RequiredText;

        /// <summary>A list that has to hold something.</summary>
        [NotNullOrEmpty] public List<string> RequiredList;

        /// <summary>A list whose entries all have to differ.</summary>
        [Unique] public List<string> UniqueEntries;

        /// <summary>Not a list, so the unique rule has nothing to check.</summary>
        [Unique] public string UniqueText;

        /// <summary>A reference that is only required while the flag below is set.</summary>
        [RequiredIf(nameof(NeedsAsset))] public GameObject ConditionalAsset;

        /// <summary>Drives the conditional requirement.</summary>
        public bool NeedsAsset;

        /// <summary>A reference restricted to a type this probe never assigns.</summary>
        [MustImplement(typeof(Texture2D))] public GameObject WrongType;

        /// <summary>A reference restricted to a type it does satisfy.</summary>
        [MustImplement(typeof(GameObject))] public GameObject RightType;
    }
}