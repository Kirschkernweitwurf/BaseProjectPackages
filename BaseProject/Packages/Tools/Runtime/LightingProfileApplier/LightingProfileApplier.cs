using Base.AttributePackage;
using UnityEngine;

namespace Base.ToolPackage.LightingProfileApplier
{
    /// <summary>
    /// Applies a lighting profile as soon as the scene holding this component is loaded.
    /// </summary>
    public class LightingProfileApplier : MonoBehaviour
    {
        [Required] [SerializeField] private LightingProfile profile;
        [SerializeField] private Light sun;

#region Unity Callbacks
        private void Awake()
        {
            profile.Apply();

            if (sun != null)
                RenderSettings.sun = sun;
        }
#endregion
    }
}