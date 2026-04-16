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
    class MetronomeSystem
    {
        Texture2D whitetexture;
        Rectangle ContainerRect;
        SpriteFont font;
        double BPM;
        double timer;

        double SPB;
        //seconds per beat
        
        public MetronomeSystem(ContentManager content, GraphicsDevice graphicsDevice, int b)
        {
            whitetexture = content.Load<Texture2D>("sprites/white");
            font = content.Load<SpriteFont>("Font");
            ContainerRect = new Rectangle(200, 500, 150, 150);

            BPM = 120;
            timer = 0;
        }

       

        public void Update(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.TotalSeconds;

            SPB = 60/BPM;
            
            if(timer >= SPB)
            {
                ContainerRect.Height = 180;
                ContainerRect.Width = 180;
                timer -= SPB;
            }
            else
            {
                ContainerRect.Height = 150;
                ContainerRect.Width = 150;
            }

        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(whitetexture, ContainerRect, Color.Yellow);
        }
    }
}
