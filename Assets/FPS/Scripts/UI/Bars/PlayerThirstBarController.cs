using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay;
using UnityEngine;

namespace FPS.Scripts.UI
{
    public class PlayerThirstBarController : MonoBehaviour
    {
        public PlayerBar thirstBar;
        private Thirst _playerThirst;

        void Start()
        {
            _playerThirst = FindObjectOfType<PlayerCharacterController>().GetComponent<Thirst>();
            thirstBar.SetMaxValue(_playerThirst.maxThirst);
        }

        private void Update()
        {
            thirstBar.SetCurrentValue(_playerThirst.currentThirst);
        }
    }
}