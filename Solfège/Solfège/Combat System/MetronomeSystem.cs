using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public enum BeatRating
    {
        None,
        Perfect,
        Good,
        Miss
    }

    public class MetronomeSystem
    {
        public Texture2D currentTexture;
        public Texture2D heartFullTexture;
        public Texture2D heartNearFullTexture;
        public Texture2D heartHalfwayTexture;
        public Texture2D heartNearGoneTexture;
        public Texture2D heartGoneTexture;

        public SpriteFont font;
        public SoundEffect heartbeatSfx;

        public double BPM;
        public double SPB;
        public double beatTimer;

        public double PerfectWindow = 0.150;
        public double GoodWindow = 0.250;

        public int Streak = 0;
        public BeatRating LastRating = BeatRating.None;
        public float ratingTimer = 0f;
        public float RatingDuration = 1.5f;

        public Texture2D pixel;
        public Rectangle barBg;
        public float beatPulse;

        public Color PerfectColor = Color.Gold;
        public Color GoodColor = Color.LimeGreen;
        public Color MissColor = Color.Red;

        public int screenW;
        public int screenH;

        public MetronomeSystem(ContentManager content, GraphicsDevice gd, int bpm)
        {
            screenW = gd.Viewport.Width;
            screenH = gd.Viewport.Height;

            BPM = bpm;
            SPB = 60.0 / BPM;

            heartFullTexture = content.Load<Texture2D>("sprites/Ui/HeartFull");
            heartNearFullTexture = content.Load<Texture2D>("sprites/Ui/Heart75");
            heartHalfwayTexture = content.Load<Texture2D>("sprites/Ui/Heart50");
            heartNearGoneTexture = content.Load<Texture2D>("sprites/Ui/Heart25");
            heartGoneTexture = content.Load<Texture2D>("sprites/Ui/HeartEmpty");
            currentTexture = heartFullTexture;

            font = content.Load<SpriteFont>("Font");
            heartbeatSfx = content.Load<SoundEffect>("HeartBeat");

            barBg = new Rectangle(screenW / 2 - 250, screenH - 150, 500, 30);

            pixel = new Texture2D(gd, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        public BeatRating RegisterAction()
        {
            double dist = Math.Min(beatTimer, SPB - beatTimer);
            BeatRating rating;

            if (dist <= PerfectWindow)
            {
                rating = BeatRating.Perfect;
            }
            else if (dist <= GoodWindow)
            {
                rating = BeatRating.Good;
            }
            else
            {
                rating = BeatRating.Miss;
            }

            LastRating = rating;
            ratingTimer = RatingDuration;

            if (rating == BeatRating.Miss)
            {
                Streak = 0;
            }
            else
            {
                Streak++;
            }

            return rating;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            beatTimer += elapsed;

            if (beatTimer >= SPB)
            {
                beatTimer -= SPB;
                beatPulse = 1.0f;
                heartbeatSfx.Play();
            }

            if (ratingTimer > 0)
            {
                ratingTimer -= elapsed;
            }

            beatPulse = MathHelper.Lerp(beatPulse, 0, elapsed * 5f);
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(pixel, barBg, Color.Black * 0.5f);

            float phase = (float)(beatTimer / SPB);
            int cursorX = barBg.X + (int)(phase * barBg.Width);

            sb.Draw(pixel, new Rectangle(cursorX - 2, barBg.Y - 5, 4, barBg.Height + 10), Color.White);

            if (Streak > 0)
            {
                string sText = "STREAK: " + Streak;
                sb.DrawString(font, sText, new Vector2(barBg.X, barBg.Y - 40), Color.White);
            }

            if (ratingTimer > 0)
            {
                string rText = LastRating.ToString().ToUpper();
                Color rCol = LastRating == BeatRating.Perfect ? PerfectColor : (LastRating == BeatRating.Good ? GoodColor : MissColor);

                sb.DrawString(font, rText, new Vector2(barBg.X + barBg.Width - 100, barBg.Y - 40), rCol * (ratingTimer / RatingDuration));
            }
        }
    }
}