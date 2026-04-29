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
        Texture2D currenttexture;
        Texture2D heartfulltexture;
        Texture2D heartnearfulltexture;
        Texture2D hearthalfwaytexture;
        Texture2D heartneargonetexture;
        Texture2D heartgonetexture;
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

            heartfulltexture = content.Load<Texture2D>("sprites/Ui/HeartFull");
            heartnearfulltexture = content.Load<Texture2D>("sprites/Ui/Heart75");
            hearthalfwaytexture = content.Load<Texture2D>("sprites/Ui/Heart50");
            heartneargonetexture = content.Load<Texture2D>("sprites/Ui/Heart25");
            heartgonetexture = content.Load<Texture2D>("sprites/Ui/HeartEmpty");
            currenttexture = heartfulltexture;

            font = content.Load<SpriteFont>("Font");
            hearbeat = content.Load<SoundEffect>("HeartBeat");

            ContainerRect = new Rectangle(graphicsDevice.Viewport.Width / 2, 600, 128, 128);
            sourceRect = new Rectangle(0, 0, 64, 64);
            BPM = 104;
            timer = 0;
            ogSize = ContainerRect.Height;
            newSize = ContainerRect.Height + 50;

        }



        public void Update(GameTime gameTime, Conductor player)
        {
            timer += gameTime.ElapsedGameTime.TotalSeconds;

            SPB = 60 / BPM;


            if (player.Health >= 100)
            {
                currenttexture = heartfulltexture;
            }
            else if (player.Health <= 75 && player.Health > 50)
            {
                currenttexture = heartnearfulltexture;
            }
            else if (player.Health <= 50 && player.Health > 25)
            {
                currenttexture = hearthalfwaytexture;
            }
            else if (player.Health <= 25 && player.Health > 0)
            {
                currenttexture = heartneargonetexture;
            }
            else if (player.Health <= 0)
            {
                currenttexture = heartgonetexture;
            }

            if (timer >= SPB && player.IsAlive == true)
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
            spriteBatch.Draw(currenttexture, ContainerRect, sourceRect, Color.White, 0, new Vector2(64 / 2, 64 / 2), SpriteEffects.None, 0);
        }
    }
}
