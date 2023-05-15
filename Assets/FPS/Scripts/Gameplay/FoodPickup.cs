using FPS.Scripts.Game.Shared;
using UnityEngine;

namespace FPS.Scripts.Gameplay
{
    public class FoodPickup : Pickup
    {
        [Header("Parameters")]
        public float HungerAmount;

        protected override void OnPicked(PlayerCharacterController player)
        {
            Hunger playerHunger = player.GetComponent<Hunger>();
            if (playerHunger && playerHunger.CanRecover())
            {
                playerHunger.Recover(HungerAmount);
                PlayPickupFeedback();
                Destroy(gameObject);
            }
        }
    }
}