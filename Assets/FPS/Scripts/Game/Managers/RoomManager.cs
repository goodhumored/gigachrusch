using System.Collections.Generic;
using FPS.Scripts.Game.Construct;
using FPS.Scripts.Game.Managers.Common;
using FPS.Scripts.Game.Repositories;
using UnityEngine;
using Random = System.Random;

namespace FPS.Scripts.Game.Managers
{
    public class RoomManager : MonoBehaviour
    {
        public static int RoomN;
        public List<Room> availableRoomsToSpawn;
        public Transform roomsParent;
        private RoomRepository _roomRepository = RoomRepository.GetInstance();

        public Room GetRandomRoom(RoomConstraints roomConstraints)
        {
            var rnd = new Random();
            var candidates = availableRoomsToSpawn;
            if (candidates.Count == 0)
            {
                return availableRoomsToSpawn[0];
            }
            while (true)
            {
                var roomToReturn = candidates[rnd.Next(candidates.Count)];
                if (roomToReturn.chanceToSpawn * 100 > rnd.Next(100))
                    return roomToReturn;
            }
        }

        public Room InstantiateRoom(Room room, Vector3 coordinates)
        {
            var createdRoom = Instantiate(room, coordinates, new Quaternion());
            createdRoom.transform.SetParent(roomsParent);
            createdRoom.name += RoomN++;
            _roomRepository.Save(createdRoom);
            return createdRoom;
        }
    }
}