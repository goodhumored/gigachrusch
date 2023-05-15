using System;
using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    public class PlayerHungerBarController : MonoBehaviour
    {
        public PlayerBar hungerBar;
        private Hunger _playerHunger;

        void Start()
        {
            _playerHunger = FindObjectOfType<PlayerCharacterController>().GetComponent<Hunger>();
            hungerBar.SetMaxValue(_playerHunger.maxHunger);
        }

        private void Update()
        {
            hungerBar.SetCurrentValue(_playerHunger.currentHunger);
        }
    }
}