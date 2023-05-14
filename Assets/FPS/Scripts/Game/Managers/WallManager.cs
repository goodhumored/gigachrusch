using System.Collections.Generic;
using System.Linq;
using FPS.Scripts.Game.Construct;
using UnityEngine;
using Random = System.Random;

namespace FPS.Scripts.Game.Managers
{
    public class WallManager : MonoBehaviour
    {
        public static int WallN;
        public List<Wall> Walls;
        public Transform RoomsParent;

        public Wall GetRandomWall(WallType[] wallTypes)
        {
            var rnd = new Random();
            var candidates = Walls.FindAll((wall) => wallTypes.Contains(wall.type));
            if (candidates.Count == 0)
            {
                return Walls[0];
            }
            while (true)
            {
                var wallToReturn = candidates[rnd.Next(candidates.Count)];
                if (wallToReturn.chanceToSpawn * 100 > rnd.Next(100))
                    return wallToReturn;
            }
        }

        public Wall InstantiateWall(Wall wall, Vector3 coordinates, bool side)
        {
            var createdWall = Instantiate(wall, coordinates, Quaternion.Euler(0, side ? 90 : 0, 0));
            createdWall.transform.SetParent(RoomsParent);
            createdWall.name += WallN++;
            return createdWall;
        }
    }
}