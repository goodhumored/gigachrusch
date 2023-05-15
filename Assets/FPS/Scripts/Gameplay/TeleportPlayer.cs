using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using UnityEngine;

namespace FPS.Scripts.Gameplay
{
    // Debug script, teleports the player across the map for faster testing
    public class TeleportPlayer : MonoBehaviour
    {
        public KeyCode ActivateKey = KeyCode.F12;

        PlayerCharacterController PlayerCharacterController;

        void Awake()
        {
            PlayerCharacterController = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, TeleportPlayer>(
                PlayerCharacterController, this);
        }

        void Update()
        {
            if (Input.GetKeyDown(ActivateKey))
            {
                PlayerCharacterController.transform.SetPositionAndRotation(transform.position, transform.rotation);
                Health playerHealth = PlayerCharacterController.GetComponent<Health>();
                if (playerHealth)
                {
                    playerHealth.Heal(999);
                }
            }
        }

    }
}