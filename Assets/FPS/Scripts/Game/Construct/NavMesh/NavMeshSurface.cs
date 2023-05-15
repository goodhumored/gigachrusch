using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.AI
{
    /// <summary> Sets the method for filtering the objects retrieved when baking the NavMesh. </summary>
    public enum CollectObjects
    {
        /// <summary> Use all the active objects. </summary>
        All = 0,
        /// <summary> Use all the active objects that overlap the bounding volume. </summary>
        Volume = 1,
        /// <summary> Use all the active objects that are children of this GameObject. </summary>
        /// <remarks> This includes the current GameObject and all the children of the children that are active.</remarks>
        Children = 2,
    }

    /// <summary> Component used for building and enabling a NavMesh surface for one agent type. </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-102)]
    [AddComponentMenu("Navigation/NavMeshSurface", 30)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshSurface : MonoBehaviour
    {
        [SerializeField]
        int AgentTypeID;
        /// <summary> Gets or sets the identifier of the agent type that will use this NavMesh Surface. </summary>
        public int agentTypeID { get { return AgentTypeID; } set { AgentTypeID = value; } }

        [SerializeField]
        CollectObjects CollectObjects = CollectObjects.All;
        /// <summary> Gets or sets the method for retrieving the objects that will be used for baking. </summary>
        public CollectObjects collectObjects { get { return CollectObjects; } set { CollectObjects = value; } }

        [SerializeField]
        Vector3 Size = new Vector3(10.0f, 10.0f, 10.0f);
        /// <summary> Gets or sets the size of the volume that delimits the NavMesh created by this component. </summary>
        /// <remarks> It is used only when <c>collectObjects</c> is set to <c>Volume</c>. The size applies in the local space of the GameObject. </remarks>
        public Vector3 size { get { return Size; } set { Size = value; } }

        [SerializeField]
        Vector3 Center = new Vector3(0, 2.0f, 0);
        /// <summary> Gets or sets the center position of the volume that delimits the NavMesh created by this component. </summary>
        /// <remarks> It is used only when <c>collectObjects</c> is set to <c>Volume</c>. The position applies in the local space of the GameObject. </remarks>
        public Vector3 center { get { return Center; } set { Center = value; } }

        [SerializeField]
        LayerMask LayerMask = ~0;
        /// <summary> Gets or sets a bitmask representing which layers to consider when selecting the objects that will be used for baking the NavMesh. </summary>
        public LayerMask layerMask { get { return LayerMask; } set { LayerMask = value; } }

        [SerializeField]
        NavMeshCollectGeometry UseGeometry = NavMeshCollectGeometry.RenderMeshes;
        /// <summary> Gets or sets which type of component in the GameObjects provides the geometry used for baking the NavMesh. </summary>
        public NavMeshCollectGeometry useGeometry { get { return UseGeometry; } set { UseGeometry = value; } }

        [SerializeField]
        int DefaultArea;
        /// <summary> Gets or sets the area type assigned to any object that does not have one specified. </summary>
        /// <remarks> To customize the area type of an object add a <see cref="NavMeshModifier"/> component and set <see cref="NavMeshModifier.overrideArea"/> to <c>true</c>. The area type information is used when baking the NavMesh. </remarks>
        /// <seealso href="https://docs.unity3d.com/Manual/nav-AreasAndCosts.html"/>
        public int defaultArea { get { return DefaultArea; } set { DefaultArea = value; } }

        [SerializeField]
        bool IgnoreNavMeshAgent = true;
        /// <summary> Gets or sets whether the process of building the NavMesh ignores the GameObjects containing a <see cref="NavMeshAgent"/> component. </summary>
        /// <remarks> There is generally no need for the NavMesh to take into consideration the objects that can move.</remarks>
        public bool ignoreNavMeshAgent { get { return IgnoreNavMeshAgent; } set { IgnoreNavMeshAgent = value; } }

        [SerializeField]
        bool IgnoreNavMeshObstacle = true;
        /// <summary> Gets or sets whether the process of building the NavMesh ignores the GameObjects containing a <see cref="NavMeshObstacle"/> component. </summary>
        /// <remarks> There is generally no need for the NavMesh to take into consideration the objects that can move.</remarks>
        public bool ignoreNavMeshObstacle { get { return IgnoreNavMeshObstacle; } set { IgnoreNavMeshObstacle = value; } }

        [SerializeField]
        bool OverrideTileSize;
        /// <summary> Gets or sets whether the NavMesh building process uses the <see cref="tileSize"/> value. </summary>
        public bool overrideTileSize { get { return OverrideTileSize; } set { OverrideTileSize = value; } }

        [SerializeField]
        int TileSize = 256;
        /// <summary> Gets or sets the width of the square grid of voxels that the NavMesh building process uses for sampling the scene geometry. </summary>
        /// <remarks> This value represents a number of voxels. Together with <see cref="voxelSize"/> it determines the real size of the individual sections that comprise the NavMesh. </remarks>
        public int tileSize { get { return TileSize; } set { TileSize = value; } }

        [SerializeField]
        bool OverrideVoxelSize;
        /// <summary> Gets or sets whether the NavMesh building process uses the <see cref="voxelSize"/> value. </summary>
        public bool overrideVoxelSize { get { return OverrideVoxelSize; } set { OverrideVoxelSize = value; } }

        [SerializeField]
        float VoxelSize;
        /// <summary> Gets or sets the width of the square voxels that the NavMesh building process uses for sampling the scene geometry. </summary>
        /// <remarks> This value is in world units. Together with <see cref="tileSize"/> it determines the real size of the individual sections that comprise the NavMesh. </remarks>
        public float voxelSize { get { return VoxelSize; } set { VoxelSize = value; } }

        [SerializeField]
        bool BuildHeightMesh;
        /// <summary> (Not supported) Gets or sets whether the NavMesh building process produces more detailed elevation information. </summary>
        /// <seealso href="https://docs.unity3d.com/Manual/nav-HeightMesh.html"/>
        [Obsolete("The buildHeightMesh option has never been implemented as originally intended.")]
        public bool buildHeightMesh { get { return BuildHeightMesh; } set { BuildHeightMesh = value; } }

        [UnityEngine.Serialization.FormerlySerializedAs("BakedNavMeshData")]
        [SerializeField]
        NavMeshData NavMeshData;
        /// <summary> Gets or sets the reference to the NavMesh data instantiated by this surface. </summary>
        public NavMeshData navMeshData { get { return NavMeshData; } set { NavMeshData = value; } }

        // Do not serialize - runtime only state.
        NavMeshDataInstance NavMeshDataInstance;
        Vector3 LastPosition = Vector3.zero;
        Quaternion LastRotation = Quaternion.identity;

        static readonly List<NavMeshSurface> s_NavMeshSurfaces = new List<NavMeshSurface>();

        /// <summary> Gets the list of all the <see cref="NavMeshSurface"/> components that are currently active in the scene. </summary>
        public static List<NavMeshSurface> activeSurfaces
        {
            get { return s_NavMeshSurfaces; }
        }

        void OnEnable()
        {
            Register(this);
            AddData();
        }

        void OnDisable()
        {
            RemoveData();
            Unregister(this);
        }

        /// <summary> Creates an instance of the NavMesh data and activates it in the navigation system. </summary>
        /// <remarks> The instance is created at the position and with the orientation of the GameObject. </remarks>
        public void AddData()
        {
#if UNITY_EDITOR
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(this);
            if (isPrefab)
            {
                //Debug.LogFormat("NavMeshData from {0}.{1} will not be added to the NavMesh world because the gameObject is a prefab.",
                //    gameObject.name, name);
                return;
            }
#endif
            if (NavMeshDataInstance.valid)
                return;

            if (NavMeshData != null)
            {
                NavMeshDataInstance = NavMesh.AddNavMeshData(NavMeshData, transform.position, transform.rotation);
                NavMeshDataInstance.owner = this;
            }

            LastPosition = transform.position;
            LastRotation = transform.rotation;
        }

        /// <summary> Removes the instance of this NavMesh data from the navigation system. </summary>
        /// <remarks> This operation does not destroy the <see cref="navMeshData"/>. </remarks>
        public void RemoveData()
        {
            NavMeshDataInstance.Remove();
            NavMeshDataInstance = new NavMeshDataInstance();
        }

        /// <summary> Retrieves a copy of the current settings chosen for building this NavMesh surface. </summary>
        /// <returns> The settings configured in this NavMeshSurface. </returns>
        public NavMeshBuildSettings GetBuildSettings()
        {
            var buildSettings = NavMesh.GetSettingsByID(AgentTypeID);
            if (buildSettings.agentTypeID == -1)
            {
                Debug.LogWarning("No build settings for agent type ID " + agentTypeID, this);
                buildSettings.agentTypeID = AgentTypeID;
            }

            if (overrideTileSize)
            {
                buildSettings.overrideTileSize = true;
                buildSettings.tileSize = tileSize;
            }
            if (overrideVoxelSize)
            {
                buildSettings.overrideVoxelSize = true;
                buildSettings.voxelSize = voxelSize;
            }
            return buildSettings;
        }

        /// <summary> Builds and instantiates this NavMesh surface. </summary>
        public void BuildNavMesh()
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(Center, Abs(Size));
            if (CollectObjects == CollectObjects.All || CollectObjects == CollectObjects.Children)
            {
                sourcesBounds = CalculateWorldBounds(sources);
            }

            var data = NavMeshBuilder.BuildNavMeshData(GetBuildSettings(),
                    sources, sourcesBounds, transform.position, transform.rotation);

            if (data != null)
            {
                data.name = gameObject.name;
                RemoveData();
                NavMeshData = data;
                if (isActiveAndEnabled)
                    AddData();
            }
        }

        /// <summary> Rebuilds parts of an existing NavMesh in the regions of the scene where the objects have changed. </summary>
        /// <remarks> This operation is executed asynchronously. </remarks>
        /// <param name="data"> The NavMesh to update according to the changes in the scene. </param>
        /// <returns> A reference to the asynchronous coroutine that builds the NavMesh. </returns>
        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(Center, Abs(Size));
            if (CollectObjects == CollectObjects.All || CollectObjects == CollectObjects.Children)
                sourcesBounds = CalculateWorldBounds(sources);

            return NavMeshBuilder.UpdateNavMeshDataAsync(data, GetBuildSettings(), sources, sourcesBounds);
        }

        static void Register(NavMeshSurface surface)
        {
#if UNITY_EDITOR
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(surface);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(surface);
            if (isPrefab)
            {
                //Debug.LogFormat("NavMeshData from {0}.{1} will not be added to the NavMesh world because the gameObject is a prefab.",
                //    surface.gameObject.name, surface.name);
                return;
            }
#endif
            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate += UpdateActive;

            if (!s_NavMeshSurfaces.Contains(surface))
                s_NavMeshSurfaces.Add(surface);
        }

        static void Unregister(NavMeshSurface surface)
        {
            s_NavMeshSurfaces.Remove(surface);

            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate -= UpdateActive;
        }

        static void UpdateActive()
        {
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
                s_NavMeshSurfaces[i].UpdateDataIfTransformChanged();
        }

        void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
        {
#if UNITY_EDITOR
            var myStage = StageUtility.GetStageHandle(gameObject);
            if (!myStage.IsValid())
                return;
#endif
            // Modifiers
            List<NavMeshModifierVolume> modifiers;
            if (CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifierVolume>(GetComponentsInChildren<NavMeshModifierVolume>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifierVolume.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(AgentTypeID))
                    continue;
#if UNITY_EDITOR
                if (!myStage.Contains(m.gameObject))
                    continue;
#endif
                var mcenter = m.transform.TransformPoint(m.center);
                var scale = m.transform.lossyScale;
                var msize = new Vector3(m.size.x * Mathf.Abs(scale.x), m.size.y * Mathf.Abs(scale.y), m.size.z * Mathf.Abs(scale.z));

                var src = new NavMeshBuildSource();
                src.shape = NavMeshBuildSourceShape.ModifierBox;
                src.transform = Matrix4x4.TRS(mcenter, m.transform.rotation, Vector3.one);
                src.size = msize;
                src.area = m.area;
                sources.Add(src);
            }
        }

        List<NavMeshBuildSource> CollectSources()
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            List<NavMeshModifier> modifiers;
            if (CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifier.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(AgentTypeID))
                    continue;
                var markup = new NavMeshBuildMarkup();
                markup.root = m.transform;
                markup.overrideArea = m.overrideArea;
                markup.area = m.area;
                markup.ignoreFromBuild = m.ignoreFromBuild;
                markups.Add(markup);
            }

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                if (CollectObjects == CollectObjects.All)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        null, LayerMask, UseGeometry, DefaultArea, markups, gameObject.scene, sources);
                }
                else if (CollectObjects == CollectObjects.Children)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        transform, LayerMask, UseGeometry, DefaultArea, markups, gameObject.scene, sources);
                }
                else if (CollectObjects == CollectObjects.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, new Bounds(Center, Size));

                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        worldBounds, LayerMask, UseGeometry, DefaultArea, markups, gameObject.scene, sources);
                }
            }
            else
#endif
            {
                if (CollectObjects == CollectObjects.All)
                {
                    NavMeshBuilder.CollectSources(null, LayerMask, UseGeometry, DefaultArea, markups, sources);
                }
                else if (CollectObjects == CollectObjects.Children)
                {
                    NavMeshBuilder.CollectSources(transform, LayerMask, UseGeometry, DefaultArea, markups, sources);
                }
                else if (CollectObjects == CollectObjects.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, new Bounds(Center, Size));
                    NavMeshBuilder.CollectSources(worldBounds, LayerMask, UseGeometry, DefaultArea, markups, sources);
                }
            }

            if (IgnoreNavMeshAgent)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshAgent>() != null));

            if (IgnoreNavMeshObstacle)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshObstacle>() != null));

            AppendModifierVolumes(ref sources);

            return sources;
        }

        static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
        {
            var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
            var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
            var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
            var worldPosition = mat.MultiplyPoint(bounds.center);
            var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
            return new Bounds(worldPosition, worldSize);
        }

        Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
        {
            // Use the unscaled matrix for the NavMeshSurface
            Matrix4x4 worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            worldToLocal = worldToLocal.inverse;

            var result = new Bounds();
            foreach (var src in sources)
            {
                switch (src.shape)
                {
                    case NavMeshBuildSourceShape.Mesh:
                        {
                            var m = src.sourceObject as Mesh;
                            result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, m.bounds));
                            break;
                        }
                    case NavMeshBuildSourceShape.Terrain:
                        {
#if NMC_CAN_ACCESS_TERRAIN
                            // Terrain pivot is lower/left corner - shift bounds accordingly
                            var t = src.sourceObject as TerrainData;
                            result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(0.5f * t.size, t.size)));
#else
                            Debug.LogWarning("The NavMesh cannot be properly baked for the terrain because the necessary functionality is missing. Add the com.unity.modules.terrain package through the Package Manager.");
#endif
                            break;
                        }
                    case NavMeshBuildSourceShape.Box:
                    case NavMeshBuildSourceShape.Sphere:
                    case NavMeshBuildSourceShape.Capsule:
                    case NavMeshBuildSourceShape.ModifierBox:
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(Vector3.zero, src.size)));
                        break;
                }
            }
            // Inflate the bounds a bit to avoid clipping co-planar sources
            result.Expand(0.1f);
            return result;
        }

        bool HasTransformChanged()
        {
            if (LastPosition != transform.position)
                return true;
            if (LastRotation != transform.rotation)
                return true;
            return false;
        }

        void UpdateDataIfTransformChanged()
        {
            if (HasTransformChanged())
            {
                RemoveData();
                AddData();
            }
        }

#if UNITY_EDITOR
        bool UnshareNavMeshAsset()
        {
            // Nothing to unshare
            if (NavMeshData == null)
                return false;

            // Prefab parent owns the asset reference
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPersistentObject = EditorUtility.IsPersistent(this);
            if (isInPreviewScene || isPersistentObject)
                return false;

            // An instance can share asset reference only with its prefab parent
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(this) as NavMeshSurface;
            if (prefab != null && prefab.navMeshData == navMeshData)
                return false;

            // Don't allow referencing an asset that's assigned to another surface
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
            {
                var surface = s_NavMeshSurfaces[i];
                if (surface != this && surface.NavMeshData == NavMeshData)
                    return true;
            }

            // Asset is not referenced by known surfaces
            return false;
        }

        void OnValidate()
        {
            if (UnshareNavMeshAsset())
            {
                Debug.LogWarning("Duplicating NavMeshSurface does not duplicate the referenced navmesh data", this);
                NavMeshData = null;
            }

            var settings = NavMesh.GetSettingsByID(AgentTypeID);
            if (settings.agentTypeID != -1)
            {
                // When unchecking the override control, revert to automatic value.
                const float kMinVoxelSize = 0.01f;
                if (!OverrideVoxelSize)
                    VoxelSize = settings.agentRadius / 3.0f;
                if (VoxelSize < kMinVoxelSize)
                    VoxelSize = kMinVoxelSize;

                // When unchecking the override control, revert to default value.
                const int kMinTileSize = 16;
                const int kMaxTileSize = 1024;
                const int kDefaultTileSize = 256;

                if (!OverrideTileSize)
                    TileSize = kDefaultTileSize;
                // Make sure tilesize is in sane range.
                if (TileSize < kMinTileSize)
                    TileSize = kMinTileSize;
                if (TileSize > kMaxTileSize)
                    TileSize = kMaxTileSize;
            }
        }
#endif
    }
}
