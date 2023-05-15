using FPS.Scripts.Game;
using FPS.Scripts.Game.Managers;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace FPS.Scripts.Gameplay
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler), typeof(AudioSource))]
    public class PlayerCharacterController : MonoBehaviour
    {
        [Header("References")]
        public Camera PlayerCamera;
        public AudioSource AudioSource;

        [Header("General")]
        public float GravityDownForce = 20f;
        public LayerMask GroundCheckLayers = -1;
        public float GroundCheckDistance = 0.05f;

        [Header("Movement")]
        public float MaxSpeedOnGround = 10f;
        public float MovementSharpnessOnGround = 15;
        [Range(0, 1)]
        public float MaxSpeedCrouchedRatio = 0.5f;
        public float MaxSpeedInAir = 10f;
        public float AccelerationSpeedInAir = 25f;
        public float SprintSpeedModifier = 2f;
        public float KillHeight = -50f;
        public float staminaCostPerSprintSecond = 1f;
        public float staminaCostPerJump = 1f;

        [Header("Rotation")]
        public float RotationSpeed = 200f;

        [Range(0.1f, 1f)]
        public float AimingRotationMultiplier = 0.4f;

        [Header("Jump")]
        public float JumpForce = 9f;

        [Header("Stance")]
        public float CameraHeightRatio = 0.9f;
        public float CapsuleHeightStanding = 1.8f;
        public float CapsuleHeightCrouching = 0.9f;
        public float CrouchingSharpness = 10f;

        [Header("Audio")]
        public float FootstepSfxFrequency = 1f;
        public float FootstepSfxFrequencyWhileSprinting = 1f;
        public AudioClip FootstepSfx;
        public AudioClip JumpSfx;
        public AudioClip LandSfx;
        public AudioClip FallDamageSfx;

        [Header("Fall Damage")]
        public bool RecievesFallDamage;
        public float MinSpeedForFallDamage = 10f;
        public float MaxSpeedForFallDamage = 30f;
        public float FallDamageAtMinSpeed = 10f;
        public float FallDamageAtMaxSpeed = 50f;

        public UnityAction<bool> OnStanceChanged;

        public Vector3 CharacterVelocity { get; set; }
        public bool IsGrounded { get; private set; }
        public bool HasJumpedThisFrame { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsCrouching { get; private set; }

        public float RotationMultiplier
        {
            get
            {
                if (_weaponsManager.IsAiming)
                {
                    return AimingRotationMultiplier;
                }

                return 1f;
            }
        }

        private Health _health;
        private Stamina _stamina;
        private PlayerInputHandler _inputHandler;
        private CharacterController _controller;
        private PlayerWeaponsManager _weaponsManager;
        private Actor _actor;
        private Vector3 _groundNormal;
        private Vector3 _characterVelocity;
        private Vector3 _latestImpactSpeed;
        private float _lastTimeJumped = 0f;
        private float _cameraVerticalAngle = 0f;
        private float _footstepDistanceCounter;
        private float _targetCharacterHeight;

        private const float JumpGroundingPreventionTime = 0.2f;
        private const float GroundCheckDistanceInAir = 0.07f;

        void Awake()
        {
            ActorsManager actorsManager = FindObjectOfType<ActorsManager>();
            if (actorsManager != null)
                actorsManager.SetPlayer(gameObject);
        }

        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _inputHandler = GetComponent<PlayerInputHandler>();
            _weaponsManager = GetComponent<PlayerWeaponsManager>();
            _health = GetComponent<Health>();
            _stamina = GetComponent<Stamina>();
            _actor = GetComponent<Actor>();
            _controller.enableOverlapRecovery = true;
            _health.OnDie += OnDie;
            
            SetCrouchingState(false, true);
            UpdateCharacterHeight(true);
        }

        void Update()
        {
            HandleOutworldDeath();
            HasJumpedThisFrame = false;
            HandleFall();
            UpdateCharacterHeight(false);
            HandleCharacterControl();
        }

        private void HandleFall()
        {
            
            bool wasGrounded = IsGrounded;
            GroundCheck();
            
            if (IsGrounded && !wasGrounded)
            {
                ApplyFallDamage();
            }
        }

        private void ApplyFallDamage()
        {
            float fallSpeed = -Mathf.Min(CharacterVelocity.y, _latestImpactSpeed.y);
            float fallSpeedRatio = (fallSpeed - MinSpeedForFallDamage) /
                                   (MaxSpeedForFallDamage - MinSpeedForFallDamage);
            if (RecievesFallDamage && fallSpeedRatio > 0f)
            {
                float dmgFromFall = Mathf.Lerp(FallDamageAtMinSpeed, FallDamageAtMaxSpeed, fallSpeedRatio);
                _health.TakeDamage(dmgFromFall, null);
                    
                AudioSource.PlayOneShot(FallDamageSfx);
            }
            else
            {
                AudioSource.PlayOneShot(LandSfx);
            }
        }

        private void HandleOutworldDeath()
        {
            if (!IsDead && transform.position.y < KillHeight)
            {
                _health.Kill();
            }
        }

        void OnDie()
        {
            IsDead = true;
            _weaponsManager.SwitchToWeaponIndex(-1, true);
            EventManager.Broadcast(Events.PlayerDeathEvent);
        }

        void GroundCheck()
        {
            float chosenGroundCheckDistance =
                IsGrounded ? (_controller.skinWidth + GroundCheckDistance) : GroundCheckDistanceInAir;

            IsGrounded = false;
            _groundNormal = Vector3.up;

            if (Time.time >= _lastTimeJumped + JumpGroundingPreventionTime)
            {
                if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(_controller.height),
                        _controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance,
                        GroundCheckLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    _groundNormal = hit.normal;

                    if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                        IsNormalUnderSlopeLimit(_groundNormal))
                    {
                        IsGrounded = true;
                        if (hit.distance > _controller.skinWidth)
                        {
                            _controller.Move(Vector3.down * hit.distance);
                        }
                    }
                }
            }
        }

        void HandleCharacterControl()
        {
            HandleRotation();
            HandleMovement();
            MoveCharacter();
        }

        private void HandleMovement()
        {   
            var isSprinting = _inputHandler.GetSprintInputHeld() && _stamina.Reduce(staminaCostPerSprintSecond * Time.deltaTime);
            if (isSprinting)
            {
                isSprinting = SetCrouchingState(false, false);
            }
            else
            {
                SetCrouchingState(_inputHandler.GetCrouchInputHeld(), false);
            }

            var speedModifier = isSprinting ? SprintSpeedModifier : 1f;
            var worldspaceMoveInput = transform.TransformVector(_inputHandler.GetMoveInput());

            if (IsGrounded)
            {
                LerpCharacterVelocity(worldspaceMoveInput, speedModifier);
                HandleJump();
                PlayFootstep(isSprinting);
            }
            else
            {
                HandleAirMovement(worldspaceMoveInput, speedModifier);
            }
        }

        private void LerpCharacterVelocity(Vector3 worldspaceMoveInput, float speedModifier)
        {
            var targetVelocity = worldspaceMoveInput * (MaxSpeedOnGround * speedModifier);
            if (IsCrouching)
                targetVelocity *= MaxSpeedCrouchedRatio;
            targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, _groundNormal) *
                             targetVelocity.magnitude;

            CharacterVelocity = Vector3.Lerp(CharacterVelocity, targetVelocity,
                MovementSharpnessOnGround * Time.deltaTime);
        }

        private void PlayFootstep(bool isSprinting)
        {
            var chosenFootstepSfxFrequency =
                (isSprinting ? FootstepSfxFrequencyWhileSprinting : FootstepSfxFrequency);
            if (_footstepDistanceCounter >= 1f / chosenFootstepSfxFrequency)
            {
                _footstepDistanceCounter = 0f;
                AudioSource.PlayOneShot(FootstepSfx);
            }
            _footstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;
        }

        private void HandleAirMovement(Vector3 worldspaceMoveInput, float speedModifier)
        {
            CharacterVelocity += worldspaceMoveInput * (AccelerationSpeedInAir * Time.deltaTime);
            float verticalVelocity = CharacterVelocity.y;
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(CharacterVelocity, Vector3.up);
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir * speedModifier);
            CharacterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);
            CharacterVelocity += Vector3.down * (GravityDownForce * Time.deltaTime);
        }

        private void HandleRotation()
        {
            transform.Rotate(
                new Vector3(0f, (_inputHandler.GetLookInputsHorizontal() * RotationSpeed * RotationMultiplier),
                    0f), Space.Self);

            _cameraVerticalAngle += _inputHandler.GetLookInputsVertical() * RotationSpeed * RotationMultiplier;

            _cameraVerticalAngle = Mathf.Clamp(_cameraVerticalAngle, -89f, 89f);

            PlayerCamera.transform.localEulerAngles = new Vector3(_cameraVerticalAngle, 0, 0);
        }

        private void HandleJump()
        {
            if (IsGrounded && _inputHandler.GetJumpInputDown() && _stamina.Reduce(staminaCostPerJump))
            {
                if (SetCrouchingState(false, false))
                {
                    CharacterVelocity = new Vector3(CharacterVelocity.x, 0f, CharacterVelocity.z);
                    CharacterVelocity += Vector3.up * JumpForce;
                    AudioSource.PlayOneShot(JumpSfx);

                    _lastTimeJumped = Time.time;
                    HasJumpedThisFrame = true;

                    IsGrounded = false;
                    _groundNormal = Vector3.up;
                }
            }
        }

        private void MoveCharacter()
        {
            Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
            Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(_controller.height);
            _controller.Move(CharacterVelocity * Time.deltaTime);

            _latestImpactSpeed = Vector3.zero;
            if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, _controller.radius,
                    CharacterVelocity.normalized, out var hit, CharacterVelocity.magnitude * Time.deltaTime, -1,
                    QueryTriggerInteraction.Ignore))
            {
                _latestImpactSpeed = CharacterVelocity;
                CharacterVelocity = Vector3.ProjectOnPlane(CharacterVelocity, hit.normal);
            }
        }

        // Returns true if the slope angle represented by the given normal is under the slope angle limit of the character controller
        bool IsNormalUnderSlopeLimit(Vector3 normal)
        {
            return Vector3.Angle(transform.up, normal) <= _controller.slopeLimit;
        }

        // Gets the center point of the bottom hemisphere of the character controller capsule    
        Vector3 GetCapsuleBottomHemisphere()
        {
            return transform.position + (transform.up * _controller.radius);
        }

        // Gets the center point of the top hemisphere of the character controller capsule    
        Vector3 GetCapsuleTopHemisphere(float atHeight)
        {
            return transform.position + (transform.up * (atHeight - _controller.radius));
        }

        // Gets a reoriented direction that is tangent to a given slope
        public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, transform.up);
            return Vector3.Cross(slopeNormal, directionRight).normalized;
        }

        void UpdateCharacterHeight(bool force)
        {
            if (force)
            {
                _controller.height = _targetCharacterHeight;
                _controller.center = Vector3.up * _controller.height * 0.5f;
                PlayerCamera.transform.localPosition = Vector3.up * _targetCharacterHeight * CameraHeightRatio;
                _actor.AimPoint.transform.localPosition = _controller.center;
            }
            else if (_controller.height != _targetCharacterHeight)
            {
                // resize the capsule and adjust camera position
                _controller.height = Mathf.Lerp(_controller.height, _targetCharacterHeight,
                    CrouchingSharpness * Time.deltaTime);
                _controller.center = Vector3.up * _controller.height * 0.5f;
                PlayerCamera.transform.localPosition = Vector3.Lerp(PlayerCamera.transform.localPosition,
                    Vector3.up * _targetCharacterHeight * CameraHeightRatio, CrouchingSharpness * Time.deltaTime);
                _actor.AimPoint.transform.localPosition = _controller.center;
            }
        }

        // returns false if there was an obstruction
        bool SetCrouchingState(bool crouched, bool ignoreObstructions)
        {
            if (crouched)
            {
                _targetCharacterHeight = CapsuleHeightCrouching;
            }
            else
            {
                if (!ignoreObstructions)
                {
                    Collider[] standingOverlaps = Physics.OverlapCapsule(
                        GetCapsuleBottomHemisphere(),
                        GetCapsuleTopHemisphere(CapsuleHeightStanding),
                        _controller.radius,
                        -1,
                        QueryTriggerInteraction.Ignore);
                    foreach (Collider c in standingOverlaps)
                    {
                        if (c != _controller)
                        {
                            return false;
                        }
                    }
                }

                _targetCharacterHeight = CapsuleHeightStanding;
            }

            if (OnStanceChanged != null)
            {
                OnStanceChanged.Invoke(crouched);
            }

            IsCrouching = crouched;
            return true;
        }
    }
}