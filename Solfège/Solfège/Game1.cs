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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
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

        SpriteFont font;
        SpriteFont titleFont;
        SpriteFont menuFont;


        KeyboardState oldKb;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


            font = Content.Load<SpriteFont>("Font");
            titleFont = font;
            menuFont = font;

            titleScreen = new TitleScreen(GraphicsDevice, titleFont, menuFont, font, Content);

            titleScreen.OnStartGame += StartGame;
            titleScreen.OnExitGame += () => this.Exit();

            map = new Map(Content, GraphicsDevice);
            camera = new Camera(ScreenWidth, ScreenHeight, map.MapWidthPixels, map.MapHeightPixels);
            Conductor = new Conductor(Content, GraphicsDevice);
            metronome = new MetronomeSystem(Content, GraphicsDevice, 240);
            waveManager = new WaveManager(GraphicsDevice);

            Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
            camera.CenterOn(Conductor.Position, Conductor.Size);

            texture = Content.Load<Texture2D>("sprites/Ui/solfegeTitle");



            // TODO: use this.Content to load your game content here
        }


        private void StartGame()
        {
            currentScreen = GameScreen.Playing;
            waveManager.StartNextWave(Conductor.Position);


            ApplyAudioSettings();
        }


        private void ApplyAudioSettings()
        {
            if (titleScreen == null) return;
            MediaPlayer.Volume = titleScreen.MusicVolume * titleScreen.MasterVolume;
            SoundEffect.MasterVolume = titleScreen.SfxVolume * titleScreen.MasterVolume;
        }



        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            GamePadState gp = GamePad.GetState(PlayerIndex.One);
            KeyboardState kb = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || !oldKb.IsKeyDown(Keys.Escape) && kb.IsKeyDown(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here

            if (currentScreen == GameScreen.Title || currentScreen == GameScreen.Settings)
            {
                titleScreen.Update(gameTime);

                currentScreen = titleScreen.CurrentScreen;
            }
            else if (currentScreen == GameScreen.Playing)
            {
                Conductor.Update(gameTime, gp, kb, map);
                camera.Update(Conductor.Position, Conductor.Size);
                metronome.Update(gameTime);

                waveManager.Update(gameTime, Conductor.Position, Conductor);
                if (!waveManager.WaveActive)
                    waveManager.StartNextWave(Conductor.Position);

                CollisionManager.Update(Conductor, waveManager);

                if (!Conductor.IsAlive)
                    currentScreen = GameScreen.GameOver;
            }

            oldKb = kb;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here

            if (currentScreen == GameScreen.Title)
            {
                spriteBatch.Begin();

                spriteBatch.Draw(texture, new Rectangle(ScreenWidth / 2, ScreenHeight / 2, 10, 10), Color.White); 

                spriteBatch.End();
            }
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
            else if (currentScreen == GameScreen.GameOver)
            {
                GraphicsDevice.Clear(new Color(10, 10, 15));
                spriteBatch.Begin();

                string msg = "GAME OVER";
                Vector2 sz = font.MeasureString(msg);
                spriteBatch.DrawString(font, msg,new Vector2(ScreenWidth / 2f - sz.X / 2f, ScreenHeight / 2f - sz.Y / 2f),Color.Red);

                string sub = "Press ENTER to return to title";
                Vector2 subSz = font.MeasureString(sub);
                spriteBatch.DrawString(font, sub,new Vector2(ScreenWidth / 2f - subSz.X / 2f, ScreenHeight / 2f + 40),new Color(107, 102, 88));

                spriteBatch.End();


                KeyboardState kb = Keyboard.GetState();
                if (!oldKb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.Enter))
                {
                    currentScreen = GameScreen.Title;

                    Conductor.Health = Conductor.MaxHealth;
                    Conductor.IsAlive = true;
                    Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
                    waveManager = new WaveManager(GraphicsDevice);
                }
                oldKb = Keyboard.GetState();
            }

            base.Draw(gameTime);
        }
    }
}
