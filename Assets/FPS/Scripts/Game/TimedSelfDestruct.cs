using UnityEngine;

namespace FPS.Scripts.Game
{
    public class TimedSelfDestruct : MonoBehaviour
    {
        public float LifeTime = 1f;

        float SpawnTime;

        void Awake()
        {
            SpawnTime = Time.time;
        }

        void Update()
        {
            if (Time.time > SpawnTime + LifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
}