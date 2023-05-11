using System;
using System.Collections;
using System.Collections.Generic;
using FPS.Scripts.Game.Construct;
using UnityEngine;

namespace FPS.Scripts.Game.Managers
{
    public class RoomManagerCollider : MonoBehaviour
    {
        private BuildingManager _builderManager;
        public Queue<Room> roomBuildQueue = new Queue<Room>();
    
        void Start()
        {
            _builderManager = FindObjectOfType<BuildingManager>();
        }

        private void OnTriggerEnter(Collider roomCollider)
        {
            if (roomCollider.gameObject.CompareTag("Room"))
            {
                Debug.Log("Room entered collision!");
                this.roomBuildQueue.Enqueue(roomCollider.GetComponent<Room>());
            }
        }

        private void Update()
        {
            if (roomBuildQueue.Count > 0)
            {
                _builderManager.CheckAndBuildNeighbourRooms(roomBuildQueue.Dequeue());
            }
        }
    }
}
