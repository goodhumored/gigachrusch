using UnityEngine;

namespace FPS.Scripts.Game.Construct
{
    public class Wall : MonoBehaviour
    {
        public enum WallType
        {
            DoorWall,
            SolidWall,
            WindowWall
        }

        public WallType type;
        
        [Range(0, 1f)]
        public float chanceToSpawn = .15f;
    }
}