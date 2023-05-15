using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using UnityEngine;

namespace FPS.Scripts.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class ChargedWeaponEffectsHandler : MonoBehaviour
    {
        [Header("Visual")] [Tooltip("Object that will be affected by charging scale & color changes")]
        public GameObject ChargingObject;

        [Tooltip("The spinning frame")] public GameObject SpinningFrame;

        [Tooltip("Scale of the charged object based on charge")]
        public MinMaxVector3 Scale;

        [Header("Particles")] [Tooltip("Particles to create when charging")]
        public GameObject DiskOrbitParticlePrefab;

        [Tooltip("Local position offset of the charge particles (relative to this transform)")]
        public Vector3 Offset;

        [Tooltip("Parent transform for the particles (Optional)")]
        public Transform ParentTransform;

        [Tooltip("Orbital velocity of the charge particles based on charge")]
        public MinMaxFloat OrbitY;

        [Tooltip("Radius of the charge particles based on charge")]
        public MinMaxVector3 Radius;

        [Tooltip("Idle spinning speed of the frame based on charge")]
        public MinMaxFloat SpinningSpeed;

        [Header("Sound")] [Tooltip("Audio clip for charge SFX")]
        public AudioClip ChargeSound;

        [Tooltip("Sound played in loop after the change is full for this weapon")]
        public AudioClip LoopChargeWeaponSfx;

        [Tooltip("Duration of the cross fade between the charge and the loop sound")]
        public float FadeLoopDuration = 0.5f;

        [Tooltip(
            "If true, the ChargeSound will be ignored and the pitch on the LoopSound will be procedural, based on the charge amount")]
        public bool UseProceduralPitchOnLoopSfx;

        [Range(1.0f, 5.0f), Tooltip("Maximum procedural Pitch value")]
        public float MaxProceduralPitchValue = 2.0f;

        public GameObject ParticleInstance { get; set; }

        ParticleSystem DiskOrbitParticle;
        WeaponController WeaponController;
        ParticleSystem.VelocityOverLifetimeModule VelocityOverTimeModule;

        AudioSource AudioSource;
        AudioSource AudioSourceLoop;

        float LastChargeTriggerTimestamp;
        float ChargeRatio;
        float EndchargeTime;

        void Awake()
        {
            LastChargeTriggerTimestamp = 0.0f;

            // The charge effect needs it's own AudioSources, since it will play on top of the other gun sounds
            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.clip = ChargeSound;
            AudioSource.playOnAwake = false;
            AudioSource.outputAudioMixerGroup =
                AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponChargeBuildup);

            // create a second audio source, to play the sound with a delay
            AudioSourceLoop = gameObject.AddComponent<AudioSource>();
            AudioSourceLoop.clip = LoopChargeWeaponSfx;
            AudioSourceLoop.playOnAwake = false;
            AudioSourceLoop.loop = true;
            AudioSourceLoop.outputAudioMixerGroup =
                AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponChargeLoop);
        }

        void SpawnParticleSystem()
        {
            ParticleInstance = Instantiate(DiskOrbitParticlePrefab,
                ParentTransform != null ? ParentTransform : transform);
            ParticleInstance.transform.localPosition += Offset;

            FindReferences();
        }

        public void FindReferences()
        {
            DiskOrbitParticle = ParticleInstance.GetComponent<ParticleSystem>();
            DebugUtility.HandleErrorIfNullGetComponent<ParticleSystem, ChargedWeaponEffectsHandler>(DiskOrbitParticle,
                this, ParticleInstance.gameObject);

            WeaponController = GetComponent<WeaponController>();
            DebugUtility.HandleErrorIfNullGetComponent<WeaponController, ChargedWeaponEffectsHandler>(
                WeaponController, this, gameObject);

            VelocityOverTimeModule = DiskOrbitParticle.velocityOverLifetime;
        }

        void Update()
        {
            if (ParticleInstance == null)
                SpawnParticleSystem();

            DiskOrbitParticle.gameObject.SetActive(WeaponController.IsWeaponActive);
            ChargeRatio = WeaponController.CurrentCharge;

            ChargingObject.transform.localScale = Scale.GetValueFromRatio(ChargeRatio);
            if (SpinningFrame != null)
            {
                SpinningFrame.transform.localRotation *= Quaternion.Euler(0,
                    SpinningSpeed.GetValueFromRatio(ChargeRatio) * Time.deltaTime, 0);
            }

            VelocityOverTimeModule.orbitalY = OrbitY.GetValueFromRatio(ChargeRatio);
            DiskOrbitParticle.transform.localScale = Radius.GetValueFromRatio(ChargeRatio * 1.1f);

            // update sound's volume and pitch 
            if (ChargeRatio > 0)
            {
                if (!AudioSourceLoop.isPlaying &&
                    WeaponController.LastChargeTriggerTimestamp > LastChargeTriggerTimestamp)
                {
                    LastChargeTriggerTimestamp = WeaponController.LastChargeTriggerTimestamp;
                    if (!UseProceduralPitchOnLoopSfx)
                    {
                        EndchargeTime = Time.time + ChargeSound.length;
                        AudioSource.Play();
                    }

                    AudioSourceLoop.Play();
                }

                if (!UseProceduralPitchOnLoopSfx)
                {
                    float volumeRatio =
                        Mathf.Clamp01((EndchargeTime - Time.time - FadeLoopDuration) / FadeLoopDuration);
                    AudioSource.volume = volumeRatio;
                    AudioSourceLoop.volume = 1 - volumeRatio;
                }
                else
                {
                    AudioSourceLoop.pitch = Mathf.Lerp(1.0f, MaxProceduralPitchValue, ChargeRatio);
                }
            }
            else
            {
                AudioSource.Stop();
                AudioSourceLoop.Stop();
            }
        }
    }
}