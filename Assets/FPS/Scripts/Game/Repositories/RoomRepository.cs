using System.Collections.Generic;
using FPS.Scripts.Game.Construct;
using UnityEngine;

namespace FPS.Scripts.Game.Repositories
{
    public class RoomRepository
    {
        private RoomRepository() {}
        private Dictionary<Vector3, Room> _rooms = new Dictionary<Vector3, Room>();
        private static RoomRepository _instance;

        public static RoomRepository GetInstance()
        {
            if (_instance == null) _instance = new RoomRepository();
            return _instance;
        }

        public bool Save(Room room)
        {
            if (_rooms.TryGetValue(room.transform.position, out _))
            {
                _rooms[room.transform.position] = room;
                return true;
            }
            else if (_rooms.TryAdd(room.transform.position, room))
            {
                return true;
            }

            return false;
        }

        public Room FindByPosition(Vector3 position)
        {
            if (_rooms.TryGetValue(position, out var room))
            {
                return room;
            }
            else
            {
                return null;
            }
        }
    }
}