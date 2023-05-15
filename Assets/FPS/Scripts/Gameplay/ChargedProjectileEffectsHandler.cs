using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using UnityEngine;

namespace FPS.Scripts.Gameplay
{
    public class ChargedProjectileEffectsHandler : MonoBehaviour
    {
        [Tooltip("Object that will be affected by charging scale & color changes")]
        public GameObject ChargingObject;

        [Tooltip("Scale of the charged object based on charge")]
        public MinMaxVector3 Scale;

        [Tooltip("Color of the charged object based on charge")]
        public MinMaxColor Color;

        MeshRenderer[] AffectedRenderers;
        ProjectileBase ProjectileBase;

        void OnEnable()
        {
            ProjectileBase = GetComponent<ProjectileBase>();
            DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ChargedProjectileEffectsHandler>(
                ProjectileBase, this, gameObject);

            ProjectileBase.OnShoot += OnShoot;

            AffectedRenderers = ChargingObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var ren in AffectedRenderers)
            {
                ren.sharedMaterial = Instantiate(ren.sharedMaterial);
            }
        }

        void OnShoot()
        {
            ChargingObject.transform.localScale = Scale.GetValueFromRatio(ProjectileBase.InitialCharge);

            foreach (var ren in AffectedRenderers)
            {
                ren.sharedMaterial.SetColor("_Color", Color.GetValueFromRatio(ProjectileBase.InitialCharge));
            }
        }
    }
}