using FPS.Scripts.Game;
using FPS.Scripts.Game.Managers;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay.Managers;
using UnityEngine;

namespace FPS.Scripts.Gameplay
{
    public class AmmoPickup : Pickup
    {
        [Tooltip("Weapon those bullets are for")]
        public WeaponController Weapon;

        [Tooltip("Number of bullets the player gets")]
        public int BulletCount = 30;

        protected override void OnPicked(PlayerCharacterController byPlayer)
        {
            PlayerWeaponsManager playerWeaponsManager = byPlayer.GetComponent<PlayerWeaponsManager>();
            if (playerWeaponsManager)
            {
                WeaponController weapon = playerWeaponsManager.HasWeapon(Weapon);
                if (weapon && weapon is GunController)
                {
                    var gun = (GunController)weapon;
                    gun.AddCarriablePhysicalBullets(BulletCount);

                    AmmoPickupEvent evt = Events.AmmoPickupEvent;
                    evt.Weapon = gun;
                    EventManager.Broadcast(evt);

                    PlayPickupFeedback();
                    Destroy(gameObject);
                }
            }
        }
    }
}
