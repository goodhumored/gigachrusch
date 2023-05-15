using UnityEngine;

namespace FPS.Scripts.Game.Shared
{
    [RequireComponent(typeof(Hunger)), RequireComponent(typeof(Thirst))]
    public class Stamina : MonoBehaviour
    {
        public float maxStamina = 10f;
        public float recoverSpeedPerSec = 1f;
        public float hungerConstPerStamina = 0.1f;
        public float secsToWaitBeforeRecover = 1f;
        public float criticalStaminaRatio = 0.3f;
        public float currentStamina;

        private Hunger _hunger;
        private Thirst _thirst;
        private float _lastReduceTime;

        public bool CanRecover() =>
            currentStamina < maxStamina && _lastReduceTime + secsToWaitBeforeRecover < Time.time;

        public float GetRatio() => currentStamina / maxStamina;

        void Start()
        {
            _hunger = GetComponent<Hunger>();
            _thirst = GetComponent<Thirst>();
            currentStamina = maxStamina;
        }

        public bool Reduce(float amount)
        {
            if (currentStamina >= amount)
            {
                _lastReduceTime = Time.time;
                currentStamina -= amount;
                return true;
            }

            return false;
        }

        public void Recover(float amount)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        }

        private void Update()
        {
            if (CanRecover())
            {
                var staminaToRecover = recoverSpeedPerSec * Time.deltaTime;
                _hunger.Reduce(staminaToRecover * hungerConstPerStamina);
                _thirst.Reduce(staminaToRecover * hungerConstPerStamina);
                Recover(staminaToRecover);
            }
        }
    }
}