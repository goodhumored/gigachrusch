using System.Collections.Generic;
using FPS.Scripts.Game.Construct;
using FPS.Scripts.Game.Managers.Common;
using FPS.Scripts.Game.Managers.NavMesh;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace FPS.Scripts.Game.Managers
{
    public class RoomManager : MonoBehaviour
    {
        public static int RoomN;
        public List<Room> RoomsCatalog;
        private Dictionary<Vector3, Room> _instantiatedRooms;
        public Transform RoomsParent;

        public Room GetRoomByPosition(Vector3 position)
        {
            return _instantiatedRooms[position];
        }
        
        private Room AddRoomWithPosition(Vector3 position, Room room)
        {
            return _instantiatedRooms[position] = room;
        }

        public Room GetRandomRoom(RoomConstraints roomConstraints)
        {
            //TODO: implement search
            var rnd = new Random();
            var roomToReturn = RoomsCatalog[rnd.Next(RoomsCatalog.Count)];
            if (roomToReturn.chanceToSpawn * 100 > rnd.Next(100)) return roomToReturn;
            return GetRandomRoom(roomConstraints);
        }

        public Room InstantiateRoom(Room room, Vector3 coordinates)
        {
            var createdRoom = Instantiate(room, coordinates, new Quaternion());
            createdRoom.transform.SetParent(RoomsParent);
            createdRoom.name += RoomN++;
            createdRoom.surface.BuildNavMesh();
            AddRoomWithPosition(coordinates, room);
            return createdRoom;
        }
    }
}