using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class Enemy
    {
        public Vector2 Position;
        public Vector2 Size;
        public bool IsAlive = true;

        private int Health = 50;
        private int MaxHealth = 50;
        private int Damage = 10;
        private float MoveSpeed = 60f;

        private Texture2D texture;

        private float damageCooldown = 0f;
        private const float DamageCooldownMax = 1.0f;

        public Enemy(Vector2 spawnPosition, GraphicsDevice graphicsDevice)
        {
            Position = spawnPosition;

            // Red placeholder rectangle for the zombie
            texture = new Texture2D(graphicsDevice, 32, 48);
            Color[] pixels = new Color[32 * 48];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.Red;
            texture.SetData(pixels);

            Size = new Vector2(32, 48);
        }

        public void Update(GameTime gameTime, Vector2 playerPosition)
        {
            if (!IsAlive) return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (damageCooldown > 0f)
                damageCooldown -= elapsed;

            // Walk straight toward the player
            Vector2 direction = playerPosition - Position;
            if (direction.Length() > 1f)
            {
                direction.Normalize();
                Position += direction * MoveSpeed * elapsed;
            }
        }

        // Returns how much damage was dealt to player this frame (0 if none)
        public int CheckContactDamage(Vector2 playerPos, Vector2 playerSize)
        {
            if (!IsAlive || damageCooldown > 0f) return 0;

            Rectangle enemyRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            Rectangle playerRect = new Rectangle((int)playerPos.X, (int)playerPos.Y, (int)playerSize.X, (int)playerSize.Y);

            if (enemyRect.Intersects(playerRect))
            {
                damageCooldown = DamageCooldownMax;
                return Damage;
            }
            return 0;
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health <= 0)
                IsAlive = false;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (!IsAlive) return;

            Vector2 screenPos = Position - camera.Position;
            spriteBatch.Draw(texture, screenPos, Color.White);

            // Health bar above enemy
            int barWidth = (int)Size.X;
            int filledWidth = (int)(barWidth * ((float)Health / MaxHealth));
            int barY = (int)screenPos.Y - 8;

            spriteBatch.Draw(texture, new Rectangle((int)screenPos.X, barY, barWidth, 5), Color.DarkRed);
            spriteBatch.Draw(texture, new Rectangle((int)screenPos.X, barY, filledWidth, 5), Color.LimeGreen);
        }
    }
}