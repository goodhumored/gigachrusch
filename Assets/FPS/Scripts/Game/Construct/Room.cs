using FPS.Scripts.Game.Managers;
using FPS.Scripts.Game.Managers.Common;
using UnityEngine;
using UnityEngine.AI;

namespace FPS.Scripts.Game.Construct
{
    public class Room : MonoBehaviour
    {
        public NavMeshSurface surface;
        public BoxCollider RoomCollider;
        
        public Wall wallNorth;
        public Wall wallSouth;
        public Wall wallWest;
        public Wall wallEast;
        
        public Room roomNorth;
        public Room roomSouth;
        public Room roomWest;
        public Room roomEast;
        
        public WallType[] wallNorthTypes;
        public WallType[] wallSouthTypes;
        public WallType[] wallWestTypes;
        public WallType[] wallEastTypes;
        
        [Range(0, 1f)]
        public float chanceToSpawn = .15f;

        private void Awake()
        {
            RoomCollider.size = new Vector3(RoomConstants.RoomWidth, RoomConstants.RoomHeight, RoomConstants.RoomLength);
            RoomCollider.center = Vector3.up * RoomConstants.RoomHeight / 2f;
        }

        public Vector3 GetWallPositionBySide(Side side)
        {
            var vector = BuildingManager.GetVectorBySide(side);
            return transform.position + vector * RoomConstants.RoomLength/2f;
        }

        public Vector3 GetNeighbourRoomPositionBySide(Side side)
        {
            var vector = BuildingManager.GetVectorBySide(side);
            // Debug.Log($"[{gameObject.name}] position now is {transform.position.ToString()}");
            return transform.position + vector * RoomConstants.RoomLength;
        }

        public Room GetNeighbourRoomBySide(Side side)
        {
            switch (side)
            {
                case Side.East: return roomEast;
                case Side.North: return roomNorth;
                case Side.West: return roomWest;
                default: return roomSouth;
            }
        }

        public void SetNeighbourRoomBySide(Room room, Side side)
        {
            // Debug.Log($"[{gameObject.name}] Setting room {(room ? room.name : "null")} as neighbour from {side}");
            switch (side)
            {
                case Side.East: roomEast = room; break;
                case Side.North: roomNorth = room; break;
                case Side.West: roomWest = room; break;
                default: roomSouth = room; break;
            }
        }

        public Wall GetWallBySide(Side side)
        {
            switch (side)
            {
                case Side.East: return wallEast;
                case Side.North: return wallNorth;
                case Side.West: return wallWest;
                default: return wallSouth;
            }
        }

        public void SetWallBySide(Wall wall, Side side)
        {
            // Debug.Log($"[{gameObject.name}] Setting wall {(wall ? wall.name : "null")} as {side} wall");
            switch (side)
            {
                case Side.East: wallEast = wall; break;
                case Side.North: wallNorth = wall; break;
                case Side.West: wallWest = wall; break;
                default: wallSouth = wall; break;
            }
        }

        public WallType[] GetWallTypesBySide(Side side)
        {
            switch (side)
            {
                case Side.East: return wallEastTypes;
                case Side.North: return wallNorthTypes;
                case Side.West: return wallWestTypes;
                default: return wallSouthTypes;
            }
        }
    }
}