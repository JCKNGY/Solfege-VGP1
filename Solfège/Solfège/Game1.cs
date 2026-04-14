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

        private Map map;
        private Conductor Conductor;
        private Camera camera;

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
            map = new Map(Content, GraphicsDevice);
            Conductor = new Conductor(Content, GraphicsDevice);
            camera = new Camera(ScreenWidth, ScreenHeight, map.MapWidthPixels, map.MapHeightPixels);

            Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);

            camera.CenterOn(Conductor.Position, Conductor.Size);

            // TODO: use this.Content to load your game content here
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


            Conductor.Update(gameTime, gp, kb, map);
            camera.Update(Conductor.Position, Conductor.Size);
            oldKb = kb;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            Conductor.Draw(spriteBatch, camera);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
