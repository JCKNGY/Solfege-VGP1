using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class Projectile
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public bool IsActive = true;
        public Texture2D texture;
        public float speed = 100f;
        public float timer = 0f;

        public Projectile(Texture2D tex, Vector2 pos, Vector2 dir)
        {
            texture = tex;
            Position = pos;
            Velocity = dir * speed;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * dt;
            timer += dt;

            if (timer > 5f)
            {
                IsActive = false;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(texture, Position, Color.White);
        }
    }
}