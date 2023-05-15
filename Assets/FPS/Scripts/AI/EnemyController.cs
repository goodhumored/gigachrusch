using System.Collections.Generic;
using FPS.Scripts.Game;
using FPS.Scripts.Game.Managers;
using FPS.Scripts.Game.Shared;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace FPS.Scripts.AI
{
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        [System.Serializable]
        public struct RendererIndexData
        {
            public Renderer Renderer;
            public int MaterialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                Renderer = renderer;
                MaterialIndex = index;
            }
        }

        [Header("Parameters")]
        [Tooltip("The Y height at which the enemy will be automatically killed (if it falls off of the level)")]
        public float SelfDestructYHeight = -20f;

        [Tooltip("The distance at which the enemy considers that it has reached its current path destination point")]
        public float PathReachingRadius = 2f;

        [Tooltip("The speed at which the enemy rotates")]
        public float OrientationSpeed = 10f;

        [Tooltip("Delay after death where the GameObject is destroyed (to allow for animation)")]
        public float DeathDuration = 0f;


        [Header("Weapons Parameters")] [Tooltip("Allow weapon swapping for this enemy")]
        public bool SwapToNextWeapon = false;

        [Tooltip("Time delay between a weapon swap and the next attack")]
        public float DelayAfterWeaponSwap = 0f;

        [Header("Eye color")] [Tooltip("Material for the eye color")]
        public Material EyeColorMaterial;

        [Tooltip("The default color of the bot's eye")] [ColorUsage(true, true)]
        public Color DefaultEyeColor;

        [Tooltip("The attack color of the bot's eye")] [ColorUsage(true, true)]
        public Color AttackEyeColor;

        [Header("Flash on hit")] [Tooltip("The material used for the body of the hoverbot")]
        public Material BodyMaterial;

        [Tooltip("The gradient representing the color of the flash on hit")] [GradientUsage(true)]
        public Gradient OnHitBodyGradient;

        [Tooltip("The duration of the flash on hit")]
        public float FlashOnHitDuration = 0.5f;

        [Header("Sounds")] [Tooltip("Sound played when recieving damages")]
        public AudioClip DamageTick;

        [Header("VFX")] [Tooltip("The VFX prefab spawned when the enemy dies")]
        public GameObject DeathVfx;

        [Tooltip("The point at which the death VFX is spawned")]
        public Transform DeathVfxSpawnPoint;

        [Header("Loot")] [Tooltip("The object this enemy can drop when dying")]
        public GameObject LootPrefab;

        [Tooltip("The chance the object has to drop")] [Range(0, 1)]
        public float DropRate = 1f;

        [Header("Debug Display")] [Tooltip("Color of the sphere gizmo representing the path reaching range")]
        public Color PathReachingRangeColor = Color.yellow;

        [Tooltip("Color of the sphere gizmo representing the attack range")]
        public Color AttackRangeColor = Color.red;

        [Tooltip("Color of the sphere gizmo representing the detection range")]
        public Color DetectionRangeColor = Color.blue;

        public UnityAction onAttack;
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;
        public UnityAction onDamaged;
        public UnityAction onDie;

        List<RendererIndexData> BodyRenderers = new List<RendererIndexData>();
        MaterialPropertyBlock BodyFlashMaterialPropertyBlock;
        float LastTimeDamaged = float.NegativeInfinity;

        RendererIndexData EyeRendererData;
        MaterialPropertyBlock EyeColorMaterialPropertyBlock;

        public PatrolPath PatrolPath { get; set; }
        public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;
        public bool IsTargetInAttackRange => DetectionModule.IsTargetInAttackRange;
        public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;
        public bool HadKnownTarget => DetectionModule.HadKnownTarget;
        public NavMeshAgent NavMeshAgent { get; private set; }
        public DetectionModule DetectionModule { get; private set; }

        int PathDestinationNodeIndex;
        EnemyManager EnemyManager;
        ActorsManager ActorsManager;
        Health Health;
        Actor Actor;
        Collider[] SelfColliders;
        GameFlowManager GameFlowManager;
        bool WasDamagedThisFrame;
        float LastTimeWeaponSwapped = Mathf.NegativeInfinity;
        int CurrentWeaponIndex;
        WeaponController CurrentWeapon;
        WeaponController[] Weapons;
        NavigationModule NavigationModule;

        void Start()
        {
            EnemyManager = FindObjectOfType<EnemyManager>();
            DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(EnemyManager, this);

            ActorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyController>(ActorsManager, this);

            EnemyManager.RegisterEnemy(this);

            Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyController>(Health, this, gameObject);

            Actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, EnemyController>(Actor, this, gameObject);

            NavMeshAgent = GetComponent<NavMeshAgent>();
            SelfColliders = GetComponentsInChildren<Collider>();

            GameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, EnemyController>(GameFlowManager, this);

            // Subscribe to damage & death actions
            Health.OnDie += OnDie;
            Health.OnDamaged += OnDamaged;

            // Find and initialize all weapons
            FindAndInitializeAllWeapons();
            var weapon = GetCurrentWeapon();
            weapon.ShowWeapon(true);

            var detectionModules = GetComponentsInChildren<DetectionModule>();
            DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, EnemyController>(detectionModules.Length, this,
                gameObject);
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
                this, gameObject);
            // Initialize detection module
            DetectionModule = detectionModules[0];
            DetectionModule.onDetectedTarget += OnDetectedTarget;
            DetectionModule.onLostTarget += OnLostTarget;
            onAttack += DetectionModule.OnAttack;

            var navigationModules = GetComponentsInChildren<NavigationModule>();
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
                this, gameObject);
            // Override navmesh agent data
            if (navigationModules.Length > 0)
            {
                NavigationModule = navigationModules[0];
                NavMeshAgent.speed = NavigationModule.MoveSpeed;
                NavMeshAgent.angularSpeed = NavigationModule.AngularSpeed;
                NavMeshAgent.acceleration = NavigationModule.Acceleration;
            }

            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == EyeColorMaterial)
                    {
                        EyeRendererData = new RendererIndexData(renderer, i);
                    }

                    if (renderer.sharedMaterials[i] == BodyMaterial)
                    {
                        BodyRenderers.Add(new RendererIndexData(renderer, i));
                    }
                }
            }

            BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

            // Check if we have an eye renderer for this enemy
            if (EyeRendererData.Renderer != null)
            {
                EyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
                EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
                EyeRendererData.Renderer.SetPropertyBlock(EyeColorMaterialPropertyBlock,
                    EyeRendererData.MaterialIndex);
            }
        }

        void Update()
        {
            EnsureIsWithinLevelBounds();

            DetectionModule.HandleTargetDetection(Actor, SelfColliders);

            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - LastTimeDamaged) / FlashOnHitDuration);
            BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in BodyRenderers)
            {
                data.Renderer.SetPropertyBlock(BodyFlashMaterialPropertyBlock, data.MaterialIndex);
            }

            WasDamagedThisFrame = false;
        }

        void EnsureIsWithinLevelBounds()
        {
            // at every frame, this tests for conditions to kill the enemy
            if (transform.position.y < SelfDestructYHeight)
            {
                Destroy(gameObject);
            }
        }

        void OnLostTarget()
        {
            onLostTarget.Invoke();

            // Set the eye attack color and property block if the eye renderer is set
            if (EyeRendererData.Renderer != null)
            {
                EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
                EyeRendererData.Renderer.SetPropertyBlock(EyeColorMaterialPropertyBlock,
                    EyeRendererData.MaterialIndex);
            }
        }

        void OnDetectedTarget()
        {
            onDetectedTarget.Invoke();

            // Set the eye default color and property block if the eye renderer is set
            if (EyeRendererData.Renderer != null)
            {
                EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", AttackEyeColor);
                EyeRendererData.Renderer.SetPropertyBlock(EyeColorMaterialPropertyBlock,
                    EyeRendererData.MaterialIndex);
            }
        }

        public void OrientTowards(Vector3 lookPosition)
        {
            Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
            if (lookDirection.sqrMagnitude != 0f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * OrientationSpeed);
            }
        }

        bool IsPathValid()
        {
            return PatrolPath && PatrolPath.PathNodes.Count > 0;
        }

        public void ResetPathDestination()
        {
            PathDestinationNodeIndex = 0;
        }

        public void SetPathDestinationToClosestNode()
        {
            if (IsPathValid())
            {
                int closestPathNodeIndex = 0;
                for (int i = 0; i < PatrolPath.PathNodes.Count; i++)
                {
                    float distanceToPathNode = PatrolPath.GetDistanceToNode(transform.position, i);
                    if (distanceToPathNode < PatrolPath.GetDistanceToNode(transform.position, closestPathNodeIndex))
                    {
                        closestPathNodeIndex = i;
                    }
                }

                PathDestinationNodeIndex = closestPathNodeIndex;
            }
            else
            {
                PathDestinationNodeIndex = 0;
            }
        }

        public Vector3 GetDestinationOnPath()
        {
            if (IsPathValid())
            {
                return PatrolPath.GetPositionOfPathNode(PathDestinationNodeIndex);
            }
            else
            {
                return transform.position;
            }
        }

        public void SetNavDestination(Vector3 destination)
        {
            if (NavMeshAgent)
            {
                NavMeshAgent.SetDestination(destination);
            }
        }

        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathValid())
            {
                // Check if reached the path destination
                if ((transform.position - GetDestinationOnPath()).magnitude <= PathReachingRadius)
                {
                    // increment path destination index
                    PathDestinationNodeIndex =
                        inverseOrder ? (PathDestinationNodeIndex - 1) : (PathDestinationNodeIndex + 1);
                    if (PathDestinationNodeIndex < 0)
                    {
                        PathDestinationNodeIndex += PatrolPath.PathNodes.Count;
                    }

                    if (PathDestinationNodeIndex >= PatrolPath.PathNodes.Count)
                    {
                        PathDestinationNodeIndex -= PatrolPath.PathNodes.Count;
                    }
                }
            }
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            // test if the damage source is the player
            if (damageSource && !damageSource.GetComponent<EnemyController>())
            {
                // pursue the player
                DetectionModule.OnDamaged(damageSource);
                
                onDamaged?.Invoke();
                LastTimeDamaged = Time.time;
            
                // play the damage tick sound
                if (DamageTick && !WasDamagedThisFrame)
                    AudioUtility.CreateSFX(DamageTick, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);
            
                WasDamagedThisFrame = true;
            }
        }

        void OnDie()
        {
            // spawn a particle system when dying
            var vfx = Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity);
            Destroy(vfx, 5f);
            onDie.Invoke();
            // tells the game flow manager to handle the enemy destuction
            EnemyManager.UnregisterEnemy(this);

            // loot an object
            if (TryDropItem())
            {
                Instantiate(LootPrefab, transform.position, Quaternion.identity);
            }

            // this will call the OnDestroy function
            Destroy(gameObject, DeathDuration);
        }

        void OnDrawGizmosSelected()
        {
            // Path reaching range
            Gizmos.color = PathReachingRangeColor;
            Gizmos.DrawWireSphere(transform.position, PathReachingRadius);

            if (DetectionModule != null)
            {
                // Detection range
                Gizmos.color = DetectionRangeColor;
                Gizmos.DrawWireSphere(transform.position, DetectionModule.DetectionRange);

                // Attack range
                Gizmos.color = AttackRangeColor;
                Gizmos.DrawWireSphere(transform.position, DetectionModule.AttackRange);
            }
        }

        public void OrientWeaponsTowards(Vector3 lookPosition)
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                // orient weapon towards player
                Vector3 weaponForward = (lookPosition - Weapons[i].WeaponRoot.transform.position).normalized;
                Weapons[i].transform.forward = weaponForward;
            }
        }

        public bool TryAtack(Vector3 enemyPosition)
        {
            if (GameFlowManager.GameIsEnding)
                return false;

            OrientWeaponsTowards(enemyPosition);

            if ((LastTimeWeaponSwapped + DelayAfterWeaponSwap) >= Time.time)
                return false;

            // Shoot the weapon
            bool didFire = GetCurrentWeapon().HandleUsageInputs(false, true, false);

            if (didFire && onAttack != null)
            {
                onAttack.Invoke();

                if (SwapToNextWeapon && Weapons.Length > 1)
                {
                    int nextWeaponIndex = (CurrentWeaponIndex + 1) % Weapons.Length;
                    SetCurrentWeapon(nextWeaponIndex);
                }
            }

            return didFire;
        }

        public bool TryDropItem()
        {
            if (DropRate == 0 || LootPrefab == null)
                return false;
            else if (DropRate == 1)
                return true;
            else
                return (Random.value <= DropRate);
        }

        void FindAndInitializeAllWeapons()
        {
            // Check if we already found and initialized the weapons
            if (Weapons == null)
            {
                Weapons = GetComponentsInChildren<WeaponController>();
                DebugUtility.HandleErrorIfNoComponentFound<WeaponController, EnemyController>(Weapons.Length, this,
                    gameObject);

                for (int i = 0; i < Weapons.Length; i++)
                {
                    Weapons[i].Owner = gameObject;
                }
            }
        }

        public WeaponController GetCurrentWeapon()
        {
            FindAndInitializeAllWeapons();
            // Check if no weapon is currently selected
            if (CurrentWeapon == null)
            {
                // Set the first weapon of the weapons list as the current weapon
                SetCurrentWeapon(0);
            }

            DebugUtility.HandleErrorIfNullGetComponent<WeaponController, EnemyController>(CurrentWeapon, this,
                gameObject);

            return CurrentWeapon;
        }

        void SetCurrentWeapon(int index)
        {
            CurrentWeaponIndex = index;
            CurrentWeapon = Weapons[CurrentWeaponIndex];
            if (SwapToNextWeapon)
            {
                LastTimeWeaponSwapped = Time.time;
            }
            else
            {
                LastTimeWeaponSwapped = Mathf.NegativeInfinity;
            }
        }
    }
}