using System;
using UnityEngine;
using UnityEngine.Events;

namespace FPS.Scripts.Game.Shared
{
    [RequireComponent(typeof(Hunger))]
    public class Health : MonoBehaviour
    {
        private Hunger _hunger;
        public float MaxHealth = 10f;
        public float CriticalHealthRatio = 0.3f;
        public float regenerateSpeedPerSecond = 0.5f; 
        public float hungryCostPerHp = 0.2f; 

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;
        public UnityAction OnDie;

        public float CurrentHealth { get; set; }
        public bool Invincible { get; set; }
        public bool CanBeHealed() => CurrentHealth < MaxHealth;

        public float GetRatio() => CurrentHealth / MaxHealth;
        public bool IsCritical() => GetRatio() <= CriticalHealthRatio;
        private bool _isDead;

        private void Start()
        {
            _hunger = GetComponent<Hunger>();
            CurrentHealth = MaxHealth;
        }

        private void Update()
        {
            if (CanBeHealed())
            {
                var healthToRecover = regenerateSpeedPerSecond * Time.deltaTime;
                _hunger.Reduce(healthToRecover * hungryCostPerHp);
                Heal(healthToRecover, true);
            }
        }

        public void Heal(float healAmount, bool isRegen = false)
        {
            float healthBefore = CurrentHealth;
            CurrentHealth += healAmount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
            
            float trueHealAmount = CurrentHealth - healthBefore;
            if (trueHealAmount > 0f && !isRegen)
            {
                OnHealed?.Invoke(trueHealAmount);
            }
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (Invincible)
                return;

            float healthBefore = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            // call OnDamage action
            float trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0f)
            {
                OnDamaged?.Invoke(trueDamageAmount, damageSource);
            }

            HandleDeath();
        }

        public void Kill()
        {
            CurrentHealth = 0f;

            // call OnDamage action
            OnDamaged?.Invoke(MaxHealth, null);

            HandleDeath();
        }

        void HandleDeath()
        {
            if (_isDead)
                return;

            // call OnDie action
            if (CurrentHealth <= 0f)
            {
                _isDead = true;
                OnDie?.Invoke();
            }
        }
    }
}