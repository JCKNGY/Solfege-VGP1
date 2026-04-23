using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;



namespace Solfège
{

    public enum GameScreen
    {
        Title,
        Settings,
        Playing,
        GameOver
    }


    internal class TitleNote
    {
        public Vector2 Position;
        public float Speed;
        public float Alpha;
        public float Size;
        public char Glyph;
        public float Phase;
        public float DriftX;
    }


    public class TitleScreen
    {

        public GameScreen CurrentScreen { get; private set; } = GameScreen.Title;


        public float MusicVolume { get; private set; } = 0.70f;
        public float SfxVolume { get; private set; } = 0.80f;
        public float MasterVolume { get; private set; } = 1.00f;


        public bool ScreenShake { get; private set; } = true;
        public bool MetronomePulse { get; private set; } = true;


        public event Action OnStartGame;
        public event Action OnExitGame;


        private SpriteFont titleFont;
        private SpriteFont menuFont;
        private SpriteFont uiFont;
        private Texture2D pixel;

        private GraphicsDevice gd;
        private int SW, SH;

        // Menu
        private string[] menuLabels = { "New Performance", "Continue", "Settings", "Exit" };
        private int menuIndex = 0;
        private KeyboardState prevKb = default;


        private float fadeIn = 0f;
        private const float FadeSpeed = 1.2f;


        private float glowTimer = 0f;


        private int settingsFocus = -1;

        private List<TitleNote> notes = new List<TitleNote>();
        private Random rng = new Random();

        private static readonly char[] NoteChars = { 'Q', 'S', 'B', 'B' };
        private const int NoteCount = 30;

        private float time = 0f;


        private static readonly Color ColGold = new Color(201, 168, 76);
        private static readonly Color ColGold2 = new Color(232, 201, 122);
        private static readonly Color ColMuted = new Color(107, 102, 88);
        private static readonly Color ColWhite = new Color(232, 228, 217);
        private static readonly Color ColInk = new Color(10, 10, 15);
        private static readonly Color ColDeep = new Color(8, 8, 16);


        



        public TitleScreen(GraphicsDevice graphicsDevice, SpriteFont titleFont, SpriteFont menuFont, SpriteFont uiFont, ContentManager content)
        {
            

            gd = graphicsDevice;
            this.titleFont = titleFont;
            this.menuFont = menuFont;
            this.uiFont = uiFont;
            SW = gd.Viewport.Width;
            SH = gd.Viewport.Height;

            pixel = new Texture2D(gd, 1, 1);
            pixel.SetData(new[] { Color.White });



            SpawnNotes();
        }

        private void SpawnNotes()
        {
            for (int i = 0; i < NoteCount; i++)
                notes.Add(MakeNote((float)rng.NextDouble() * SH));
        }

        private TitleNote MakeNote(float startY)
        {
            return new TitleNote {
                Position = new Vector2((float)rng.NextDouble() * SW, startY),
                Speed = 14f + (float)rng.NextDouble() * 22f,
                Alpha = 0.03f + (float)rng.NextDouble() * 0.09f,
                Size = 0.4f + (float)rng.NextDouble() * 0.8f,
                Glyph = NoteChars[rng.Next(NoteChars.Length)],
                Phase = (float)rng.NextDouble() * MathHelper.TwoPi,
                DriftX = ((float)rng.NextDouble() - 0.5f) * 12f,
            };
        }


        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kb = Keyboard.GetState();
            time += dt;
            glowTimer += dt;
            fadeIn = Math.Min(1f, fadeIn + dt * FadeSpeed);


            foreach (var n in notes)
            {
                n.Position.Y -= n.Speed * dt;
                n.Position.X += (float)Math.Sin(time * 0.6f + n.Phase) * n.DriftX * dt;
                if (n.Position.Y < -40)
                {
                    var fresh = MakeNote(SH + 20);
                    n.Position = fresh.Position;
                    n.Speed = fresh.Speed;
                    n.Alpha = fresh.Alpha;
                    n.Size = fresh.Size;
                    n.Glyph = fresh.Glyph;
                    n.Phase = fresh.Phase;
                }
            }


            if (CurrentScreen == GameScreen.Title)
                UpdateTitleInput(kb);
            else if (CurrentScreen == GameScreen.Settings)
                UpdateSettingsInput(kb);

            prevKb = kb;
        }

        private void UpdateTitleInput(KeyboardState kb)
        {
            bool up = JustPressed(kb, Keys.Up) || JustPressed(kb, Keys.W);
            bool down = JustPressed(kb, Keys.Down) || JustPressed(kb, Keys.S);
            bool enter = JustPressed(kb, Keys.Enter) || JustPressed(kb, Keys.Space);

            if (up) menuIndex = (menuIndex - 1 + menuLabels.Length) % menuLabels.Length;
            if (down) menuIndex = (menuIndex + 1) % menuLabels.Length;

            if (enter) ActivateMenu();
        }

        private void ActivateMenu()
        {
            switch (menuIndex)
            {
                case 0: // New Performance
                    CurrentScreen = GameScreen.Playing;
                    OnStartGame?.Invoke();
                    break;
                case 1: // Continue  (same for now — extend with save system later)
                    CurrentScreen = GameScreen.Playing;
                    OnStartGame?.Invoke();
                    break;
                case 2: // Settings
                    CurrentScreen = GameScreen.Settings;
                    settingsFocus = -1;
                    break;
                case 3: // Exit
                    OnExitGame?.Invoke();
                    break;
            }
        }


        private void UpdateSettingsInput(KeyboardState kb)
        {

            if (JustPressed(kb, Keys.Down) || JustPressed(kb, Keys.S))
                settingsFocus = Math.Min(settingsFocus + 1, 4); // 0-2 sliders, 3-4 toggles
            if (JustPressed(kb, Keys.Up) || JustPressed(kb, Keys.W))
                settingsFocus = Math.Max(settingsFocus - 1, 0);


            float nudge = JustPressed(kb, Keys.Left) || JustPressed(kb, Keys.A) ? -0.05f :
                          JustPressed(kb, Keys.Right) || JustPressed(kb, Keys.D) ? 0.05f : 0f;

            if (nudge != 0f)
            {
                switch (settingsFocus)
                {
                    case 0: MusicVolume = MathHelper.Clamp(MusicVolume + nudge, 0f, 1f); ApplyVolumes(); break;
                    case 1: SfxVolume = MathHelper.Clamp(SfxVolume + nudge, 0f, 1f); break;
                    case 2: MasterVolume = MathHelper.Clamp(MasterVolume + nudge, 0f, 1f); ApplyVolumes(); break;
                    case 3: ScreenShake = !ScreenShake; break;
                    case 4: MetronomePulse = !MetronomePulse; break;
                }
            }


            if (JustPressed(kb, Keys.Enter) || JustPressed(kb, Keys.Space))
            {
                if (settingsFocus == 3) ScreenShake = !ScreenShake;
                if (settingsFocus == 4) MetronomePulse = !MetronomePulse;
            }


            if (JustPressed(kb, Keys.Escape))
                CurrentScreen = GameScreen.Title;

            
        }

        private void ApplyVolumes()
        {

            MediaPlayer.Volume = MusicVolume * MasterVolume;
            SoundEffect.MasterVolume = SfxVolume * MasterVolume;
        }


        public void Draw(SpriteBatch sb, GameTime gameTime)
        {

            sb.Draw(pixel, new Rectangle(0, 0, SW, SH), ColInk);


            if (CurrentScreen == GameScreen.Title)
            {

                DrawTitle(sb);
            }

            else if (CurrentScreen == GameScreen.Settings)
            {
                //DrawSettings(sb);
            }
            
        }



        private void DrawTitle(SpriteBatch sb)
        {
            float f = fadeIn;


            string title = "SOLFEGE";
            Vector2 titleSize = titleFont.MeasureString(title);
            Vector2 titlePos = new Vector2(SW / 2f - titleSize.X / 2f, SH * 0.28f);


            sb.DrawString(titleFont, title, titlePos + new Vector2(0, 4), ColGold * 0.15f * f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            

            //sb.DrawString(titleFont, title, titlePos, ColWhite * f);


            


            DrawHorizontalRule(sb, new Vector2(SW / 2f, SH * 0.50f), 120, f);

            float menuTop = SH * 0.54f;
            float lineH = 52f;

            for (int i = 0; i < menuLabels.Length; i++)
            {
                bool selected = (i == menuIndex);
                float itemAlpha = selected ? 1f : 0.45f;
                itemAlpha *= f;

                Color labelColor = selected ? ColWhite : ColMuted;
                float scale = selected ? 1.05f : 1.0f;

                string label = menuLabels[i].ToUpper();
                Vector2 labelSz = menuFont.MeasureString(label) * scale;
                float x = SW / 2f - labelSz.X / 2f;
                float y = menuTop + i * lineH;

                if (selected)
                {
                    float glow = 0.6f + 0.4f * (float)Math.Sin(glowTimer * 3f);
                    float dotX = x - 22;
                    float dotY = y + labelSz.Y / 2f;
                    int dotSize = 7;
                    sb.Draw(pixel,new Rectangle((int)(dotX - dotSize / 2), (int)(dotY - dotSize / 2), dotSize, dotSize),ColGold * glow * f);
                }

                sb.DrawString(menuFont, label,new Vector2(x, y),labelColor * itemAlpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }


            if (uiFont != null)
            {
                string hint = "W / S  or  UP / DOWN  to navigate     ENTER to select";
                Vector2 hSz = uiFont.MeasureString(hint);
                sb.DrawString(uiFont, hint, new Vector2(SW / 2f - hSz.X / 2f, SH - 32), ColMuted * 0.4f * f);
            }
        }


        private void DrawSettings(SpriteBatch sb)
        {

            sb.Draw(pixel, new Rectangle(0, 0, SW, SH), Color.Black * 0.65f);


            int panelW = 480, panelH = 460;
            int panelX = SW / 2 - panelW / 2;
            int panelY = SH / 2 - panelH / 2;


            sb.Draw(pixel, new Rectangle(panelX, panelY, panelW, panelH), ColDeep);


            DrawBorder(sb, new Rectangle(panelX, panelY, panelW, panelH), ColGold * 0.35f, 1);


            DrawCorner(sb, new Vector2(panelX + 12, panelY + 12), true, true);
            DrawCorner(sb, new Vector2(panelX + panelW - 12, panelY + 12), false, true);
            DrawCorner(sb, new Vector2(panelX + 12, panelY + panelH - 12), true, false);
            DrawCorner(sb, new Vector2(panelX + panelW - 12, panelY + panelH - 12), false, false);

            float cx = panelX + panelW / 2f;
            float cy = panelY + 40;


            if (titleFont != null)
            {
                string st = "SETTINGS";
                Vector2 stSz = menuFont.MeasureString(st);
                sb.DrawString(menuFont, st, new Vector2(cx - stSz.X / 2f, cy), ColWhite);
            }
            cy += 55;

            DrawSectionLabel(sb, "AUDIO", panelX + 30, (int)cy);
            cy += 28;

            DrawSliderRow(sb, "Music", MusicVolume, 0, panelX, panelY, panelW, (int)cy, settingsFocus == 0); cy += 46;
            DrawSliderRow(sb, "SFX", SfxVolume, 1, panelX, panelY, panelW, (int)cy, settingsFocus == 1); cy += 46;
            DrawSliderRow(sb, "Master", MasterVolume, 2, panelX, panelY, panelW, (int)cy, settingsFocus == 2); cy += 54;


            DrawSectionLabel(sb, "DISPLAY", panelX + 30, (int)cy);
            cy += 28;

            DrawToggleRow(sb, "Screen Shake", ScreenShake, panelX, panelW, (int)cy, settingsFocus == 3); cy += 38;
            DrawToggleRow(sb, "Metronome Pulse", MetronomePulse, panelX, panelW, (int)cy, settingsFocus == 4); cy += 46;


            if (uiFont != null)
            {
                string hint = "ESC  to go back     ENTER  to toggle";
                Vector2 hSz = uiFont.MeasureString(hint);
                sb.DrawString(uiFont, hint,
                              new Vector2(cx - hSz.X / 2f, panelY + panelH - 32),
                              ColMuted * 0.55f);
            }
        }


        private void DrawSliderRow(SpriteBatch sb, string label, float value, int focusId,
                                   int panelX, int panelY, int panelW, int y, bool focused)
        {
            int labelX = panelX + 30;
            int sliderX = panelX + 130;
            int sliderW = panelW - 200;
            int valX = panelX + panelW - 55;
            Color rowColor = focused ? ColWhite : ColMuted;


            if (uiFont != null)
                sb.DrawString(uiFont, label.ToUpper(), new Vector2(labelX, y), rowColor);


            sb.Draw(pixel, new Rectangle(sliderX, y + 8, sliderW, 2), Color.White * 0.1f);


            int fillW = (int)(sliderW * value);
            sb.Draw(pixel, new Rectangle(sliderX, y + 8, fillW, 2),
                    focused ? ColGold2 : ColGold * 0.7f);


            int thumbX = sliderX + fillW - 5;
            int thumbY = y + 2;
            float glow = focused ? 0.7f + 0.3f * (float)Math.Sin(glowTimer * 4f) : 0.5f;
            sb.Draw(pixel, new Rectangle(thumbX, thumbY, 10, 12),
                    focused ? ColGold2 * glow : ColGold * 0.5f);


            if (uiFont != null)
                sb.DrawString(uiFont, ((int)(value * 100)).ToString(),
                              new Vector2(valX, y), rowColor);
        }

        private void DrawToggleRow(SpriteBatch sb, string label, bool value,
                                   int panelX, int panelW, int y, bool focused)
        {
            Color rowColor = focused ? ColWhite : ColMuted;

            if (uiFont != null)
                sb.DrawString(uiFont, label.ToUpper(), new Vector2(panelX + 30, y), rowColor);


            int tx = panelX + panelW - 70;
            sb.Draw(pixel, new Rectangle(tx, y, 44, 20), value ? ColGold * 0.3f : Color.White * 0.08f);
            DrawBorder(sb, new Rectangle(tx, y, 44, 20),
                       focused ? ColGold * 0.8f : ColGold * 0.25f, 1);


            int knobX = value ? tx + 44 - 18 : tx + 2;
            sb.Draw(pixel, new Rectangle(knobX, y + 3, 14, 14),
                    value ? ColGold2 : ColMuted * 0.6f);
        }

        private void DrawSectionLabel(SpriteBatch sb, string text, int x, int y)
        {
            if (uiFont == null) return;
            sb.DrawString(uiFont, text, new Vector2(x, y), ColGold * 0.75f);

            int lineX = x + (int)uiFont.MeasureString(text).X + 10;
            sb.Draw(pixel, new Rectangle(lineX, y + 8, 300, 1), ColGold * 0.2f);
        }

        private void DrawHorizontalRule(SpriteBatch sb, Vector2 center, int halfWidth, float alpha)
        {
            int thickness = 1;
            sb.Draw(pixel,
                    new Rectangle((int)(center.X - halfWidth), (int)center.Y, halfWidth * 2, thickness),
                    ColGold * 0.35f * alpha);

            int ds = 5;
            sb.Draw(pixel,
                    new Rectangle((int)center.X - ds / 2, (int)center.Y - ds / 2 + thickness / 2, ds, ds),
                    ColGold * 0.6f * alpha);
        }

        private void DrawBorder(SpriteBatch sb, Rectangle r, Color c, int thickness)
        {
            sb.Draw(pixel, new Rectangle(r.X, r.Y, r.Width, thickness), c); // top
            sb.Draw(pixel, new Rectangle(r.X, r.Bottom, r.Width, thickness), c); // bottom
            sb.Draw(pixel, new Rectangle(r.X, r.Y, thickness, r.Height), c); // left
            sb.Draw(pixel, new Rectangle(r.Right, r.Y, thickness, r.Height), c); // right
        }

        private void DrawCorner(SpriteBatch sb, Vector2 pos, bool left, bool top)
        {
            int len = 14, t = 1;
            int sx = left ? (int)pos.X : (int)pos.X - len;
            int sy = top ? (int)pos.Y : (int)pos.Y - len;
            Color c = ColGold * 0.4f;
            sb.Draw(pixel, new Rectangle(sx, sy, len, t), c); // horizontal
            sb.Draw(pixel, new Rectangle(sx, sy, t, len), c); // vertical
        }


        private bool JustPressed(KeyboardState kb, Keys key)
            => kb.IsKeyDown(key) && !prevKb.IsKeyDown(key);
    }
}
