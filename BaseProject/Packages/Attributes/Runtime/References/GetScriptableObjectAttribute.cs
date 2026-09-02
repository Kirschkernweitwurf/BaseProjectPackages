using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Fills an empty ScriptableObject reference with the first asset of that type in the project. Meant
    /// for the configs that exist exactly once and get dragged into forty places by hand.
    /// </summary>
    /// <remarks>
    /// The search is cached per type and only runs while the field is empty, because asking the asset
    /// database anything is expensive and the inspector redraws constantly. A project with more than one
    /// matching asset gets whichever the database returns first, so use this where "there is only one"
    /// is actually true.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GetScriptableObjectAttribute : PropertyAttribute { }
}