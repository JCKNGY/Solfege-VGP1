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
        List<Beat> beatlist;

        String text;
        double BPM;
        double timer;

        double SPB;
        //seconds per beat
        
        public MetronomeSystem(ContentManager content, GraphicsDevice graphicsDevice)
        {
            whitetexture = content.Load<Texture2D>("sprites/white");
            font = content.Load<SpriteFont>("Font");
            ContainerRect = new Rectangle(0, 500, 800, 150);

            beatlist = new List<Beat>();
            text = " ";
            beatlist.Add(new Beat(content));
            timer = 0;
        }

       

        public void Update(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.TotalSeconds;

            SPB = BPM / 60;
            
            if(timer >= SPB)
            {
                text = "click";
                timer -= SPB;
            }
            else
            {
                text = " ";
            }

        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, text, new Vector2(200, 200), Color.White);
            spriteBatch.Draw(whitetexture, ContainerRect, Color.Yellow);
        }
    }
}
