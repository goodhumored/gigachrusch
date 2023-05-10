using FPS.Scripts.Game.Construct;
using FPS.Scripts.Game.Managers.NavMesh;
using UnityEngine;

namespace FPS.Scripts.Game.Managers
{
    public class BuildingManager : MonoBehaviour
    {
        private RoomManager _roomManager;
        private WallManager _wallManager;
        private GameObject _player;

        private void Start()
        {
            _player = GameObject.FindWithTag("Player");
            _roomManager = GetComponent<RoomManager>();
            _wallManager = GetComponent<WallManager>();
            _roomManager.InstantiateRoom(_roomManager.GetRandomRoom(), _player.transform.position + Vector3.down * 2f);
            // CheckOrGenerateNeighbourRooms(room);
        }

        public void CheckOrGenerateNeighbourRooms(Room originRoom)
        {
            if (!originRoom.GetEastRoom())
            {
                var wall = _wallManager.GetRandomWall(originRoom.wallEastTypes);
                var room = _roomManager.GetRandomRoom(); // wall.type
                var createdWall = _wallManager.InstantiateWall(wall, originRoom.GetEastWallPosition(), false);
                var createdRoom = _roomManager.InstantiateRoom(room, originRoom.GetEastRoomPosition());
                originRoom.wallEast = createdWall;
                originRoom.roomEast = createdRoom;
                createdRoom.wallWest = createdWall;
                createdRoom.roomWest = createdRoom;
                originRoom.GetComponentInChildren<NavMeshSurface>().BuildNavMesh();
            }

            if (!originRoom.GetWestRoom())
            {
                var wall = _wallManager.GetRandomWall(originRoom.wallWestTypes);
                var room = _roomManager.GetRandomRoom(); // wall.type
                var createdWall = _wallManager.InstantiateWall(wall, originRoom.GetWestWallPosition(), false);
                var createdRoom = _roomManager.InstantiateRoom(room, originRoom.GetWestRoomPosition());
                originRoom.wallWest = createdWall;
                originRoom.roomWest = createdRoom;
                createdRoom.wallEast = createdWall;
                createdRoom.roomEast = createdRoom;
                originRoom.GetComponentInChildren<NavMeshSurface>().BuildNavMesh();
            }

            if (!originRoom.GetSouthRoom())
            {
                var wall = _wallManager.GetRandomWall(originRoom.wallSouthTypes);
                var room = _roomManager.GetRandomRoom(); // wall.type
                var createdWall = _wallManager.InstantiateWall(wall, originRoom.GetSouthWallPosition(), true);
                var createdRoom = _roomManager.InstantiateRoom(room, originRoom.GetSouthRoomPosition());
                originRoom.wallSouth = createdWall;
                originRoom.roomSouth = createdRoom;
                createdRoom.wallNorth = createdWall;
                createdRoom.roomNorth = createdRoom;
                originRoom.GetComponentInChildren<NavMeshSurface>().BuildNavMesh();
            }

            if (!originRoom.GetNorthRoom())
            {
                var wall = _wallManager.GetRandomWall(originRoom.wallNorthTypes);
                var room = _roomManager.GetRandomRoom(); // wall.type
                var createdWall = _wallManager.InstantiateWall(wall, originRoom.GetNorthWallPosition(), true);
                var createdRoom = _roomManager.InstantiateRoom(room, originRoom.GetNorthRoomPosition());
                originRoom.wallNorth = createdWall;
                originRoom.roomNorth = createdRoom;
                createdRoom.wallSouth = createdWall;
                createdRoom.roomSouth = createdRoom;
                originRoom.GetComponentInChildren<NavMeshSurface>().BuildNavMesh();
            }
        }
    }
}