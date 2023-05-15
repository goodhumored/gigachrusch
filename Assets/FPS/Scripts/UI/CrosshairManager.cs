using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    public class CrosshairManager : MonoBehaviour
    {
        public Image CrosshairImage;
        public Sprite NullCrosshairSprite;
        public float CrosshairUpdateshrpness = 5f;

        PlayerWeaponsManager WeaponsManager;
        bool WasPointingAtEnemy;
        RectTransform CrosshairRectTransform;
        CrosshairData CrosshairDataDefault;
        CrosshairData CrosshairDataTarget;
        CrosshairData CurrentCrosshair;

        void Start()
        {
            WeaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, CrosshairManager>(WeaponsManager, this);

            OnWeaponChanged(WeaponsManager.GetActiveWeapon());

            WeaponsManager.OnSwitchedToWeapon += OnWeaponChanged;
        }

        void Update()
        {
            UpdateCrosshairPointingAtEnemy(false);
            WasPointingAtEnemy = WeaponsManager.IsPointingAtEnemy;
        }

        void UpdateCrosshairPointingAtEnemy(bool force)
        {
            if (CrosshairDataDefault.CrosshairSprite == null)
                return;

            if ((force || !WasPointingAtEnemy) && WeaponsManager.IsPointingAtEnemy)
            {
                CurrentCrosshair = CrosshairDataTarget;
                CrosshairImage.sprite = CurrentCrosshair.CrosshairSprite;
                CrosshairRectTransform.sizeDelta = CurrentCrosshair.CrosshairSize * Vector2.one;
            }
            else if ((force || WasPointingAtEnemy) && !WeaponsManager.IsPointingAtEnemy)
            {
                CurrentCrosshair = CrosshairDataDefault;
                CrosshairImage.sprite = CurrentCrosshair.CrosshairSprite;
                CrosshairRectTransform.sizeDelta = CurrentCrosshair.CrosshairSize * Vector2.one;
            }

            CrosshairImage.color = Color.Lerp(CrosshairImage.color, CurrentCrosshair.CrosshairColor,
                Time.deltaTime * CrosshairUpdateshrpness);

            CrosshairRectTransform.sizeDelta = Mathf.Lerp(CrosshairRectTransform.sizeDelta.x,
                CurrentCrosshair.CrosshairSize,
                Time.deltaTime * CrosshairUpdateshrpness) * Vector2.one;
        }

        void OnWeaponChanged(WeaponController newWeapon)
        {
            if (newWeapon)
            {
                CrosshairImage.enabled = true;
                CrosshairDataDefault = newWeapon.CrosshairDataDefault;
                CrosshairDataTarget = newWeapon.CrosshairDataTargetInSight;
                CrosshairRectTransform = CrosshairImage.GetComponent<RectTransform>();
                DebugUtility.HandleErrorIfNullGetComponent<RectTransform, CrosshairManager>(CrosshairRectTransform,
                    this, CrosshairImage.gameObject);
            }
            else
            {
                if (NullCrosshairSprite)
                {
                    CrosshairImage.sprite = NullCrosshairSprite;
                }
                else
                {
                    CrosshairImage.enabled = false;
                }
            }

            UpdateCrosshairPointingAtEnemy(true);
        }
    }
}