using UnityEngine;

namespace Base.ToolsPackage.AssetZoo
{
    /// <summary>
    /// One place in a built zoo and what was measured there. Keeping the measurement on the object means the
    /// zoo can be arranged again and its labels drawn without loading or measuring a single prefab, also after
    /// the scene was closed.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")] // hidden from the Add Component menu; only the builder adds this
    public sealed class ZooItem : MonoBehaviour
    {
        [Tooltip("The prefab or model shown here.")]
        [SerializeField] private GameObject asset;

        [Tooltip("The instance of the asset in the scene.")]
        [SerializeField] private Transform content;

        [Tooltip("Where the asset sits in the group's own order.")]
        [SerializeField] private int listIndex;

        [Tooltip("How many different materials the asset uses.")]
        [SerializeField] private int materialCount;

        [Tooltip("The size of what you see, measured with the asset's pivot on this object.")]
        [SerializeField] private Bounds measuredBounds;

        [Tooltip("What an artist should look at on this asset. Empty when nothing stands out.")]
        [SerializeField] private string problem;

        [Tooltip("How much floor the asset takes in the current layout, x across and y into the zoo.")]
        [SerializeField] private Vector2 footprint;

        [Tooltip("The asset's triangles, counting only the highest level of detail.")]
        [SerializeField] private int triangleCount;

        [Tooltip("True for the scale figure, which stands in every group and is no asset of its own.")]
        [SerializeField] private bool isFigure;

        [Tooltip("True when the asset was found by the last build and not by the one before.")]
        [SerializeField] private bool isNew;

        /// <summary>The prefab or model shown here.</summary>
        public GameObject Asset => asset;

        /// <summary>The instance of the asset in the scene.</summary>
        public Transform Content => content;

        /// <summary>How much floor the asset takes, x across and y into the zoo.</summary>
        public Vector2 Footprint => footprint;

        /// <summary>True for the scale figure, which stands in every group and is no asset of its own.</summary>
        public bool IsFigure => isFigure;

        /// <summary>True when the asset was found by the last build and not by the one before.</summary>
        public bool IsNew => isNew;

        /// <summary>Where the asset sits in the group's own order.</summary>
        public int ListIndex => listIndex;

        /// <summary>How many different materials the asset uses.</summary>
        public int MaterialCount => materialCount;

        /// <summary>The size of what you see, measured with the asset's pivot on this object.</summary>
        public Bounds MeasuredBounds => measuredBounds;

        /// <summary>What an artist should look at on this asset. Empty when nothing stands out.</summary>
        public string Problem => problem;

        /// <summary>The asset's triangles, counting only the highest level of detail.</summary>
        public int TriangleCount => triangleCount;

        /// <summary>The box around what you see, as it stands in the zoo right now, in world space.</summary>
        public Bounds WorldBounds
        {
            get
            {
                Vector3 offset = content != null
                    ? content.localPosition
                    : Vector3.zero;

                return new Bounds(transform.TransformPoint(measuredBounds.center + offset), measuredBounds.size);
            }
        }

        /// <summary>Fills in what the builder measured.</summary>
        /// <param name="newAsset">The prefab or model shown here.</param>
        /// <param name="newContent">Its instance in the scene.</param>
        /// <param name="bounds">The size of what you see.</param>
        /// <param name="triangles">The asset's triangles.</param>
        /// <param name="materials">How many different materials it uses.</param>
        /// <param name="newProblem">What to look at, or empty.</param>
        public void SetMeasurement(GameObject newAsset, Transform newContent, Bounds bounds, int triangles,
            int materials, string newProblem)
        {
            asset = newAsset;
            content = newContent;
            measuredBounds = bounds;
            triangleCount = triangles;
            materialCount = materials;
            problem = newProblem;
        }

        /// <summary>Fills in where the item belongs in its group.</summary>
        /// <param name="index">Its place in the group's own order.</param>
        /// <param name="figure">True for the scale figure.</param>
        /// <param name="newlyFound">True when the last build found it for the first time.</param>
        public void SetRole(int index, bool figure, bool newlyFound)
        {
            listIndex = index;
            isFigure = figure;
            isNew = newlyFound;
        }

        /// <summary>Moves the instance so the asset stands the way the zoo wants it to.</summary>
        /// <param name="offset">Where the instance goes, relative to this object.</param>
        /// <param name="newFootprint">How much floor the asset takes.</param>
        public void SetPlacement(Vector3 offset, Vector2 newFootprint)
        {
            footprint = newFootprint;

            if (content != null)
                content.localPosition = offset;
        }
    }
}