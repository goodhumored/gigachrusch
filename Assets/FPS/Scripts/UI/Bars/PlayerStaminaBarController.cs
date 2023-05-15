using System;
using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    public class PlayerStaminaBarController : MonoBehaviour
    {
        public PlayerBar staminaBar;
        private Stamina _playerStamina;

        void Start()
        {
            _playerStamina = FindObjectOfType<PlayerCharacterController>().GetComponent<Stamina>();
            staminaBar.SetMaxValue(_playerStamina.maxStamina);
        }

        private void Update()
        {
            staminaBar.SetCurrentValue(_playerStamina.currentStamina);
        }
    }
}