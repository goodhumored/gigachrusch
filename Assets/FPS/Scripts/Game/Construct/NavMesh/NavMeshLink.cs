using System.Collections.Generic;

namespace UnityEngine.AI
{
    /// <summary>
    /// Component used to create a navigable link between two NavMesh locations.
    /// </summary>
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-101)]
    [AddComponentMenu("Navigation/NavMeshLink", 33)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshLink : MonoBehaviour
    {
        [SerializeField]
        int AgentTypeID;
        /// <summary> Gets or sets the type of agent that can use the link. </summary>
        public int agentTypeID { get { return AgentTypeID; } set { AgentTypeID = value; UpdateLink(); } }

        [SerializeField]
        Vector3 StartPoint = new Vector3(0.0f, 0.0f, -2.5f);
        /// <summary> Gets or sets the position at the middle of the link's start edge. </summary>
        /// <remarks> The position is relative to the GameObject transform. </remarks>
        public Vector3 startPoint { get { return StartPoint; } set { StartPoint = value; UpdateLink(); } }

        [SerializeField]
        Vector3 EndPoint = new Vector3(0.0f, 0.0f, 2.5f);
        /// <summary> Gets or sets the position at the middle of the link's end edge. </summary>
        /// <remarks> The position is relative to the GameObject transform. </remarks>
        public Vector3 endPoint { get { return EndPoint; } set { EndPoint = value; UpdateLink(); } }

        [SerializeField]
        float Width;
        /// <summary> The width of the segments making up the ends of the link. </summary>
        /// <remarks> The segments are created perpendicular to the line from start to end. </remarks>
        public float width { get { return Width; } set { Width = value; UpdateLink(); } }

        [SerializeField]
        int CostModifier = -1;
        /// <summary> Gets or sets a value that determines the cost of traversing the link.</summary>
        /// <remarks> A negative value implies that the traversal cost is obtained based on the area type.
        /// A positive or zero value applies immediately, overridding the cost associated with the area type.</remarks>
        public int costModifier { get { return CostModifier; } set { CostModifier = value; UpdateLink(); } }

        [SerializeField]
        bool Bidirectional = true;
        /// <summary> Gets or sets whether the link can be traversed in both directions. </summary>
        public bool bidirectional { get { return Bidirectional; } set { Bidirectional = value; UpdateLink(); } }

        [SerializeField]
        bool AutoUpdatePosition;
        /// <summary> Gets or sets whether the world positions of the link's edges update whenever
        /// the GameObject transform changes at runtime. </summary>
        public bool autoUpdate { get { return AutoUpdatePosition; } set { SetAutoUpdate(value); } }

        [SerializeField]
        int Area;
        /// <summary> The area type of the link. </summary>
        public int area { get { return Area; } set { Area = value; UpdateLink(); } }

        NavMeshLinkInstance LinkInstance = new NavMeshLinkInstance();

        Vector3 LastPosition = Vector3.zero;
        Quaternion LastRotation = Quaternion.identity;

        static readonly List<NavMeshLink> s_Tracked = new List<NavMeshLink>();

        void OnEnable()
        {
            AddLink();
            if (AutoUpdatePosition && LinkInstance.valid)
                AddTracking(this);
        }

        void OnDisable()
        {
            RemoveTracking(this);
            LinkInstance.Remove();
        }

        /// <summary> Replaces the link with a new one using the current settings. </summary>
        public void UpdateLink()
        {
            LinkInstance.Remove();
            AddLink();
        }

        static void AddTracking(NavMeshLink link)
        {
#if UNITY_EDITOR
            if (s_Tracked.Contains(link))
            {
                Debug.LogError("Link is already tracked: " + link);
                return;
            }
#endif

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate += UpdateTrackedInstances;

            s_Tracked.Add(link);
        }

        static void RemoveTracking(NavMeshLink link)
        {
            s_Tracked.Remove(link);

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate -= UpdateTrackedInstances;
        }

        void SetAutoUpdate(bool value)
        {
            if (AutoUpdatePosition == value)
                return;
            AutoUpdatePosition = value;
            if (value)
                AddTracking(this);
            else
                RemoveTracking(this);
        }

        void AddLink()
        {
#if UNITY_EDITOR
            if (LinkInstance.valid)
            {
                Debug.LogError("Link is already added: " + this);
                return;
            }
#endif

            var link = new NavMeshLinkData();
            link.startPosition = StartPoint;
            link.endPosition = EndPoint;
            link.width = Width;
            link.costModifier = CostModifier;
            link.bidirectional = Bidirectional;
            link.area = Area;
            link.agentTypeID = AgentTypeID;
            LinkInstance = NavMesh.AddLink(link, transform.position, transform.rotation);
            if (LinkInstance.valid)
                LinkInstance.owner = this;

            LastPosition = transform.position;
            LastRotation = transform.rotation;
        }

        bool HasTransformChanged()
        {
            if (LastPosition != transform.position)
                return true;
            if (LastRotation != transform.rotation)
                return true;
            return false;
        }

        void OnDidApplyAnimationProperties()
        {
            UpdateLink();
        }

        static void UpdateTrackedInstances()
        {
            foreach (var instance in s_Tracked)
            {
                if (instance.HasTransformChanged())
                    instance.UpdateLink();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Width = Mathf.Max(0.0f, Width);

            if (!LinkInstance.valid)
                return;

            UpdateLink();

            if (!AutoUpdatePosition)
            {
                RemoveTracking(this);
            }
            else if (!s_Tracked.Contains(this))
            {
                AddTracking(this);
            }
        }
#endif
    }
}
