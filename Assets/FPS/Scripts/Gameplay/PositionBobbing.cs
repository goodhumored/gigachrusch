
using UnityEngine;

namespace FPS.Scripts.Gameplay
{
    public class PositionBobbing : MonoBehaviour
    {
        [Tooltip("Frequency at which the item will move up and down")]
        public float VerticalBobFrequency = 1f;

        [Tooltip("Distance the item will move up and down")]
        public float BobbingAmount = 0.5f;

        Vector3 StartPosition;

        void Start()
        {
            // Remember start position for animation
            StartPosition = transform.position;
        }

        void Update()
        {
            // Handle bobbing
            float bobbingAnimationPhase = ((Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f) + 0.5f) * BobbingAmount;
            transform.position = StartPosition + Vector3.up * bobbingAnimationPhase;
        }
    }
}