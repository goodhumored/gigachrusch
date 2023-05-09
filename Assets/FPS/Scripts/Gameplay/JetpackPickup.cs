﻿namespace FPS.Scripts.Gameplay
{
    public class JetpackPickup : Pickup
    {
        protected override void OnPicked(PlayerCharacterController byPlayer)
        {
            var jetpack = byPlayer.GetComponent<Jetpack>();
            if (!jetpack)
                return;

            if (jetpack.TryUnlock())
            {
                PlayPickupFeedback();
                Destroy(gameObject);
            }
        }
    }
}