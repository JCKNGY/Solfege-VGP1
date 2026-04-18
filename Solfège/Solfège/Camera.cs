using Microsoft.Xna.Framework;

namespace Solfège
{
    public class Camera
    {
        public Vector2 Position;

        private readonly int screenWidth;
        private readonly int screenHeight;

        // Map dimensions kept for constructor compatibility but clamp is removed
        public Camera(int screenW, int screenH, int mapW, int mapH)
        {
            screenWidth = screenW;
            screenHeight = screenH;
        }

        public void CenterOn(Vector2 playerPos, Vector2 playerSize)
        {
            Position.X = playerPos.X + playerSize.X / 2f - screenWidth / 2f;
            Position.Y = playerPos.Y + playerSize.Y / 2f - screenHeight / 2f;
            // No clamp — infinite world, camera follows freely
        }

        public void Update(Vector2 playerPos, Vector2 playerSize)
        {
            CenterOn(playerPos, playerSize);
        }
    }
}