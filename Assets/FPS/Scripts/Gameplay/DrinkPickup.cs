using FPS.Scripts.Game.Shared;
using UnityEngine;

namespace FPS.Scripts.Gameplay
{
    public class DrinkPickup : Pickup
    {
        [Header("Parameters")]
        public float ThirstAmount;

        protected override void OnPicked(PlayerCharacterController player)
        {
            var playerThirst = player.GetComponent<Thirst>();
            if (playerThirst && playerThirst.CanRecover())
            {
                playerThirst.Recover(ThirstAmount);
                PlayPickupFeedback();
                Destroy(gameObject);
            }
        }
    }
}