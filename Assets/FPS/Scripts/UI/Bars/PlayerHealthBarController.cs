using System;
using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    public class PlayerHealthBarController : MonoBehaviour
    {
        public PlayerBar healthBar;
        private Health _playerHealth;

        void Start()
        {
            _playerHealth = FindObjectOfType<PlayerCharacterController>().GetComponent<Health>();
            healthBar.SetMaxValue(_playerHealth.MaxHealth);
        }

        private void Update()
        {
            healthBar.SetCurrentValue(_playerHealth.CurrentHealth);
        }
    }
}