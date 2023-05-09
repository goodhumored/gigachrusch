using FPS.Scripts.Game.Construct;
using UnityEngine;

namespace FPS.Scripts.Game.Managers
{
    public class RoomManagerCollider : MonoBehaviour
    {
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
                _builderManager.CheckOrGenerateNeighbourRooms(roomCollision.gameObject.GetComponent<Room>());
            }
        }
    }
}
