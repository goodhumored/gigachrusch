using UnityEngine;

namespace FPS.Scripts.Game.Shared
{
    public class Destructable : MonoBehaviour
    {
        Health Health;

        void Start()
        {
            Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, Destructable>(Health, this, gameObject);

            // Subscribe to damage & death actions
            Health.OnDie += OnDie;
            Health.OnDamaged += OnDamaged;
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            // TODO: damage reaction
        }

        void OnDie()
        {
            // this will call the OnDestroy function
            Destroy(gameObject);
        }
    }
}