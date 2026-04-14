using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Solfège   
{
    class Conductor
    {

        private Texture2D texture;


        public Vector2 Position;


        public Vector2 Size;


        private const float MoveSpeed = 200f;


        private const float DeadZone = 0.2f;

        public Conductor(ContentManager content, GraphicsDevice graphicsDevice)
        {

            texture = content.Load<Texture2D>("sprites/white");
            Size = new Vector2(texture.Width, texture.Height);
        }

        public void Update(GameTime gameTime, GamePadState gp, KeyboardState kb)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;


            Vector2 input = gp.ThumbSticks.Right;


            input.Y = -input.Y;


            if (input.Length() < DeadZone)
            {
                input = Vector2.Zero;
            }

            if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))
            {
                input.Y = -1f;
            }

            if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))
            {
                input.Y = 1f;
            }

            if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))
            {
                input.X = -1f;
            }

            if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right))
            {
                input.X = 1f;
            }

            if (input.Length() > 1f)
            {
                input.Normalize();
            }

            Vector2 velocity = input * MoveSpeed * elapsed;

        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {

            Vector2 screenPos = Position - camera.Position;
            spriteBatch.Draw(texture, screenPos, Color.White);
        }
    }
}
