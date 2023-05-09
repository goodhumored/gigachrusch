using UnityEngine;

namespace FPS.Scripts.Game.Managers
{
    public class RoomRendererCollider : MonoBehaviour
    {
        public Vector3 roomsCount;
        private BuildingManager _builderManager;
    
        void Start()
        {
            _builderManager = FindObjectOfType<BuildingManager>();
        }

        private void OnCollisionEnter(Collision roomCollision)
        {
            Debug.Log("Something entered collision!");
            if (roomCollision.gameObject.CompareTag("Room"))
            {
                Debug.Log("Room entered collision!");
                roomCollision.gameObject.SetActive(true);
            }
        }

        private void OnCollisionExit(Collision roomCollision)
        {
            Debug.Log("Something exited collision!");
            if (roomCollision.gameObject.CompareTag("Room"))
            {
                Debug.Log("Room exited collision!");
                roomCollision.gameObject.SetActive(false);
            }
        }
    }
}
