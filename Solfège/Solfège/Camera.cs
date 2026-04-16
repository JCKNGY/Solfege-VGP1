using Microsoft.Xna.Framework;

namespace Solfège
{
    public class Camera
    {
        public Vector2 Position;

        private readonly int screenWidth;
        private readonly int screenHeight;
        private readonly int mapWidth;
        private readonly int mapHeight;

        public Camera(int screenW, int screenH, int mapW, int mapH)
        {
            screenWidth = screenW;
            screenHeight = screenH;
            mapWidth = mapW;
            mapHeight = mapH;
        }

        public void CenterOn(Vector2 playerPos, Vector2 playerSize)
        {

            Position.X = playerPos.X + playerSize.X / 2f - screenWidth / 2f;
            Position.Y = playerPos.Y + playerSize.Y / 2f - screenHeight / 2f;
            ClampToMap();
        }

        public void Update(Vector2 playerPos, Vector2 playerSize)
        {
            CenterOn(playerPos, playerSize);
        }

        private void ClampToMap()
        {
            if (Position.X < 0)
            {
                Position.X = 0;
            }

            if (Position.Y < 0)
            {
                Position.Y = 0;
            }

            if (Position.X > mapWidth - screenWidth)
            {
                Position.X = mapWidth - screenWidth;
            }

            if (Position.Y > mapHeight - screenHeight)
            {
                Position.Y = mapHeight - screenHeight;
            }
        }
    }
}