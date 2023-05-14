using FPS.Scripts.Game.Construct;
using UnityEngine;

namespace FPS.Scripts.Game.Managers
{
    public class RoomRendererCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider roomCollider)
        {
            roomCollider.GetComponentInParent<Room>().Show();
        }

        private void OnTriggerExit(Collider roomCollider)
        {
            roomCollider.GetComponentInParent<Room>().Hide();
        }
    }
}
