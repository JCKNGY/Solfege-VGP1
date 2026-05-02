using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solfège
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private const int ScreenWidth = 1280;
        private const int ScreenHeight = 720;

        private TitleScreen titleScreen;
        private GameScreen currentScreen = GameScreen.Title;

        private Map map;
        private Conductor Conductor;
        private Camera camera;
        private MetronomeSystem metronome;
        private WaveManager waveManager;
        private Texture2D texture;
        private Texture2D pixel;

        SpriteFont font;
        SpriteFont titleFont;
        SpriteFont menuFont;

        private Song titleMusic;
        private Song gameMusic;

        KeyboardState oldKb;

        private static readonly Color ColGold = new Color(201, 168, 76);
        private static readonly Color ColGold2 = new Color(232, 201, 122);
        private static readonly Color ColMuted = new Color(107, 102, 88);
        private static readonly Color ColWhite = new Color(232, 228, 217);
        private static readonly Color ColDeep = new Color(8, 8, 16);

        private string[] pauseLabels = { "Resume", "Settings", "Quit to Title" };
        private int pauseIndex = 0;
        private float pauseGlowTimer = 0f;





        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Font");
            titleFont = font;
            menuFont = font;

            titleScreen = new TitleScreen(GraphicsDevice, titleFont, menuFont, font, Content);

            titleScreen.OnStartGame += StartGame;
            titleScreen.OnNewGame += NewGame;
            titleScreen.OnExitGame += ExitGame;

            map = new Map(Content, GraphicsDevice);
            camera = new Camera(ScreenWidth, ScreenHeight, map.MapWidthPixels, map.MapHeightPixels);
            Conductor = new Conductor(Content, GraphicsDevice);
            metronome = new MetronomeSystem(Content, GraphicsDevice);
            waveManager = new WaveManager(GraphicsDevice);

            Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
            camera.CenterOn(Conductor.Position, Conductor.Size);

            texture = Content.Load<Texture2D>("sprites/Ui/solfegeTitle");

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            titleMusic = Content.Load<Song>("Music/TitleMusic");
            gameMusic = Content.Load<Song>("Music/GameMusic");

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 1f;
            MediaPlayer.Play(titleMusic);
        }
        private void StartGame()
        {
            currentScreen = GameScreen.Playing;
            waveManager.StartNextWave(Conductor.Position);
            ApplyAudioSettings();
            MediaPlayer.Play(gameMusic);
        }
        private void NewGame()
        {
            currentScreen = GameScreen.Playing;
            Conductor.Health = Conductor.MaxHealth;
            Conductor.IsAlive = true;
            Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
            waveManager = new WaveManager(GraphicsDevice);
            metronome.ResetStreak();
            waveManager.StartNextWave(Conductor.Position);
            ApplyAudioSettings();
            MediaPlayer.Play(gameMusic);
        }

        private void ExitGame()
        {
            this.Exit();
        }

        private void ApplyAudioSettings()
        {
            if (titleScreen == null) return;
            MediaPlayer.Volume = titleScreen.MusicVolume * titleScreen.MasterVolume;
            SoundEffect.MasterVolume = titleScreen.SfxVolume * titleScreen.MasterVolume;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            GamePadState gp = GamePad.GetState(PlayerIndex.One);
            KeyboardState kb = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (currentScreen == GameScreen.Title || currentScreen == GameScreen.Settings)
            {
                titleScreen.Update(gameTime);
                currentScreen = titleScreen.CurrentScreen;
            }
            else if (currentScreen == GameScreen.Playing)
            {
                if (!oldKb.IsKeyDown(Keys.Escape) && kb.IsKeyDown(Keys.Escape))
                {
                    currentScreen = GameScreen.Paused;
                    pauseIndex = 0;
                }
                else
                {
                    Conductor.Update(gameTime, gp, kb, map, metronome, waveManager);
                    camera.Update(Conductor.Position, Conductor.Size);
                    metronome.Update(gameTime, Conductor);
                    System.Diagnostics.Debug.WriteLine("Conductor Size: " + Conductor.Size + " Center: " + (Conductor.Position + Conductor.Size / 2f));
                    waveManager.Update(gameTime, Conductor.Position + Conductor.Size / 2f, Conductor);
                    if (!waveManager.WaveActive)
                    {
                        waveManager.StartNextWave(Conductor.Position);
                    }
                        CollisionManager.Update(Conductor, waveManager);
                    if (!Conductor.IsAlive)
                    {
                        currentScreen = GameScreen.GameOver;
                    }
                    }
                }
            else if (currentScreen == GameScreen.Paused)
            {
                pauseGlowTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                UpdatePauseInput(kb);
            }
            else if (currentScreen == GameScreen.GameOver)
            {
                if (!oldKb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.Enter))
                {
                    currentScreen = GameScreen.Title;
                    Conductor.Health = Conductor.MaxHealth;
                    Conductor.IsAlive = true;
                    Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
                    waveManager = new WaveManager(GraphicsDevice);
                    metronome.ResetStreak();
                    MediaPlayer.Play(titleMusic);
                }
            }

            oldKb = kb;
            base.Update(gameTime);
        }

        private void UpdatePauseInput(KeyboardState kb)
        {
            if (!oldKb.IsKeyDown(Keys.Escape) && kb.IsKeyDown(Keys.Escape))
            {
                currentScreen = GameScreen.Playing;
                return;
            }

            bool up = (!oldKb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.Up)) ||(!oldKb.IsKeyDown(Keys.W) && kb.IsKeyDown(Keys.W));
            bool down = (!oldKb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.Down)) ||(!oldKb.IsKeyDown(Keys.S) && kb.IsKeyDown(Keys.S));
            bool enter = (!oldKb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.Enter)) || (!oldKb.IsKeyDown(Keys.Space) && kb.IsKeyDown(Keys.Space));

            if (up)
                pauseIndex = (pauseIndex - 1 + pauseLabels.Length) % pauseLabels.Length;
            if (down)
                pauseIndex = (pauseIndex + 1) % pauseLabels.Length;

            if (enter)
                ActivatePauseMenu();
        }

        private void ActivatePauseMenu()
        {
            if (pauseIndex == 0)
            {
                currentScreen = GameScreen.Playing;
            }
            else if (pauseIndex == 1)
            {
                titleScreen.ForceScreen(GameScreen.Settings);
                currentScreen = GameScreen.Settings;
            }
            else if (pauseIndex == 2)
            {
                titleScreen.ForceScreen(GameScreen.Title);
                currentScreen = GameScreen.Title;
                Conductor.Health = Conductor.MaxHealth;
                Conductor.IsAlive = true;
                Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
                waveManager = new WaveManager(GraphicsDevice);
                MediaPlayer.Play(titleMusic);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (currentScreen == GameScreen.Title || currentScreen == GameScreen.Settings)
            {
                GraphicsDevice.Clear(new Color(10, 10, 15));
                spriteBatch.Begin();
                titleScreen.Draw(spriteBatch, gameTime);
                spriteBatch.End();
            }
            else if (currentScreen == GameScreen.Playing)
            {
                GraphicsDevice.Clear(Color.White);
                spriteBatch.Begin();

                map.Draw(spriteBatch, camera);
                metronome.Draw(spriteBatch);
                waveManager.Draw(spriteBatch, camera);

                spriteBatch.DrawString(font, "Wave: " + waveManager.CurrentWave, new Vector2(10, 35), Color.Black);
                spriteBatch.DrawString(font, "Coins: " + waveManager.CoinsEarned, new Vector2(10, 55), Color.DarkGoldenrod);

                Conductor.Draw(spriteBatch, camera, font);

                spriteBatch.End();
            }
            else if (currentScreen == GameScreen.Paused)
            {
                GraphicsDevice.Clear(Color.White);
                spriteBatch.Begin();

                map.Draw(spriteBatch, camera);
                metronome.Draw(spriteBatch);
                waveManager.Draw(spriteBatch, camera);
                Conductor.Draw(spriteBatch, camera, font);

                DrawPauseOverlay(spriteBatch);

                spriteBatch.End();
            }
            else if (currentScreen == GameScreen.GameOver)
            {
                GraphicsDevice.Clear(new Color(10, 10, 15));
                spriteBatch.Begin();

                string msg = "GAME OVER";
                Vector2 sz = font.MeasureString(msg);
                spriteBatch.DrawString(font, msg,new Vector2(ScreenWidth / 2f - sz.X / 2f, ScreenHeight / 2f - sz.Y / 2f),Color.Red);

                string sub = "Press ENTER to return to title";
                Vector2 subSz = font.MeasureString(sub);
                spriteBatch.DrawString(font, sub,new Vector2(ScreenWidth / 2f - subSz.X / 2f, ScreenHeight / 2f + 40), new Color(107, 102, 88));

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
        private void DrawPauseOverlay(SpriteBatch sb)
        {
            sb.Draw(pixel, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.Black * 0.60f);

            int panelW = 400;
            int panelH = 320;
            int panelX = ScreenWidth / 2 - panelW / 2;
            int panelY = ScreenHeight / 2 - panelH / 2;

            sb.Draw(pixel, new Rectangle(panelX, panelY, panelW, panelH), ColDeep);

            float cx = panelX + panelW / 2f;

            string header = "PAUSED";
            Vector2 headerSz = menuFont.MeasureString(header);
            sb.DrawString(menuFont, header, new Vector2(cx - headerSz.X / 2f, panelY + 36), ColWhite);

            DrawHorizontalRule(sb, new Vector2(cx, panelY + 90), 80, 1f);

            float menuTop = panelY + 110f;
            float lineH = 56f;

            for (int i = 0; i < pauseLabels.Length; i++)
            {
                bool selected = (i == pauseIndex);

                float itemAlpha = 0.45f;
                if (selected)
                {
                    itemAlpha = 1f;
                }


                Color labelColor = ColMuted;
                if (selected)
                {
                    labelColor = ColWhite;
                }


                float scale = 1.0f;
                if (selected)
                {
                    scale = 1.05f;
                }


                string label = pauseLabels[i].ToUpper();
                Vector2 labelSz = menuFont.MeasureString(label) * scale;
                float x = cx - labelSz.X / 2f;
                float y = menuTop + i * lineH;

                if (selected)
                {
                    float glow = 0.6f + 0.4f * (float)Math.Sin(pauseGlowTimer * 3f);
                    float dotX = x - 22;
                    float dotY = y + labelSz.Y / 2f;
                    int dotSize = 7;
                    sb.Draw(pixel, new Rectangle((int)(dotX - dotSize / 2), (int)(dotY - dotSize / 2), dotSize, dotSize), ColGold * glow);
                }

                sb.DrawString(menuFont, label, new Vector2(x, y), labelColor * itemAlpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            if (font != null)
            {
                string hint = "ESC to resume     ENTER to select";
                Vector2 hSz = font.MeasureString(hint);
                sb.DrawString(font, hint, new Vector2(cx - hSz.X / 2f, panelY + panelH - 30), ColMuted * 0.55f);
            }
        }

        private void DrawHorizontalRule(SpriteBatch sb, Vector2 center, int halfWidth, float alpha)
        {
            int thickness = 1;
            sb.Draw(pixel, new Rectangle((int)(center.X - halfWidth), (int)center.Y, halfWidth * 2, thickness), ColGold * 0.35f * alpha);
            int ds = 5;
            sb.Draw(pixel,new Rectangle((int)center.X - ds / 2, (int)center.Y - ds / 2 + thickness / 2, ds, ds), ColGold * 0.6f * alpha);
        }
    }
}
