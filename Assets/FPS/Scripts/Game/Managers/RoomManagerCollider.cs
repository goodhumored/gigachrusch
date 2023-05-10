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

        private void OnTriggerEnter(Collider roomCollider)
        {
            if (roomCollider.gameObject.CompareTag("Room"))
            {
                Debug.Log("Room entered collision!");
                _builderManager.CheckOrGenerateNeighbourRooms(roomCollider.gameObject.GetComponent<Room>());
            }
        }
    }
}
