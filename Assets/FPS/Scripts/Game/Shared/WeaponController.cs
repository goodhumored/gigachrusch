using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace FPS.Scripts.Game.Shared
{
    public enum WeaponUsageType
    {
        Manual,
        Automatic,
        Charge,
    }

    [System.Serializable]
    public struct CrosshairData
    {
        [Tooltip("The image that will be used for this weapon's crosshair")]
        public Sprite CrosshairSprite;

        [Tooltip("The size of the crosshair image")]
        public int CrosshairSize;

        [Tooltip("The color of the crosshair image")]
        public Color CrosshairColor;
    }

    [RequireComponent(typeof(AudioSource))]
    public abstract class WeaponController : MonoBehaviour
    {
        [Header("Information")] [Tooltip("The name that will be displayed in the UI for this weapon")]
        public string WeaponName;

        [Tooltip("The image that will be displayed in the UI for this weapon")]
        public Sprite WeaponIcon;

        [Tooltip("Default data for the crosshair")]
        public CrosshairData CrosshairDataDefault;

        [Tooltip("Data for the crosshair when targeting an enemy")]
        public CrosshairData CrosshairDataTargetInSight;

        [Header("Internal References")]
        [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
        public GameObject WeaponRoot;

        [Header("Usage Parameters")] [Tooltip("The type of weapon wil affect how it shoots")]
        public WeaponUsageType UsageType;

        [FormerlySerializedAs("AttackDelay")] [FormerlySerializedAs("DelayBetweenShots")] [Tooltip("Minimum duration between two shots")]
        public float UseDelay = 0.5f;

        [Header("Charging parameters (charging weapons only)")]
        [Tooltip("Trigger a shot when maximum charge is reached")]
        public bool AutomaticReleaseOnCharged;

        [Tooltip("Duration to reach maximum charge")]
        public float MaxChargeDuration = 2f;

        [Header("Audio & Visual")] 
        [Tooltip("Optional weapon animator for OnUsage animations")]
        public Animator WeaponAnimator;

        [Tooltip("sound played when shooting")]
        public AudioClip UsageSfx;

        [Tooltip("Sound played when changing to this weapon")]
        public AudioClip ChangeWeaponSfx;

        [Tooltip("Continuous Usageing Sound")] public bool UseContinuousUsageSound = false;
        public AudioClip ContinuousUsageStartSfx;
        public AudioClip ContinuousUsageLoopSfx;
        public AudioClip ContinuousUsageEndSfx;
        protected AudioSource ContinuousUsageAudioSource = null;

        public UnityAction OnUse;
        public event Action OnUseProcessed;
        
        protected float LastTimeUsed = Mathf.NegativeInfinity;
        public float LastChargeTriggerTimestamp { get; protected set; }

        public GameObject Owner { get; set; }
        public GameObject SourcePrefab { get; set; }
        public bool IsCharging { get; protected set; }
        public bool IsWeaponActive { get; protected set; }
        public bool IsCooling { get; protected set; }
        public float CurrentCharge { get; protected set; }
        public Vector3 MuzzleWorldVelocity { get; protected set; }
        
        public bool WantsToUse;

        protected AudioSource UsageAudioSource;

        const string AnimAttackParameter = "Attack";
        const string AnimChargeParameter = "Charge";

        private void Awake()
        {
            UsageAudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(UsageAudioSource, this,
                gameObject);

            if (UseContinuousUsageSound)
            {
                ContinuousUsageAudioSource = gameObject.AddComponent<AudioSource>();
                ContinuousUsageAudioSource.playOnAwake = false;
                ContinuousUsageAudioSource.clip = ContinuousUsageLoopSfx;
                ContinuousUsageAudioSource.outputAudioMixerGroup =
                    AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
                ContinuousUsageAudioSource.loop = true;
            }
        }

        void PlaySFX(AudioClip sfx) => AudioUtility.CreateSFX(sfx, transform.position, AudioUtility.AudioGroups.WeaponShoot, 0.0f);

        void Update()
        {
            UpdateCharge();
            UpdateContinuousUsageSound();
        }

        protected virtual void UpdateCharge()
        {
            if (IsCharging)
            {
                if (CurrentCharge < 1f)
                {
                    WeaponAnimator.SetFloat(AnimChargeParameter, CurrentCharge);
                    float chargeLeft = 1f - CurrentCharge;

                    // Calculate how much charge ratio to add this frame
                    float chargeAdded = 0f;
                    if (MaxChargeDuration <= 0f)
                    {
                        chargeAdded = chargeLeft;
                    }
                    else
                    {
                        chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
                    }
                    chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);
                    CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                }
            }
        }

        void UpdateContinuousUsageSound()
        {
            if (UseContinuousUsageSound)
            {
                if (!ContinuousUsageAudioSource.isPlaying)
                {
                    UsageAudioSource.PlayOneShot(UsageSfx);
                    UsageAudioSource.PlayOneShot(ContinuousUsageStartSfx);
                    ContinuousUsageAudioSource.Play();
                }
                else if (ContinuousUsageAudioSource.isPlaying)
                {
                    UsageAudioSource.PlayOneShot(ContinuousUsageEndSfx);
                    ContinuousUsageAudioSource.Stop();
                }
            }
        }

        public void ShowWeapon(bool show)
        {
            WeaponRoot.SetActive(show);

            if (show && ChangeWeaponSfx)
            {
                UsageAudioSource.PlayOneShot(ChangeWeaponSfx);
            }

            IsWeaponActive = show;
        }

        public bool HandleUsageInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            WantsToUse = inputDown || inputHeld;
            switch (UsageType)
            {
                case WeaponUsageType.Manual:
                    if (inputDown)
                    {
                        return TryToUse();
                    }

                    return false;

                case WeaponUsageType.Automatic:
                    if (inputHeld)
                    {
                        return TryToUse();
                    }

                    return false;

                case WeaponUsageType.Charge:
                    if (inputHeld)
                    {
                        TryBeginCharge();
                    }

                    // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                    if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                    {
                        return TryReleaseCharge();
                    }

                    return false;

                default:
                    return false;
            }
        }

        protected virtual bool TryToUse()
        {
            if (LastTimeUsed + UseDelay < Time.time)
            {
                HandleUsage();
                return true;
            }

            return false;
        }

        protected virtual bool TryBeginCharge()
        {
            if (!IsCharging
                && LastTimeUsed + UseDelay < Time.time)
            {
                LastChargeTriggerTimestamp = Time.time;
                IsCharging = true;
                return true;
            }
            return false;
        }

        protected virtual bool TryReleaseCharge()
        {
            if (IsCharging)
            {
                HandleUsage();

                CurrentCharge = 0f;
                WeaponAnimator.SetFloat(AnimChargeParameter, 0f);
                IsCharging = false;

                return true;
            }

            return false;
        }

        protected virtual void HandleUsage()
        {
            LastTimeUsed = Time.time;

            if (UsageSfx && !UseContinuousUsageSound)
            {
                UsageAudioSource.PlayOneShot(UsageSfx);
            }
            
            if (WeaponAnimator && CurrentCharge < 0.1)
            {
                WeaponAnimator.SetTrigger(AnimAttackParameter);
            }

            OnUse?.Invoke();
            OnUseProcessed?.Invoke();
        }
    }
}