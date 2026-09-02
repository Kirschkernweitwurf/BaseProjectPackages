using Base.ServicesPackage;
using Base.SettingsPackage.Core;
using Base.UtilityPackage.Identification;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Non-generic base for every setting component. Lets settings of different value types be discovered and
    /// inspected polymorphically (for example via <see cref="Object.FindObjectsByType{T}(FindObjectsSortMode)"/>).
    /// Concrete components inherit from <see cref="TypedSettingComponent{TValue, TSetting}"/>, not this type.
    /// </summary>
    public abstract class SettingComponent : MonoBehaviour
    {
        /// <summary>The key under which this component registers its setting.</summary>
        public abstract PersistentKey Key { get; }

        /// <summary>
        /// The setting backing this component, or null until <see cref="Awake"/> has resolved the context.
        /// </summary>
        public abstract ISetting Setting { get; }

#region Unity Callbacks
        protected virtual void Awake()
        {
            // TryGet reports a missing context itself; disabling keeps it from being reported again later.
            if (!ServiceLocator.TryGet(out SettingsContext context))
            {
                enabled = false;
                return;
            }

            RegisterAndSubscribe(context);
        }

        protected virtual void OnDestroy() => Unsubscribe();
#endregion

        /// <summary>Creates the setting, registers it on the context, and subscribes the applier.</summary>
        protected abstract void RegisterAndSubscribe(SettingsContext context);

        /// <summary>Detaches the applier from the setting.</summary>
        protected abstract void Unsubscribe();
    }
}