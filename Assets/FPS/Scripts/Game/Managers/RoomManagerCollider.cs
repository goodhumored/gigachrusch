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
    
        void Start()
        {
            _builderManager = FindObjectOfType<BuildingManager>();
        }

        private void OnTriggerEnter(Collider roomCollider)
        {
            var room = roomCollider.GetComponentInParent<Room>();
            _builderManager.CheckAndBuildNeighbourRooms(room);
        }
    }
}
