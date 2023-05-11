using FPS.Scripts.Game.Construct;

namespace FPS.Scripts.Game.Managers.Common
{
    public struct RoomConstraints
    {
        public WallType NorthWallType;
        public WallType EastWallType;
        public WallType SouthWallType;
        public WallType WestWallType;

        public void SetWallTypeBySide(Side side, WallType wallType)
        {
            switch (side)
            {
                case Side.East:
                    EastWallType = wallType;
                    break;
                case Side.West:
                    WestWallType = wallType;
                    break;
                case Side.North:
                    NorthWallType = wallType;
                    break;
                default:
                    SouthWallType = wallType;
                    break;
            }
        }
    }
}