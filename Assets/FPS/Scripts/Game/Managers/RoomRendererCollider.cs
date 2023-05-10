using UnityEngine;

namespace FPS.Scripts.Game.Managers
{
    public class RoomRendererCollider : MonoBehaviour
    {
        private BuildingManager _builderManager;
    
        void Start()
        {
            _builderManager = FindObjectOfType<BuildingManager>();
        }

        private void OnTriggerEnter(Collider roomCollider)
        {
            Debug.Log($"Something entered collision room renderer! Tag: {roomCollider.tag} {roomCollider.name} {roomCollider.gameObject.name} {roomCollider.gameObject.tag}");
            if (roomCollider.gameObject.CompareTag("Room"))
            {
                Debug.Log("Room entered collision room renderer!");
                roomCollider.gameObject.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider roomCollider)
        {
            Debug.Log($"Something exited collision room renderer! Tag: {roomCollider.tag}");
            if (roomCollider.gameObject.CompareTag("Room"))
            {
                Debug.Log("Room exited collision room renderer!");
                roomCollider.gameObject.SetActive(false);
            }
        }
    }
}
