using FPS.Scripts.Game;
using FPS.Scripts.Game.Managers;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    [RequireComponent(typeof(FillBarColorChange))]
    public class AmmoCounter : MonoBehaviour
    {
        [Tooltip("CanvasGroup to fade the ammo UI")]
        public CanvasGroup CanvasGroup;

        [Tooltip("Image for the weapon icon")] public Image WeaponImage;

        [Tooltip("Image component for the background")]
        public Image AmmoBackgroundImage;

        [Tooltip("Image component to display fill ratio")]
        public Image AmmoFillImage;

        [Tooltip("Text for Weapon index")] 
        public TextMeshProUGUI WeaponIndexText;

        [Tooltip("Text for Bullet Counter")] 
        public TextMeshProUGUI BulletCounter;

        [Tooltip("Reload Text for Weapons with physical bullets")]
        public RectTransform Reload;

        [Header("Selection")] [Range(0, 1)] [Tooltip("Opacity when weapon not selected")]
        public float UnselectedOpacity = 0.5f;

        [Tooltip("Scale when weapon not selected")]
        public Vector3 UnselectedScale = Vector3.one * 0.8f;

        [Tooltip("Root for the control keys")] public GameObject ControlKeysRoot;

        [Header("Feedback")] [Tooltip("Component to animate the color when empty or full")]
        public FillBarColorChange FillBarColorChange;

        [Tooltip("Sharpness for the fill ratio movements")]
        public float AmmoFillMovementSharpness = 20f;

        public int WeaponCounterIndex { get; set; }

        PlayerWeaponsManager PlayerWeaponsManager;
        WeaponController Weapon;

        void Awake()
        {
            EventManager.AddListener<AmmoPickupEvent>(OnAmmoPickup);
        }

        void OnAmmoPickup(AmmoPickupEvent evt)
        {
            if (Weapon is GunController && evt.Weapon == Weapon)
            {
                var gun = (GunController)Weapon;
                BulletCounter.text = gun.GetCarriedPhysicalBullets().ToString();
            }
        }

        public void Initialize(WeaponController weapon, int weaponIndex)
        {
            Weapon = weapon;
            WeaponCounterIndex = weaponIndex;
            WeaponImage.sprite = weapon.WeaponIcon;

            Reload.gameObject.SetActive(false);
            PlayerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, AmmoCounter>(PlayerWeaponsManager, this);

            WeaponIndexText.text = (WeaponCounterIndex + 1).ToString();
            if (weapon is GunController)
            {
                var gun = (GunController)weapon;
                if (!gun.HasPhysicalBullets)
                    BulletCounter.transform.parent.gameObject.SetActive(false);
                else
                    BulletCounter.text = gun.GetCarriedPhysicalBullets().ToString();
                FillBarColorChange.Initialize(1f, gun.GetAmmoNeededToShoot());
            }
        }

        void Update()
        {
            if (Weapon is GunController)
            {
                var gun = (GunController)Weapon;
                float currenFillRatio = gun.CurrentAmmoRatio;
                AmmoFillImage.fillAmount = Mathf.Lerp(AmmoFillImage.fillAmount, currenFillRatio,
                    Time.deltaTime * AmmoFillMovementSharpness);

                BulletCounter.text = gun.GetCarriedPhysicalBullets().ToString();

                bool isActiveWeapon = gun == PlayerWeaponsManager.GetActiveWeapon();

                CanvasGroup.alpha = Mathf.Lerp(CanvasGroup.alpha, isActiveWeapon ? 1f : UnselectedOpacity,
                    Time.deltaTime * 10);
                transform.localScale = Vector3.Lerp(transform.localScale,
                    isActiveWeapon ? Vector3.one : UnselectedScale,
                    Time.deltaTime * 10);
                ControlKeysRoot.SetActive(!isActiveWeapon);

                FillBarColorChange.UpdateVisual(currenFillRatio);

                Reload.gameObject.SetActive(gun.GetCarriedPhysicalBullets() > 0 &&
                                            gun.GetCurrentAmmo() == 0 && gun.IsWeaponActive);
            }
        }

        void Destroy()
        {
            EventManager.RemoveListener<AmmoPickupEvent>(OnAmmoPickup);
        }
    }
}