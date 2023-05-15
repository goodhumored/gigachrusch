using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using UnityEngine;

namespace FPS.Scripts.Gameplay
{
    public class ProjectileChargeParameters : MonoBehaviour
    {
        public MinMaxFloat Damage;
        public MinMaxFloat Radius;
        public MinMaxFloat Speed;
        public MinMaxFloat GravityDownAcceleration;
        public MinMaxFloat AreaOfEffectDistance;

        ProjectileBase ProjectileBase;

        void OnEnable()
        {
            ProjectileBase = GetComponent<ProjectileBase>();
            DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ProjectileChargeParameters>(ProjectileBase,
                this, gameObject);

            ProjectileBase.OnShoot += OnShoot;
        }

        void OnShoot()
        {
            // Apply the parameters based on projectile charge
            ProjectileStandard proj = GetComponent<ProjectileStandard>();
            if (proj)
            {
                proj.Damage = Damage.GetValueFromRatio(ProjectileBase.InitialCharge);
                proj.Radius = Radius.GetValueFromRatio(ProjectileBase.InitialCharge);
                proj.Speed = Speed.GetValueFromRatio(ProjectileBase.InitialCharge);
                proj.GravityDownAcceleration =
                    GravityDownAcceleration.GetValueFromRatio(ProjectileBase.InitialCharge);
            }
        }
    }
}