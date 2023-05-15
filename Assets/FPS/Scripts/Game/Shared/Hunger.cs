using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace FPS.Scripts.Game.Shared
{
    public class Hunger : MonoBehaviour
    {
        public float maxHunger = 10f;
        public float reduceSpeedPerSec = 0.1f;
        public float criticalHungerRatio = 0.3f;

        public float currentHunger;
        private float _lastReduceTime;

        public bool CanRecover() => currentHunger < maxHunger;

        public float GetRatio() => currentHunger / maxHunger;

        void Start()
        {
            currentHunger = maxHunger;
        }

        public void Reduce(float amount)
        {
            currentHunger -= amount;
        }

        public void Recover(float amount)
        {
            currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
        }

        private void Update()
        {
            Reduce(reduceSpeedPerSec * Time.deltaTime);
        }

        public bool IsStarving()
        {
            return GetRatio() < criticalHungerRatio;
        }
    }
}