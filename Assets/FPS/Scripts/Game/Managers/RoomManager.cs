using System.Collections.Generic;
using FPS.Scripts.Game.Construct;
using FPS.Scripts.Game.Managers.Common;
using UnityEngine;
using Random = System.Random;

namespace FPS.Scripts.Game.Managers
{
    public class RoomManager : MonoBehaviour
    {
        public static int RoomN;
        public List<Room> availableRoomsToSpawn;
        private Dictionary<Vector3, Room> _instantiatedRooms = new Dictionary<Vector3, Room>();
        public Transform roomsParent;

        public Room GetRoomByPosition(Vector3 position)
        {
            // Debug.Log($"Getting room at position {position}");
            if (_instantiatedRooms.TryGetValue(position, out var roomByPosition))
            {
                // Debug.Log($"Room at position {position}: {roomByPosition.name}");
                return roomByPosition;
            }
            return null;
        }
        
        private void AddRoomWithPosition(Vector3 position, Room room)
        {
            // Debug.Log($"Adding {room.name} to position {position}");
            _instantiatedRooms.TryAdd(position, room);
        }

        public Room GetRandomRoom(RoomConstraints roomConstraints)
        {
            //TODO: implement search
            var rnd = new Random();
            var roomToReturn = availableRoomsToSpawn[rnd.Next(availableRoomsToSpawn.Count)];
            return roomToReturn;
        }

        public Room InstantiateRoom(Room room, Vector3 coordinates)
        {
            var createdRoom = Instantiate(room, coordinates, new Quaternion());
            createdRoom.transform.SetParent(roomsParent);
            createdRoom.name += RoomN++;
            AddRoomWithPosition(coordinates, createdRoom);
            return createdRoom;
        }
    }
}