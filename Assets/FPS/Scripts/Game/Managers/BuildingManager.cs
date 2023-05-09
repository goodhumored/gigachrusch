using FPS.Scripts.Game.Construct;
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
            var room = _roomManager.InstantiateRoom(_roomManager.GetRandomRoom(), _player.transform.position);
            CheckOrGenerateNeighbourRooms(room);
        }

        public void CheckOrGenerateNeighbourRooms(Room originRoom)
        {
            if (!originRoom.GetEastRoom())
            {
                var wall = _wallManager.GetRandomWall(originRoom.wallEastTypes);
                var room = _roomManager.GetRandomRoom(); // wall.type
                var createdWall = _wallManager.InstantiateWall(wall, originRoom.GetEastWallPosition(), false);
                var createdRoom = _roomManager.InstantiateRoom(room, originRoom.GetEastRoomPosition());
                room.wallEast = createdWall;
                room.roomEast = createdRoom;
            }

            if (!originRoom.GetWestRoom())
            {
                var wall = _wallManager.GetRandomWall(originRoom.wallWestTypes);
                var room = _roomManager.GetRandomRoom(); // wall.type
                var createdWall = _wallManager.InstantiateWall(wall, originRoom.GetWestWallPosition(), false);
                var createdRoom = _roomManager.InstantiateRoom(room, originRoom.GetWestRoomPosition());
                room.wallWest = createdWall;
                room.roomWest = createdRoom;
            }

            if (!originRoom.GetSouthRoom())
            {
                var wall = _wallManager.GetRandomWall(originRoom.wallSouthTypes);
                var room = _roomManager.GetRandomRoom(); // wall.type
                var createdWall = _wallManager.InstantiateWall(wall, originRoom.GetSouthWallPosition(), true);
                var createdRoom = _roomManager.InstantiateRoom(room, originRoom.GetSouthRoomPosition());
                room.wallSouth = createdWall;
                room.roomSouth = createdRoom;
            }

            if (!originRoom.GetNorthRoom())
            {
                var wall = _wallManager.GetRandomWall(originRoom.wallNorthTypes);
                var room = _roomManager.GetRandomRoom(); // wall.type
                var createdWall = _wallManager.InstantiateWall(wall, originRoom.GetNorthWallPosition(), true);
                var createdRoom = _roomManager.InstantiateRoom(room, originRoom.GetNorthRoomPosition());
                room.wallNorth = createdWall;
                room.roomNorth = createdRoom;
            }
        }
    }
}