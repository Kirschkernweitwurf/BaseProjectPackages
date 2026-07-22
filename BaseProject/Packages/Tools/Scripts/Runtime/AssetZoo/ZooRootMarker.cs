using UnityEngine;

namespace Base.ToolPackage.AssetZoo
{
    /// <summary>
    /// Empty marker component attached to a built zoo's root GameObject. Lets the
    /// builder identify "its" object via type lookup instead of name matching,
    /// which avoids collisions with user-named GameObjects.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")] // hidden from the Add Component menu; only the builder adds this
    public class ZooRootMarker : MonoBehaviour { }
}