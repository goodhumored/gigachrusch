using FPS.Scripts.Game.Managers;
using UnityEngine;

namespace FPS.Scripts.AI
{
    public class FollowPlayer : MonoBehaviour
    {
        Transform PlayerTransform;
        Vector3 OriginalOffset;

        void Start()
        {
            ActorsManager actorsManager = FindObjectOfType<ActorsManager>();
            if (actorsManager != null)
                PlayerTransform = actorsManager.Player.transform;
            else
            {
                enabled = false;
                return;
            }

            OriginalOffset = transform.position - PlayerTransform.position;
        }

        void LateUpdate()
        {
            transform.position = PlayerTransform.position + OriginalOffset;
        }
    }
}