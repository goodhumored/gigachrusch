using System.Collections.Generic;
using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay.Managers;
using UnityEngine;

namespace FPS.Scripts.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying weapon ammo")]
        public RectTransform AmmoPanel;

        [Tooltip("Prefab for displaying weapon ammo")]
        public GameObject AmmoCounterPrefab;

        PlayerWeaponsManager PlayerWeaponsManager;
        List<AmmoCounter> AmmoCounters = new List<AmmoCounter>();

        void Start()
        {
            PlayerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, WeaponHUDManager>(PlayerWeaponsManager,
                this);

            WeaponController activeWeapon = PlayerWeaponsManager.GetActiveWeapon();
            if (activeWeapon)
            {
                AddWeapon(activeWeapon, PlayerWeaponsManager.ActiveWeaponIndex);
                ChangeWeapon(activeWeapon);
            }

            PlayerWeaponsManager.OnAddedWeapon += AddWeapon;
            PlayerWeaponsManager.OnRemovedWeapon += RemoveWeapon;
            PlayerWeaponsManager.OnSwitchedToWeapon += ChangeWeapon;
        }

        void AddWeapon(WeaponController newWeapon, int weaponIndex)
        {
            var ammoCounterInstance = Instantiate(AmmoCounterPrefab, AmmoPanel);
            var newAmmoCounter = ammoCounterInstance.GetComponent<AmmoCounter>();
            newAmmoCounter.Initialize(newWeapon, weaponIndex);

            AmmoCounters.Add(newAmmoCounter);
        }

        void RemoveWeapon(WeaponController newWeapon, int weaponIndex)
        {
            int foundCounterIndex = -1;
            for (int i = 0; i < AmmoCounters.Count; i++)
            {
                if (AmmoCounters[i].WeaponCounterIndex == weaponIndex)
                {
                    foundCounterIndex = i;
                    Destroy(AmmoCounters[i].gameObject);
                }
            }

            if (foundCounterIndex >= 0)
            {
                AmmoCounters.RemoveAt(foundCounterIndex);
            }
        }

        void ChangeWeapon(WeaponController weapon)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(AmmoPanel);
        }
    }
}