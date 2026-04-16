using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solfège
{
    public class Conductor
    {
        private Texture2D texture;

        public Vector2 Position;

        public Vector2 Size;

        private const float MoveSpeed = 200f;

        private const float DeadZone = 0.2f;

        public Conductor(ContentManager content, GraphicsDevice graphicsDevice)
        {
            texture = content.Load<Texture2D>("sprites/ConductorFront");
            Size = new Vector2(texture.Width, texture.Height);
        }

        public void Update(GameTime gameTime, GamePadState gp, KeyboardState kb, Map map)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 input = gp.ThumbSticks.Right;

            input.Y = -input.Y;

            if (input.Length() < DeadZone)
            {
                input = Vector2.Zero;
            }

            if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))
                input.Y = -1f;

            if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))
                input.Y = 1f;

            if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))
                input.X = -1f;

            if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right))
                input.X = 1f;

            if (input.Length() > 1f)
                input.Normalize();

            Vector2 velocity = input * MoveSpeed * elapsed;

            Vector2 newPos = new Vector2(Position.X + velocity.X, Position.Y);
            if (!CollidesWithWall(newPos, map))
            {
                Position.X = newPos.X;
            }

            newPos = new Vector2(Position.X, Position.Y + velocity.Y);
            if (!CollidesWithWall(newPos, map))
            {
                Position.Y = newPos.Y;
            }
        }

        private bool CollidesWithWall(Vector2 pos, Map map)
        {
            int left = (int)(pos.X / Map.TileWidth);
            int right = (int)((pos.X + Size.X - 1) / Map.TileWidth);
            int top = (int)(pos.Y / Map.TileHeight);
            int bottom = (int)((pos.Y + Size.Y - 1) / Map.TileHeight);

            return map.IsWall(left, top) || map.IsWall(right, top) ||
                   map.IsWall(left, bottom) || map.IsWall(right, bottom);
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            Vector2 screenPos = Position - camera.Position;
            spriteBatch.Draw(texture, screenPos, Color.White);
        }
    }
}
