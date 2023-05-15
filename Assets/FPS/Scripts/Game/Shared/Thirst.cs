using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace FPS.Scripts.Game.Shared
{
    public class Thirst : MonoBehaviour
    {
        public float maxThirst = 10f;
        public float reduceSpeedPerSec = 0.1f;
        public float criticalThirstRatio = 0.3f;

        public float currentThirst;
        private float _lastReduceTime;

        public bool CanRecover() => currentThirst < maxThirst;

        public float GetRatio() => currentThirst / maxThirst;

        void Start()
        {
            currentThirst = maxThirst;
        }

        public void Reduce(float amount)
        {
            currentThirst -= amount;
        }

        public void Recover(float amount)
        {
            currentThirst = Mathf.Min(maxThirst, currentThirst + amount);
        }

        private void Update()
        {
            Reduce(reduceSpeedPerSec * Time.deltaTime);
        }

        public bool IsThirsty()
        {
            return GetRatio() < criticalThirstRatio;
        }
    }
}