using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FuncWorks.XNA.XTiled;
using System.Diagnostics;

namespace Mars_Life
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static bool debugMode = false;

        public static Random random = new Random();
        public static GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;

        public static GameState gameState;

        public static SaveGame saveGame;

        public static LoginModule loginModule;

        public static LevelManager levelManager;
        public static CreditManager creditManager;
        public static MainMenu mainMenu;
        public static PauseMenu pauseMenu;
        public static SplashScreen splashScreen;

        public static KeyboardState prevKeyState;
        public static GamePadState prevPadState;

        public static SpriteFont segoeFont;
        public static SpriteFont segoe16Font;
        public static SpriteFont spaceAndAstronomy;

        public static Texture2D blankTexture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 600;

            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Window.Title = "Mars Life";
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            gameState = GameState.None;

            try
            {
                blankTexture = new Texture2D(GraphicsDevice, 1, 1);
                blankTexture.SetData(new Color[] { Color.White });

                //-- Load Fonts
                segoeFont = Content.Load<SpriteFont>("Fonts/SegoeFont");
                segoe16Font = Content.Load<SpriteFont>("Fonts/Segoe16Font");
                spaceAndAstronomy = Content.Load<SpriteFont>("Fonts/Space and Astronomy");

                //-- Load the SaveGameManager
                saveGame = new SaveGame();
                saveGame.ConnectToDatabase();

                //-- Load Independent Textures

                //-- Load the Managers
                MediaPlayer.IsRepeating = true;

                splashScreen = new SplashScreen(Content, GraphicsDevice);
                mainMenu = new MainMenu(Content, GraphicsDevice);
                creditManager = new CreditManager(Content, GraphicsDevice);
                levelManager = new LevelManager(Content, GraphicsDevice, true);
                pauseMenu = new PauseMenu(Content, GraphicsDevice);

                loginModule = new LoginModule(Content, GraphicsDevice);
                MediaPlayer.Stop();
            }
            catch (ContentLoadException e)
            {
                System.Windows.Forms.MessageBox.Show("" + e.Message.ToString() + " | " + e.TargetSite.ToString());
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("" + e.Message.ToString() + " | " + e.TargetSite.ToString());
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            saveGame.DisconnectFromDatabase();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) { } //this.Exit();


            switch (gameState)
            {
                case (GameState.None):
                    gameState = GameState.SplashScreen;
                    //    
                    //-- Do any Pre-load initalizations here
                    //

                    break;

                case (GameState.SplashScreen):
                    splashScreen.Update(gameTime);
                    break;

                case GameState.LogIn:
                    mainMenu.UpdateBackground(gameTime);
                    loginModule.Update(gameTime);
                    break;

                case GameState.MainMenu:
                    mainMenu.Update(gameTime);
                    break;

                case (GameState.PlayGame):
                    levelManager.Update(gameTime, GraphicsDevice);
                    break;

                case (GameState.NewGame):
                    Game1.levelManager.StartGame(false);
                    gameState = GameState.PlayGame;
                    Debug.WriteLine("Switching State: " + gameState);
                    break;

                case (GameState.LoadGame):
                    gameState = GameState.PlayGame;
                    Game1.levelManager.StartGame(true);
                    Debug.WriteLine("Switching State: " + gameState);
                    break;

                case GameState.Paused:
                    pauseMenu.Update(gameTime);
                    break;

                case (GameState.CreditsTransition):
                    creditManager = new CreditManager(Content, GraphicsDevice);
                    gameState = GameState.Credits;
                    Debug.WriteLine("Switching State: " + gameState);
                    break;
                case (GameState.Credits):
                    mainMenu.UpdateBackground(gameTime);
                    creditManager.Update(gameTime);
                    break;

                case (GameState.ExitGame):
                    this.Exit();
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (gameState)
            {
                case GameState.SplashScreen:
                    splashScreen.Draw(gameTime, spriteBatch);
                    break;

                case GameState.LogIn:
                    mainMenu.DrawBackground(gameTime, spriteBatch);
                    loginModule.Draw(spriteBatch);
                    break;

                case GameState.MainMenu:
                    mainMenu.Draw(gameTime, spriteBatch);
                    break;

                case GameState.PlayGame:
                    DrawGame(gameTime);
                    break;

                case GameState.Paused:
                    DrawGame(gameTime);
                    pauseMenu.Draw(gameTime, spriteBatch);
                    break;

                case GameState.Credits:
                    mainMenu.DrawBackground(gameTime, spriteBatch);
                    creditManager.Draw(spriteBatch);
                    break;
            }

            base.Draw(gameTime);
        }

        private void DrawGame(GameTime gameTime)
        {
            levelManager.Draw(gameTime, spriteBatch);

            if (debugMode)
            {
                spriteBatch.Begin();
                RenderText.Draw(spriteBatch, levelManager.CurrentLevel.mapView.ToString(), new Vector2(0, 25), Color.White, segoeFont);
                RenderText.Draw(spriteBatch, levelManager.CurrentLevel.map.Bounds.ToString(), Vector2.Zero, Color.White, segoeFont);
                RenderText.Draw(spriteBatch, levelManager.CurrentLevel.actor.CollisionRectangle.ToString(), new Vector2(0, 50), Color.White, segoeFont);
                // Last Case Draw
                spriteBatch.End();
            }
        }
    }
}
