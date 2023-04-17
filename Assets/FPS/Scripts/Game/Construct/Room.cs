using UnityEngine;

namespace Unity.FPS.Game
{
    public class Room : MonoBehaviour
    {
        public bool passableNorth = true;
        public bool passableSouth = true;
        public bool passableWest = true;
        public bool passableEast = true;
        [Range(0, 1f)]
        public float chanceToSpawn = .15f;
    }
}