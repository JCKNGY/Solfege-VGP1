using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Solfège
{
    public class InstrumentSpell
    {
        public double BeatsPerMinute = 60;
        public Texture2D fluteSprite;
        public Texture2D noteSprite;
        public List<Projectile> activeNotes = new List<Projectile>();
        public int perfectHitCount = 0;

        public InstrumentSpell(ContentManager content)
        {
            fluteSprite = content.Load<Texture2D>("sprites/Instruments/Flute");
            noteSprite = content.Load<Texture2D>("sprites/Projectiles/Note");
        }

        public void ProcessHit(BeatRating rating, Vector2 pos, WaveManager wave)
        {
            if (rating == BeatRating.Perfect)
            {
                perfectHitCount++;
            }
            else
            {
                perfectHitCount = 0;
            }

            if (perfectHitCount >= 3)
            {
                perfectHitCount = 0;
                SpawnMusicBlast(pos, wave);
            }
        }

        public void SpawnMusicBlast(Vector2 pos, WaveManager wave)
        {
            wave.shockwaves.Add(new Shockwave(pos, 300f, 1.0f));

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.ToRadians(i * 45);
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                activeNotes.Add(new Projectile(noteSprite, pos, dir));
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = activeNotes.Count - 1; i >= 0; i--)
            {
                activeNotes[i].Update(gameTime);

                if (activeNotes[i].IsActive == false)
                {
                    activeNotes.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch sb, Vector2 playerPos)
        {
            sb.Draw(fluteSprite, playerPos + new Vector2(40, 0), Color.White);

            foreach (var note in activeNotes)
            {
                note.Draw(sb);
            }
        }
    }
}