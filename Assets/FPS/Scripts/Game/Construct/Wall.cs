using UnityEngine;

namespace FPS.Scripts.Game.Construct
{
    public enum WallType
    {
        DoorWall,
        SolidWall,
        WindowWall
    }
    
    public class Wall : MonoBehaviour
    {

        public WallType type;
        
        [Range(0, 1f)]
        public float chanceToSpawn = .15f;

        public static WallType[] GetAllWallTypes()
        {
            return new[] { WallType.DoorWall, WallType.SolidWall, WallType.WindowWall };
        }
    }
}