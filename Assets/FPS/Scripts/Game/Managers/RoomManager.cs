using System.Collections.Generic;
using FPS.Scripts.Game.Construct;
using FPS.Scripts.Game.Managers.NavMesh;
using UnityEngine;
using Random = System.Random;

namespace FPS.Scripts.Game.Managers
{
    public class RoomManager : MonoBehaviour
    {
        public List<Room> Rooms;
        public Transform RoomsParent;

        public Room GetRandomRoom()
        {
            var rnd = new Random();
            var roomToReturn = Rooms[rnd.Next(Rooms.Count)];
            if (roomToReturn.chanceToSpawn * 100 > rnd.Next(100)) return roomToReturn;
            return GetRandomRoom();
        }

        public Room InstantiateRoom(Room room, Vector3 coordinates)
        {
            var createdRoom = Instantiate(room, coordinates, new Quaternion());
            createdRoom.transform.SetParent(RoomsParent);
            var surface = createdRoom.GetComponentInChildren<NavMeshSurface>();
            // surface.BuildNavMesh();
            return createdRoom;
        }
    }
}