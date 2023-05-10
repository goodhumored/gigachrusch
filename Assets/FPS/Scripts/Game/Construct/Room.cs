using System;
using UnityEngine;

namespace FPS.Scripts.Game.Construct
{
    public class Room : MonoBehaviour
    {   
        
        public float RoomWidth = 15f;
        public float RoomLength = 15f;
        public float RoomHeight = 15f;
        
        public Wall wallNorth;
        public Wall wallSouth;
        public Wall wallWest;
        public Wall wallEast;
        
        public Room roomNorth;
        public Room roomSouth;
        public Room roomWest;
        public Room roomEast;
        
        public Wall.WallType[] wallNorthTypes;
        public Wall.WallType[] wallSouthTypes;
        public Wall.WallType[] wallWestTypes;
        public Wall.WallType[] wallEastTypes;
        
        [Range(0, 1f)]
        public float chanceToSpawn = .15f;

        public Vector3 GetNorthWallPosition()
        {
            var roomTransform = transform;
            return roomTransform.position + new Vector3(0, 0, RoomLength/2f);
        }

        public Vector3 GetNorthRoomPosition()
        {
            var roomTransform = transform;
            return roomTransform.position + new Vector3(0, 0, RoomLength);
        }

        public Room GetNorthRoom()
        {
            if (!roomNorth)
            { 
                foreach (var hitCollider in Physics.OverlapSphere(GetNorthRoomPosition(), 1f,  LayerMask.NameToLayer("Default")))
                {
                    if (hitCollider.gameObject.CompareTag("Room")) roomNorth = hitCollider.gameObject.GetComponent<Room>();
                }
            }

            return roomNorth;
        }

        public Vector3 GetEastWallPosition()
        {
            var roomTransform = transform;
            return roomTransform.position + new Vector3(RoomWidth/2f, 0, 0);
        }
        
        public Vector3 GetEastRoomPosition()
        {
            var roomTransform = transform;
            return roomTransform.position + new Vector3(RoomWidth, 0, 0);
        }

        public Room GetEastRoom()
        {
            if (!roomEast)
            {
                foreach (var hitCollider in Physics.OverlapSphere(GetNorthRoomPosition(), 1f,  LayerMask.NameToLayer("Default")))
                {
                    if (hitCollider.gameObject.CompareTag("Room")) roomNorth = hitCollider.gameObject.GetComponent<Room>();
                }
            }

            return roomEast;
        }

        public Vector3 GetSouthWallPosition()
        {
            var roomTransform = transform;
            return roomTransform.position - new Vector3(0, 0, RoomLength/2f);
        }
        
        public Vector3 GetSouthRoomPosition()
        {
            var roomTransform = transform;
            return roomTransform.position - new Vector3(0, 0, RoomLength);
        }

        public Room GetSouthRoom()
        {
            if (!roomSouth)
            {
                foreach (var hitCollider in Physics.OverlapSphere(GetNorthRoomPosition(), 1f,  LayerMask.NameToLayer("Default")))
                {
                    if (hitCollider.gameObject.CompareTag("Room")) roomNorth = hitCollider.gameObject.GetComponent<Room>();
                }
            }

            return roomSouth;
        }

        public Vector3 GetWestWallPosition()
        {
            var roomTransform = transform;
            return roomTransform.position - new Vector3(RoomWidth/2f, 0, 0);
        }
        
        public Vector3 GetWestRoomPosition()
        {
            var roomTransform = transform;
            return roomTransform.position - new Vector3(RoomWidth, 0, 0);
        }

        public Room GetWestRoom()
        {
            if (!roomWest)
            {
                foreach (var hitCollider in Physics.OverlapSphere(GetNorthRoomPosition(), 1f,  LayerMask.NameToLayer("Default")))
                {
                    if (hitCollider.gameObject.CompareTag("Room")) roomNorth = hitCollider.gameObject.GetComponent<Room>();
                }
            }

            return roomWest;
        }

        private void Awake()
        {
            var boxCollider = GetComponent<BoxCollider>();
            boxCollider.size = new Vector3(RoomWidth, RoomHeight, RoomLength);
            boxCollider.center = Vector3.up * RoomHeight / 2f;
        }
    }
}