using System;
using System.Collections;
using System.Collections.Generic;
using FPS.Scripts.Game.Construct;
using FPS.Scripts.Game.Managers.Common;
using UnityEngine;

namespace FPS.Scripts.Game.Managers
{
    public enum Side
    {
        North,
        East,
        West,
        South
    }
    
    public class BuildingManager : MonoBehaviour
    {
        private RoomManager _roomManager;
        private WallManager _wallManager;
        private GameObject _player;
        private Queue<Vector3> _buildingQueue = new Queue<Vector3>();
        private Room _startRoom;

        private void Start()
        {
            _player = GameObject.FindWithTag("Player");
            _roomManager = GetComponent<RoomManager>();
            _wallManager = GetComponent<WallManager>();
            StartCoroutine(BuildRandomRoom(_player.transform.position + Vector3.down));
        }

        private void Update()
        {
            if (_buildingQueue.TryDequeue(out var position))
            {
                StartCoroutine(BuildRandomRoom(position));
            }
        }

        void EnqueueRoom(Vector3 position)
        {
            if (!_buildingQueue.Contains(position)) _buildingQueue.Enqueue(position);
        }

        public void CheckAndBuildNeighbourRooms(Room originRoom)
        {
            var sides = GetSides();
            foreach (var side in sides)
            {
                var room = originRoom.GetNeighbourRoomBySide(side);
                if (!room)
                {
                    EnqueueRoom(originRoom.GetNeighbourRoomPositionBySide(side));
                }
            }
        }

        private IEnumerator BuildRandomRoom(Vector3 position)
        {
            if (_roomManager.GetRoomByPosition(position)) yield break;
            Debug.Log($"Building {position} from the queue.");
            var sides = GetSides();
            var roomConstraints = new RoomConstraints();
            yield return null;
            foreach (var side in sides)
            {
                var roomPosition = position + GetVectorBySide(side) * RoomConstants.RoomLength;
                var roomByPosition = _roomManager.GetRoomByPosition(roomPosition);
                // Debug.Log($"(Before) Room at {roomPosition.ToString()} found: {(roomByPosition ? roomByPosition.name : "null")}");
                if (roomByPosition)
                {
                    var counterSide = GetCounterSide(side);
                    var wall = _wallManager.GetRandomWall(roomByPosition.GetWallTypesBySide(counterSide));
                    _wallManager.InstantiateWall(wall, roomByPosition.GetWallPositionBySide(counterSide),
                        IsSideward(side));
                    roomByPosition.SetWallBySide(wall, counterSide);
                    roomConstraints.SetWallTypeBySide(side, wall.type);
                }
                yield return null;
            }

            var roomToSet = _roomManager.GetRandomRoom(roomConstraints);
            roomToSet = _roomManager.InstantiateRoom(roomToSet, position);
            yield return null;

            foreach (var side in sides)
            {
                var counterSide = GetCounterSide(side);
                var neighbourRoomPosition = roomToSet.GetNeighbourRoomPositionBySide(side);
                var neighbourRoom = _roomManager.GetRoomByPosition(neighbourRoomPosition);
                // Debug.Log($"(After) Room at {neighbourRoomPosition.ToString()} found: {(neighbourRoom ? neighbourRoom.name : "null")}");
                if (neighbourRoom)
                {
                    neighbourRoom.SetNeighbourRoomBySide(roomToSet, counterSide);
                    roomToSet.SetNeighbourRoomBySide(neighbourRoom, side);
                    roomToSet.SetWallBySide(neighbourRoom.GetWallBySide(counterSide), side);
                }
                yield return null;
            }

            if (!_startRoom) _startRoom = roomToSet;
            _startRoom.surface.BuildNavMesh();
        }

        public static Vector3 GetVectorBySide(Side side)
        {
            switch (side)
            {
                case Side.North: return Vector3.forward;
                case Side.East: return Vector3.right;
                case Side.South: return Vector3.back;
                default: return Vector3.left;
            }
        }

        public static Side GetCounterSide(Side side)
        {
            switch (side)
            {
                case Side.North: return Side.South;
                case Side.East: return Side.West;
                case Side.South: return Side.North;
                default: return Side.East;
            }
        }

        public static bool IsSideward(Side side)
        {
            return side == Side.North || side == Side.South;
        }

        public static Side[] GetSides()
        {
            return new Side[] { Side.East, Side.North, Side.South, Side.West };
        }
    }
}