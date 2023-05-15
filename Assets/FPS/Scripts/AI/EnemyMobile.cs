using FPS.Scripts.Game;
using UnityEngine;

namespace FPS.Scripts.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyMobile : MonoBehaviour
    {
        public enum AIState
        {
            Patrol,
            Follow,
            Attack,
        }

        public Animator Animator;
        public Rigidbody[] Rigidbodies;

        [Tooltip("Fraction of the enemy's attack range at which it will stop moving towards target while attacking")]
        [Range(0f, 1f)]
        public float AttackStopDistanceRatio = 0.5f;

        [Tooltip("The random hit damage effects")]
        public ParticleSystem[] RandomHitSparks;

        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        [Header("Sound")] public AudioClip MovementSound;
        public MinMaxFloat PitchDistortionMovementSpeed;

        public AIState AiState { get; private set; }
        EnemyController EnemyController;
        AudioSource AudioSource;

        const string AnimMoveSpeedParameter = "MoveSpeed";
        const string AnimAttackParameter = "Attack";
        const string AnimAlertedParameter = "Alerted";
        const string AnimOnDamagedParameter = "OnDamaged";

        void Start()
        {
            EnemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyMobile>(EnemyController, this,
                gameObject);

            EnemyController.onAttack += OnAttack;
            EnemyController.onDetectedTarget += OnDetectedTarget;
            EnemyController.onLostTarget += OnLostTarget;
            EnemyController.SetPathDestinationToClosestNode();
            EnemyController.onDamaged += OnDamaged;
            EnemyController.onDie += OnDie;

            // Start patrolling
            AiState = AIState.Patrol;

            // adding a audio source to play the movement sound on it
            AudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, EnemyMobile>(AudioSource, this, gameObject);
            AudioSource.clip = MovementSound;
            AudioSource.Play();
        }

        void Update()
        {
            UpdateAiStateTransitions();
            UpdateCurrentAiState();

            float moveSpeed = EnemyController.NavMeshAgent.velocity.magnitude;

            // Update animator speed parameter
            Animator.SetFloat(AnimMoveSpeedParameter, moveSpeed);

            // changing the pitch of the movement sound depending on the movement speed
            AudioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min, PitchDistortionMovementSpeed.Max,
                moveSpeed / EnemyController.NavMeshAgent.speed);
        }

        void UpdateAiStateTransitions()
        {
            // Handle transitions 
            switch (AiState)
            {
                case AIState.Follow:
                    // Transition to attack when there is a line of sight to the target
                    if (EnemyController.IsSeeingTarget && EnemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Attack;
                        EnemyController.SetNavDestination(transform.position);
                    }

                    break;
                case AIState.Attack:
                    // Transition to follow when no longer a target in attack range
                    if (!EnemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Follow;
                    }

                    break;
            }
        }

        void UpdateCurrentAiState()
        {
            // Handle logic 
            switch (AiState)
            {
                case AIState.Patrol:
                    EnemyController.UpdatePathDestination();
                    EnemyController.SetNavDestination(EnemyController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    EnemyController.SetNavDestination(EnemyController.KnownDetectedTarget.transform.position);
                    EnemyController.OrientTowards(EnemyController.KnownDetectedTarget.transform.position);
                    EnemyController.OrientWeaponsTowards(EnemyController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Attack:
                    if (Vector3.Distance(EnemyController.KnownDetectedTarget.transform.position,
                            EnemyController.DetectionModule.DetectionSourcePoint.position)
                        >= (AttackStopDistanceRatio * EnemyController.DetectionModule.AttackRange))
                    {
                        EnemyController.SetNavDestination(EnemyController.KnownDetectedTarget.transform.position);
                    }
                    else
                    {
                        EnemyController.SetNavDestination(transform.position);
                    }

                    EnemyController.OrientTowards(EnemyController.KnownDetectedTarget.transform.position);
                    EnemyController.TryAtack(EnemyController.KnownDetectedTarget.transform.position);
                    break;
            }
        }

        void OnAttack()
        {
            Animator.SetTrigger(AnimAttackParameter);
        }

        void OnDetectedTarget()
        {
            if (AiState == AIState.Patrol)
            {
                AiState = AIState.Follow;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Play();
            }

            if (OnDetectSfx)
            {
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
            }

            Animator.SetBool(AnimAlertedParameter, true);
        }

        void OnLostTarget()
        {
            if (AiState == AIState.Follow || AiState == AIState.Attack)
            {
                AiState = AIState.Patrol;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Stop();
            }

            Animator.SetBool(AnimAlertedParameter, false);
        }

        void OnDamaged()
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }

            Animator.SetTrigger(AnimOnDamagedParameter);
        }

        void OnDie()
        {
            foreach (var rb in Rigidbodies)
            {
                rb.isKinematic = false;
            }
            GetComponent<EnemyController>().enabled = false;
            Animator.GetComponent<Animator>().enabled = false;
            GetComponent<EnemyMobile>().enabled = false;
        }
    }
}