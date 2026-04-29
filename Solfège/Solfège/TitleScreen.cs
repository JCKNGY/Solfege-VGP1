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
        Paused,
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

        private int musicPct = 100;
        private int sfxPct = 100;
        private int masterPct = 100;

        public float MusicVolume { get { return musicPct / 100f; } }
        public float SfxVolume { get { return sfxPct / 100f; } }
        public float MasterVolume { get { return masterPct / 100f; } }

        public bool ScreenShake { get; private set; } = true;
        public bool MetronomePulse { get; private set; } = true;

        public event Action OnStartGame;
        public event Action OnExitGame;

        private SpriteFont titleFont;
        private SpriteFont menuFont;
        private SpriteFont uiFont;
        private Texture2D pixel;
        private Texture2D logoTexture;

        private GraphicsDevice gd;
        private int SW, SH;

        private string[] menuLabels = { "New Performance", "Continue", "Settings", "Exit" };
        private int menuIndex = 0;
        private KeyboardState prevKb = default;

        private float holdTimer = 0f;
        private float holdDelay = 0.4f;
        private float holdRepeat = 0.08f;
        private bool holdingLeft = false;
        private bool holdingRight = false;

        private MouseState prevMouse = default;

        private float fadeIn = 0f;
        private const float FadeSpeed = 1.2f;

        private float glowTimer = 0f;

        private int settingsFocus = -1;

        private List<TitleNote> notes = new List<TitleNote>();
        private Random rng = new Random();
        private Rectangle[] sliderRects = new Rectangle[3];

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

            logoTexture = content.Load<Texture2D>("sprites/Ui/solfegeTitle");

            SpawnNotes();
        }

        private void SpawnNotes()
        {
            for (int i = 0; i < NoteCount; i++)
                notes.Add(MakeNote((float)rng.NextDouble() * SH));
        }

        private TitleNote MakeNote(float startY)
        {
            TitleNote n = new TitleNote();
            n.Position = new Vector2((float)rng.NextDouble() * SW, startY);
            n.Speed = 14f + (float)rng.NextDouble() * 22f;
            n.Alpha = 0.03f + (float)rng.NextDouble() * 0.09f;
            n.Size = 0.4f + (float)rng.NextDouble() * 0.8f;
            n.Glyph = NoteChars[rng.Next(NoteChars.Length)];
            n.Phase = (float)rng.NextDouble() * MathHelper.TwoPi;
            n.DriftX = ((float)rng.NextDouble() - 0.5f) * 12f;
            return n;
        }

        public void ForceScreen(GameScreen screen)
        {
            CurrentScreen = screen;
            if (screen == GameScreen.Title)
                fadeIn = 0f;
            if (screen == GameScreen.Settings)
                settingsFocus = 0;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kb = Keyboard.GetState();
            time += dt;
            glowTimer += dt;
            fadeIn = Math.Min(1f, fadeIn + dt * FadeSpeed);

            foreach (TitleNote n in notes)
            {
                n.Position.Y -= n.Speed * dt;
                n.Position.X += (float)Math.Sin(time * 0.6f + n.Phase) * n.DriftX * dt;
                if (n.Position.Y < -40)
                {
                    TitleNote fresh = MakeNote(SH + 20);
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
            if (menuIndex == 0)
            {
                CurrentScreen = GameScreen.Playing;
                if (OnStartGame != null) OnStartGame();
            }
            else if (menuIndex == 1)
            {
                CurrentScreen = GameScreen.Playing;
                if (OnStartGame != null) OnStartGame();
            }
            else if (menuIndex == 2)
            {
                CurrentScreen = GameScreen.Settings;
                settingsFocus = -1;
            }
            else if (menuIndex == 3)
            {
                if (OnExitGame != null) OnExitGame();
            }
        }

        private void UpdateSettingsInput(KeyboardState kb)
        {
            float dt = (float)(1.0 / 60.0);

            if (JustPressed(kb, Keys.Down) || JustPressed(kb, Keys.S))
                settingsFocus = Math.Min(settingsFocus + 1, 4);
            if (JustPressed(kb, Keys.Up) || JustPressed(kb, Keys.W))
                settingsFocus = Math.Max(settingsFocus - 1, 0);

            bool leftDown = kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A);
            bool rightDown = kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D);

            bool stepLeft = false;
            bool stepRight = false;

            if (leftDown || rightDown)
            {
                bool dirChanged = (leftDown && !holdingLeft) || (rightDown && !holdingRight);
                if (dirChanged)
                {
                    holdTimer = 0f;
                    holdingLeft = leftDown;
                    holdingRight = rightDown;
                    stepLeft = leftDown;
                    stepRight = rightDown;
                }
                else
                {
                    holdTimer += dt;
                    float threshold = holdTimer < holdDelay ? holdDelay : holdRepeat;
                    if (holdTimer >= threshold)
                    {
                        holdTimer -= holdRepeat;
                        stepLeft = leftDown;
                        stepRight = rightDown;
                    }
                }
            }
            else
            {
                holdingLeft = false;
                holdingRight = false;
                holdTimer = 0f;
            }

            if (stepLeft || stepRight)
            {
                int delta = stepRight ? 5 : -5;
                if (settingsFocus == 0)
                {
                    musicPct = Math.Max(0, Math.Min(100, musicPct + delta));
                    ApplyVolumes();
                }
                else if (settingsFocus == 1)
                {
                    sfxPct = Math.Max(0, Math.Min(100, sfxPct + delta));
                }
                else if (settingsFocus == 2)
                {
                    masterPct = Math.Max(0, Math.Min(100, masterPct + delta));
                    ApplyVolumes();
                }
                else if (settingsFocus == 3)
                {
                    ScreenShake = !ScreenShake;
                }
                else if (settingsFocus == 4)
                {
                    MetronomePulse = !MetronomePulse;
                }
            }

            MouseState mouse = Mouse.GetState();

            bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released;

            if (mouseClicked)
            {
                int panelW = 480;
                int panelX = SW / 2 - panelW / 2;
                int sliderX = panelX + 130;
                int sliderW = panelW - 200;

                for (int i = 0; i < 3; i++)
                {
                    if (sliderRects[i] != Rectangle.Empty)
                    {
                        Rectangle hit = new Rectangle(sliderRects[i].X, sliderRects[i].Y - 10, sliderRects[i].Width, sliderRects[i].Height + 20);
                        if (hit.Contains(mouse.X, mouse.Y))
                        {
                            float ratio = MathHelper.Clamp((float)(mouse.X - sliderX) / sliderW, 0f, 1f);
                            int pct = (int)Math.Round(ratio * 20f) * 5;
                            pct = Math.Max(0, Math.Min(100, pct));
                            settingsFocus = i;
                            if (i == 0) { musicPct = pct; ApplyVolumes(); }
                            else if (i == 1) { sfxPct = pct; }
                            else if (i == 2) { masterPct = pct; ApplyVolumes(); }
                        }
                    }
                }
            }

            prevMouse = mouse;

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

            DrawFloatingNotes(sb);

            if (CurrentScreen == GameScreen.Title)
                DrawTitle(sb);
            else if (CurrentScreen == GameScreen.Settings)
                DrawSettings(sb);
        }

        private void DrawFloatingNotes(SpriteBatch sb)
        {
            if (titleFont == null) return;
            foreach (TitleNote n in notes)
            {
                string glyph = n.Glyph.ToString();
                Vector2 sz = titleFont.MeasureString(glyph) * n.Size;
                Vector2 pos = new Vector2(n.Position.X - sz.X / 2f, n.Position.Y - sz.Y / 2f);
                sb.DrawString(titleFont, glyph, pos, ColGold * n.Alpha, 0f, Vector2.Zero, n.Size, SpriteEffects.None, 0f);
            }
        }

        private void DrawTitle(SpriteBatch sb)
        {
            float f = fadeIn;

            if (logoTexture != null)
            {
                int logoW = 420;
                int logoH = (int)(logoTexture.Height * (420f / logoTexture.Width));
                int logoX = SW / 2 - logoW / 2;
                int logoY = (int)(SH * 0.10f);
                sb.Draw(logoTexture, new Rectangle(logoX, logoY, logoW, logoH), Color.White * f);
            }

            DrawHorizontalRule(sb, new Vector2(SW / 2f, SH * 0.50f), 120, f);

            float menuTop = SH * 0.54f;
            float lineH = 52f;

            for (int i = 0; i < menuLabels.Length; i++)
            {
                bool selected = (i == menuIndex);

                float itemAlpha = 0.45f;
                if (selected) itemAlpha = 1f;
                itemAlpha *= f;

                Color labelColor = ColMuted;
                if (selected) labelColor = ColWhite;

                float scale = 1.0f;
                if (selected) scale = 1.05f;

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
                    sb.Draw(pixel, new Rectangle((int)(dotX - dotSize / 2), (int)(dotY - dotSize / 2), dotSize, dotSize), ColGold * glow * f);
                }

                sb.DrawString(menuFont, label, new Vector2(x, y), labelColor * itemAlpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
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

            if (menuFont != null)
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
                sb.DrawString(uiFont, hint, new Vector2(cx - hSz.X / 2f, panelY + panelH - 32), ColMuted * 0.55f);
            }
        }

        private void DrawSliderRow(SpriteBatch sb, string label, float value, int focusId,
                                   int panelX, int panelY, int panelW, int y, bool focused)
        {
            int labelX = panelX + 30;
            int sliderX = panelX + 130;
            int sliderW = panelW - 200;
            int valX = panelX + panelW - 55;

            if (focusId >= 0 && focusId < sliderRects.Length)
                sliderRects[focusId] = new Rectangle(sliderX, y, sliderW, 12);

            Color rowColor = ColMuted;
            if (focused) rowColor = ColWhite;

            if (uiFont != null)
                sb.DrawString(uiFont, label.ToUpper(), new Vector2(labelX, y), rowColor);

            sb.Draw(pixel, new Rectangle(sliderX, y + 8, sliderW, 2), Color.White * 0.1f);

            int fillW = (int)(sliderW * value);

            Color fillColor = ColGold * 0.7f;
            if (focused) fillColor = ColGold2;
            sb.Draw(pixel, new Rectangle(sliderX, y + 8, fillW, 2), fillColor);

            int thumbX = sliderX + fillW - 5;
            int thumbY = y + 2;
            float glow = 0.5f;
            if (focused) glow = 0.7f + 0.3f * (float)Math.Sin(glowTimer * 4f);

            Color thumbColor = ColGold * 0.5f;
            if (focused) thumbColor = ColGold2 * glow;
            sb.Draw(pixel, new Rectangle(thumbX, thumbY, 10, 12), thumbColor);

            if (uiFont != null)
                sb.DrawString(uiFont, ((int)(value * 100)).ToString(), new Vector2(valX, y), rowColor);
        }

        private void DrawToggleRow(SpriteBatch sb, string label, bool value,
                                   int panelX, int panelW, int y, bool focused)
        {
            Color rowColor = ColMuted;
            if (focused) rowColor = ColWhite;

            if (uiFont != null)
                sb.DrawString(uiFont, label.ToUpper(), new Vector2(panelX + 30, y), rowColor);

            int tx = panelX + panelW - 70;

            Color trackColor = Color.White * 0.08f;
            if (value) trackColor = ColGold * 0.3f;
            sb.Draw(pixel, new Rectangle(tx, y, 44, 20), trackColor);

            Color borderColor = ColGold * 0.25f;
            if (focused) borderColor = ColGold * 0.8f;
            DrawBorder(sb, new Rectangle(tx, y, 44, 20), borderColor, 1);

            int knobX = tx + 2;
            if (value) knobX = tx + 44 - 18;

            Color knobColor = ColMuted * 0.6f;
            if (value) knobColor = ColGold2;
            sb.Draw(pixel, new Rectangle(knobX, y + 3, 14, 14), knobColor);
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
            sb.Draw(pixel, new Rectangle(r.X, r.Y, r.Width, thickness), c);
            sb.Draw(pixel, new Rectangle(r.X, r.Bottom, r.Width, thickness), c);
            sb.Draw(pixel, new Rectangle(r.X, r.Y, thickness, r.Height), c);
            sb.Draw(pixel, new Rectangle(r.Right, r.Y, thickness, r.Height), c);
        }

        private void DrawCorner(SpriteBatch sb, Vector2 pos, bool left, bool top)
        {
            int len = 14, t = 1;
            int sx = (int)pos.X;
            if (!left) sx = (int)pos.X - len;
            int sy = (int)pos.Y;
            if (!top) sy = (int)pos.Y - len;
            Color c = ColGold * 0.4f;
            sb.Draw(pixel, new Rectangle(sx, sy, len, t), c);
            sb.Draw(pixel, new Rectangle(sx, sy, t, len), c);
        }

        private bool JustPressed(KeyboardState kb, Keys key)
        {
            return kb.IsKeyDown(key) && !prevKb.IsKeyDown(key);
        }
    }
}
