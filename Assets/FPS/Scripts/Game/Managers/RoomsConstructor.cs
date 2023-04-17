using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Unity.FPS.Game
{
    public class RoomsConstructor : MonoBehaviour
    {
        public List<Room> Rooms;
        public List<Wall> Walls;
        public float RoomWidth = 15f;
        public float RoomHeight = 15f;
        public int RoomsCountX = 5;
        public int RoomsCountZ = 5;
        public int FloorsCount = 5;
        public Transform RoomsParent;

        private void Start()
        {
            for (int i = 0; i < FloorsCount; i++)
            {
                SetupFloor(i * RoomHeight);
            }
        }

        private void SetupFloor(float y)
        {
            var x = -RoomsCountX * RoomWidth / 2;
            float z;
            for (var ix = 0; ix < RoomsCountX; ix++)
            {
                z = -RoomsCountZ * RoomWidth / 2;
                for (var iz = 0; iz < RoomsCountZ; iz++)
                {
                    InstantiateWall(GetRandomWall(), new Vector3(x - RoomWidth / 2, y, z), false).transform.parent =
                        RoomsParent;
                    if (ix == RoomsCountX - 1)
                    {
                        InstantiateWall(GetRandomWall(), new Vector3(x + RoomWidth / 2, y, z), false).transform.parent =
                            RoomsParent;
                    }
                    InstantiateWall(GetRandomWall(), new Vector3(x, y, z - RoomWidth / 2), true).transform.parent =
                        RoomsParent;
                    InstantiateRoom(GetRandomRoom(), new Vector3(x, y, z)).transform.parent =
                        RoomsParent;
                    z += RoomWidth;
                }

                InstantiateWall(GetRandomWall(), new Vector3(x, y, z - RoomWidth / 2), true).transform.parent =
                    RoomsParent;
                x += RoomWidth;
            }
        }

        private Wall GetRandomWall()
        {
            var rnd = new Random();
            var wallToReturn = Walls[rnd.Next(Walls.Count)];
            if (wallToReturn.chanceToSpawn * 100 > rnd.Next(100)) return wallToReturn;
            return GetRandomWall();
        }

        private Room GetRandomRoom()
        {
            var rnd = new Random();
            var roomToReturn = Rooms[rnd.Next(Rooms.Count)];
            if (roomToReturn.chanceToSpawn * 100 > rnd.Next(100)) return roomToReturn;
            return GetRandomRoom();
        }

        private Room InstantiateRoom(Room room, Vector3 coordinates)
        {
            return Instantiate(room, coordinates, new Quaternion());
        }

        private Wall InstantiateWall(Wall wall, Vector3 coordinates, bool horizontal)
        {
            return Instantiate(wall, coordinates, Quaternion.Euler(0, horizontal ? 90 : 0, 0));
        }
    }
}