using FPS.Scripts.Game;
using FPS.Scripts.Game.Managers;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    public class FeedbackFlashHUD : MonoBehaviour
    {
        [Header("References")] [Tooltip("Image component of the flash")]
        public Image FlashImage;

        [Tooltip("CanvasGroup to fade the damage flash, used when recieving damage end healing")]
        public CanvasGroup FlashCanvasGroup;

        [Tooltip("CanvasGroup to fade the critical health vignette")]
        public CanvasGroup VignetteCanvasGroup;

        [Header("Damage")] [Tooltip("Color of the damage flash")]
        public Color DamageFlashColor;

        [Tooltip("Duration of the damage flash")]
        public float DamageFlashDuration;

        [Tooltip("Max alpha of the damage flash")]
        public float DamageFlashMaxAlpha = 1f;

        [Header("Critical health")] [Tooltip("Max alpha of the critical vignette")]
        public float CriticaHealthVignetteMaxAlpha = .8f;

        [Tooltip("Frequency at which the vignette will pulse when at critical health")]
        public float PulsatingVignetteFrequency = 4f;

        [Header("Heal")] [Tooltip("Color of the heal flash")]
        public Color HealFlashColor;

        [Tooltip("Duration of the heal flash")]
        public float HealFlashDuration;

        [Tooltip("Max alpha of the heal flash")]
        public float HealFlashMaxAlpha = 1f;

        bool FlashActive;
        float LastTimeFlashStarted = Mathf.NegativeInfinity;
        Health PlayerHealth;
        GameFlowManager GameFlowManager;

        void Start()
        {
            // Subscribe to player damage events
            PlayerCharacterController playerCharacterController = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, FeedbackFlashHUD>(
                playerCharacterController, this);

            PlayerHealth = playerCharacterController.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, FeedbackFlashHUD>(PlayerHealth, this,
                playerCharacterController.gameObject);

            GameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, FeedbackFlashHUD>(GameFlowManager, this);

            PlayerHealth.OnDamaged += OnTakeDamage;
            PlayerHealth.OnHealed += OnHealed;
        }

        void Update()
        {
            if (PlayerHealth.IsCritical())
            {
                VignetteCanvasGroup.gameObject.SetActive(true);
                float vignetteAlpha =
                    (1 - (PlayerHealth.CurrentHealth / PlayerHealth.MaxHealth /
                          PlayerHealth.CriticalHealthRatio)) * CriticaHealthVignetteMaxAlpha;

                if (GameFlowManager.GameIsEnding)
                    VignetteCanvasGroup.alpha = vignetteAlpha;
                else
                    VignetteCanvasGroup.alpha =
                        ((Mathf.Sin(Time.time * PulsatingVignetteFrequency) / 2) + 0.5f) * vignetteAlpha;
            }
            else
            {
                VignetteCanvasGroup.gameObject.SetActive(false);
            }


            if (FlashActive)
            {
                float normalizedTimeSinceDamage = (Time.time - LastTimeFlashStarted) / DamageFlashDuration;

                if (normalizedTimeSinceDamage < 1f)
                {
                    float flashAmount = DamageFlashMaxAlpha * (1f - normalizedTimeSinceDamage);
                    FlashCanvasGroup.alpha = flashAmount;
                }
                else
                {
                    FlashCanvasGroup.gameObject.SetActive(false);
                    FlashActive = false;
                }
            }
        }

        void ResetFlash()
        {
            LastTimeFlashStarted = Time.time;
            FlashActive = true;
            FlashCanvasGroup.alpha = 0f;
            FlashCanvasGroup.gameObject.SetActive(true);
        }

        void OnTakeDamage(float dmg, GameObject damageSource)
        {
            ResetFlash();
            FlashImage.color = DamageFlashColor;
        }

        void OnHealed(float amount)
        {
            ResetFlash();
            FlashImage.color = HealFlashColor;
        }
    }
}