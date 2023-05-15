using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FPS.Scripts.Game.Shared
{

    [RequireComponent(typeof(AudioSource))]
    public class GunController : WeaponController
    {
        [Tooltip("Tip of the weapon, where the projectiles are shot")]
        public Transform WeaponMuzzle;

        [Tooltip("The projectile prefab")] public ProjectileBase ProjectilePrefab;

        [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
        public float BulletSpreadAngle = 0f;

        [Tooltip("Amount of bullets per shot")]
        public int BulletsPerShot = 1;

        [Tooltip("Force that will push back the weapon after each shot")] [Range(0f, 5f)]
        public float RecoilForce = 1;

        [Tooltip("Ratio of the default FOV that this weapon applies while aiming")] [Range(0f, 1f)]
        public float AimZoomRatio = 1f;

        [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
        public Vector3 AimOffset;

        [Header("Ammo Parameters")]
        [Tooltip("Should the player manually reload")]
        public bool AutomaticReload = true;
        [Tooltip("Has physical clip on the weapon and ammo shells are ejected when firing")]
        public bool HasPhysicalBullets = false;
        [Tooltip("Number of bullets in a clip")]
        public int ClipSize = 30;
        [Tooltip("Bullet Shell Casing")]
        public GameObject ShellCasing;
        [Tooltip("Weapon Ejection Port for physical ammo")]
        public Transform EjectionPort;
        [Tooltip("Force applied on the shell")]
        [Range(0.0f, 5.0f)] public float ShellCasingEjectionForce = 2.0f;
        [Tooltip("Maximum number of shell that can be spawned before reuse")]
        [Range(1, 30)] public int ShellPoolSize = 1;
        [Tooltip("Amount of ammo reloaded per second")]
        public float AmmoReloadRate = 1f;

        [Tooltip("Delay after the last shot before starting to reload")]
        public float AmmoReloadDelay = 2f;

        [Tooltip("Maximum amount of ammo in the gun")]
        public int MaxAmmo = 8;

        [Tooltip("Initial ammo used when starting to charge")]
        public float AmmoUsedOnStartCharge = 1f;

        [Tooltip("Additional ammo used when charge reaches its maximum")]
        public float AmmoUsageRateWhileCharging = 1f;

        [Tooltip("Prefab of the muzzle flash")]
        public GameObject MuzzleFlashPrefab;

        [Tooltip("Unparent the muzzle flash instance on spawn")]
        public bool UnparentMuzzleFlash;

        public UnityAction OnUsage;
        public event Action OnUsageProcessed;

        int CarriedPhysicalBullets;
        float CurrentAmmo;
        
        Vector3 LastMuzzlePosition;
        
        public float CurrentAmmoRatio { get; private set; }

        public float GetAmmoNeededToShoot() =>
            (UsageType != WeaponUsageType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
            (MaxAmmo * BulletsPerShot);

        public int GetCarriedPhysicalBullets() => CarriedPhysicalBullets;
        public int GetCurrentAmmo() => Mathf.FloorToInt(CurrentAmmo);

        public bool IsReloading { get; private set; }

        const string AnimAttackParameter = "Attack";
        const string AnimChargeParameter = "Charge";

        private Queue<Rigidbody> PhysicalAmmoPool;

        private void Awake()
        {
            CurrentAmmo = MaxAmmo;
            CarriedPhysicalBullets = HasPhysicalBullets ? ClipSize : 0;
            LastMuzzlePosition = WeaponMuzzle.position;

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

            if (HasPhysicalBullets)
            {
                PhysicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);

                for (int i = 0; i < ShellPoolSize; i++)
                {
                    GameObject shell = Instantiate(ShellCasing, transform);
                    shell.SetActive(false);
                    PhysicalAmmoPool.Enqueue(shell.GetComponent<Rigidbody>());
                }
            }
        }

        public void AddCarriablePhysicalBullets(int count) => CarriedPhysicalBullets = Mathf.Max(CarriedPhysicalBullets + count, MaxAmmo);

        void ShotShell()
        {
            Rigidbody nextShell = PhysicalAmmoPool.Dequeue();

            nextShell.transform.position = EjectionPort.transform.position;
            nextShell.transform.rotation = EjectionPort.transform.rotation;
            nextShell.gameObject.SetActive(true);
            nextShell.transform.SetParent(null);
            nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
            nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

            PhysicalAmmoPool.Enqueue(nextShell);
        }

        void PlaySFX(AudioClip sfx) => AudioUtility.CreateSFX(sfx, transform.position, AudioUtility.AudioGroups.WeaponShoot, 0.0f);


        void Reload()
        {
            if (CarriedPhysicalBullets > 0)
            {
                CurrentAmmo = Mathf.Min(CarriedPhysicalBullets, ClipSize);
            }

            IsReloading = false;
        }

        public void StartReloadAnimation()
        {
            if (CurrentAmmo < CarriedPhysicalBullets)
            {
                GetComponent<Animator>().SetTrigger("Reload");
                IsReloading = true;
            }
        }

        void Update()
        {
            UpdateAmmo();
            UpdateCharge();
            UpdateContinuousUsageSound();
            if (IsReloading) Reload();

            if (Time.deltaTime > 0)
            {
                MuzzleWorldVelocity = (WeaponMuzzle.position - LastMuzzlePosition) / Time.deltaTime;
                LastMuzzlePosition = WeaponMuzzle.position;
            }
        }

        void UpdateAmmo()
        {
            if (AutomaticReload && LastTimeUsed + AmmoReloadDelay < Time.time && CurrentAmmo < MaxAmmo && !IsCharging)
            {
                // reloads weapon over time
                CurrentAmmo += AmmoReloadRate * Time.deltaTime;

                // limits ammo to max value
                CurrentAmmo = Mathf.Clamp(CurrentAmmo, 0, MaxAmmo);

                IsCooling = true;
            }
            else
            {
                IsCooling = false;
            }

            if (float.IsPositiveInfinity(MaxAmmo))
            {
                CurrentAmmoRatio = 1f;
            }
            else
            {
                CurrentAmmoRatio = CurrentAmmo / MaxAmmo;
            }
        }

        protected override void UpdateCharge()
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

                    // See if we can actually add this charge
                    float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
                    if (ammoThisChargeWouldRequire <= CurrentAmmo)
                    {
                        // Use ammo based on charge added
                        UseAmmo(ammoThisChargeWouldRequire);

                        // set current charge ratio
                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                    }
                }
            }
        }

        void UpdateContinuousUsageSound()
        {
            if (UseContinuousUsageSound)
            {
                if (WantsToUse && CurrentAmmo >= 1f)
                {
                    if (!ContinuousUsageAudioSource.isPlaying)
                    {
                        UsageAudioSource.PlayOneShot(UsageSfx);
                        UsageAudioSource.PlayOneShot(ContinuousUsageStartSfx);
                        ContinuousUsageAudioSource.Play();
                    }
                }
                else if (ContinuousUsageAudioSource.isPlaying)
                {
                    UsageAudioSource.PlayOneShot(ContinuousUsageEndSfx);
                    ContinuousUsageAudioSource.Stop();
                }
            }
        }

        public void UseAmmo(float amount)
        {
            CurrentAmmo = Mathf.Clamp(CurrentAmmo - amount, 0f, MaxAmmo);
            CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
            CarriedPhysicalBullets = Mathf.Clamp(CarriedPhysicalBullets, 0, MaxAmmo);
            LastTimeUsed = Time.time;
        }

        protected override bool TryToUse()
        {
            if (CurrentAmmo >= 1f
                && LastTimeUsed + UseDelay < Time.time)
            {
                HandleUsage();
                CurrentAmmo -= 1f;

                return true;
            }

            return false;
        }

        protected override bool TryBeginCharge()
        {
            if (!IsCharging
                && CurrentAmmo >= AmmoUsedOnStartCharge
                && Mathf.FloorToInt((CurrentAmmo - AmmoUsedOnStartCharge) * BulletsPerShot) > 0
                && LastTimeUsed + UseDelay < Time.time)
            {
                UseAmmo(AmmoUsedOnStartCharge);

                LastChargeTriggerTimestamp = Time.time;
                IsCharging = true;

                return true;
            }

            return false;
        }

        protected override void HandleUsage()
        {
            int bulletsPerShotFinal = UsageType == WeaponUsageType.Charge
                ? Mathf.CeilToInt(CurrentCharge * BulletsPerShot)
                : BulletsPerShot;
            
            for (int i = 0; i < bulletsPerShotFinal; i++)
            {
                Vector3 shotDirection = GetShotDirectionWithinSpread(WeaponMuzzle);
                ProjectileBase newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position,
                    Quaternion.LookRotation(shotDirection));
                newProjectile.Shoot(this);
            }
            
            if (MuzzleFlashPrefab != null)
            {
                GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position,
                    WeaponMuzzle.rotation, WeaponMuzzle.transform);
                
                if (UnparentMuzzleFlash)
                {
                    muzzleFlashInstance.transform.SetParent(null);
                }

                Destroy(muzzleFlashInstance, 2f);
            }

            if (HasPhysicalBullets) ShotShell();
            UseAmmo(bulletsPerShotFinal);
            
            base.HandleUsage();
        }

        public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
        {
            float spreadAngleRatio = BulletSpreadAngle / 180f;
            Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
                spreadAngleRatio);

            return spreadWorldDirection;
        }
    }
}