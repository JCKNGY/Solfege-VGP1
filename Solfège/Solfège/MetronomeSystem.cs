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
        Rectangle sourceRect;
        SpriteFont font;
        double BPM;
        double timer;

        SoundEffect hearbeat;
        int ogSize;
        int newSize;

        double SPB;
        //seconds per beat
        
        public MetronomeSystem(ContentManager content, GraphicsDevice graphicsDevice, int b)
        {
            whitetexture = content.Load<Texture2D>("sprites/Ui/HeartFull");
            font = content.Load<SpriteFont>("Font");
            hearbeat = content.Load<SoundEffect>("HeartBeat");

            ContainerRect = new Rectangle(100, 150, 128, 128);
            sourceRect = new Rectangle(0, 0, 64, 64);
            BPM = 60;
            timer = 0;
            ogSize = ContainerRect.Height;
            newSize = ContainerRect.Height + 50;
            
        }

       

        public void Update(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.TotalSeconds;

            SPB = 60/BPM;
            
            if(timer >= SPB)
            {
                ContainerRect.Height = newSize;
                ContainerRect.Width = newSize;
                hearbeat.Play();
                timer -= SPB;
                
            }
            else
            {
                ContainerRect.Height = ogSize;
                ContainerRect.Width = ogSize;
            }

        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(whitetexture, ContainerRect, sourceRect, Color.White, 0, new Vector2(64/2, 64/2), SpriteEffects.None, 0);
        }
    }
}
