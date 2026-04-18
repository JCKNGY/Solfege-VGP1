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

        public int MaxHealth = 100;
        public int Health = 100;
        public bool IsAlive = true;

        public Conductor(ContentManager content, GraphicsDevice graphicsDevice)
        {
            texture = content.Load<Texture2D>("sprites/ConductorFront");
            Size = new Vector2(texture.Width, texture.Height);
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                Health = 0;
                IsAlive = false;
            }
        }

        public void Update(GameTime gameTime, GamePadState gp, KeyboardState kb, Map map)
        {
            if (!IsAlive) return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 input = Vector2.Zero;

            if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up)) input.Y = -1f;
            if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down)) input.Y = 1f;
            if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left)) input.X = -1f;
            if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) input.X = 1f;

            if (input.Length() > 1f)
                input.Normalize();

            Position += input * MoveSpeed * elapsed;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, SpriteFont font)
        {
            Vector2 screenPos = Position - camera.Position;
            spriteBatch.Draw(texture, screenPos, Color.White);

            // Health bar above player
            int barWidth = (int)Size.X;
            int filledWidth = (int)(barWidth * ((float)Health / MaxHealth));
            int barY = (int)screenPos.Y - 12;

            // Draw bar background and fill using a simple colored rectangle trick
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            spriteBatch.Draw(pixel, new Rectangle((int)screenPos.X, barY, barWidth, 8), Color.DarkRed);
            spriteBatch.Draw(pixel, new Rectangle((int)screenPos.X, barY, filledWidth, 8), Color.LimeGreen);

            // HP text
            spriteBatch.DrawString(font, "HP: " + Health + "/" + MaxHealth, new Vector2(10, 10), Color.Red);
        }
    }
}