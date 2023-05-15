using System.Collections.Generic;
using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using UnityEngine;

namespace FPS.Scripts.Gameplay
{
    public class OverheatBehavior : MonoBehaviour
    {
        [System.Serializable]
        public struct RendererIndexData
        {
            public Renderer Renderer;
            public int MaterialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                this.Renderer = renderer;
                this.MaterialIndex = index;
            }
        }

        [Header("Visual")] [Tooltip("The VFX to scale the spawn rate based on the ammo ratio")]
        public ParticleSystem SteamVfx;

        [Tooltip("The emission rate for the effect when fully overheated")]
        public float SteamVfxEmissionRateMax = 8f;

        //Set gradient field to HDR
        [GradientUsage(true)] [Tooltip("Overheat color based on ammo ratio")]
        public Gradient OverheatGradient;

        [Tooltip("The material for overheating color animation")]
        public Material OverheatingMaterial;

        [Header("Sound")] [Tooltip("Sound played when a cell are cooling")]
        public AudioClip CoolingCellsSound;

        [Tooltip("Curve for ammo to volume ratio")]
        public AnimationCurve AmmoToVolumeRatioCurve;


        GunController Weapon;
        AudioSource AudioSource;
        List<RendererIndexData> OverheatingRenderersData;
        MaterialPropertyBlock OverheatMaterialPropertyBlock;
        float LastAmmoRatio;
        ParticleSystem.EmissionModule SteamVfxEmissionModule;

        void Awake()
        {
            var emissionModule = SteamVfx.emission;
            emissionModule.rateOverTimeMultiplier = 0f;

            OverheatingRenderersData = new List<RendererIndexData>();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == OverheatingMaterial)
                        OverheatingRenderersData.Add(new RendererIndexData(renderer, i));
                }
            }

            OverheatMaterialPropertyBlock = new MaterialPropertyBlock();
            SteamVfxEmissionModule = SteamVfx.emission;

            Weapon = GetComponent<GunController>();
            DebugUtility.HandleErrorIfNullGetComponent<WeaponController, OverheatBehavior>(Weapon, this, gameObject);

            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.clip = CoolingCellsSound;
            AudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponOverheat);
        }

        void Update()
        {
            // visual smoke shooting out of the gun
            float currentAmmoRatio = Weapon.CurrentAmmoRatio;
            if (currentAmmoRatio != LastAmmoRatio)
            {
                OverheatMaterialPropertyBlock.SetColor("_EmissionColor",
                    OverheatGradient.Evaluate(1f - currentAmmoRatio));

                foreach (var data in OverheatingRenderersData)
                {
                    data.Renderer.SetPropertyBlock(OverheatMaterialPropertyBlock, data.MaterialIndex);
                }

                SteamVfxEmissionModule.rateOverTimeMultiplier = SteamVfxEmissionRateMax * (1f - currentAmmoRatio);
            }

            // cooling sound
            if (CoolingCellsSound)
            {
                if (!AudioSource.isPlaying
                    && currentAmmoRatio != 1
                    && Weapon.IsWeaponActive
                    && Weapon.IsCooling)
                {
                    AudioSource.Play();
                }
                else if (AudioSource.isPlaying
                         && (currentAmmoRatio == 1 || !Weapon.IsWeaponActive || !Weapon.IsCooling))
                {
                    AudioSource.Stop();
                    return;
                }

                AudioSource.volume = AmmoToVolumeRatioCurve.Evaluate(1 - currentAmmoRatio);
            }

            LastAmmoRatio = currentAmmoRatio;
        }
    }
}