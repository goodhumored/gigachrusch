using System.Collections.Generic;
using FPS.Scripts.Game.Construct;
using UnityEngine;

namespace FPS.Scripts.Game.Repositories
{
    public class WallRepository
    {
        private WallRepository() {}
            
        private Dictionary<Vector3, Wall> _walls = new Dictionary<Vector3, Wall>();
        
        private static WallRepository _instance;

        public static WallRepository GetInstance()
        {
            if (_instance == null) _instance = new WallRepository();
            return _instance;
        }

        public bool Save(Wall wall)
        {
            if (_walls.TryGetValue(wall.transform.position, out _))
            {
                _walls[wall.transform.position] = wall;
                return true;
            }
            else if (_walls.TryAdd(wall.transform.position, wall))
            {
                return true;
            }

            return false;
        }

        public Wall FindByPosition(Vector3 position)
        {
            if (_walls.TryGetValue(position, out var wall))
            {
                return wall;
            }
            else
            {
                return null;
            }
        }
    }
}