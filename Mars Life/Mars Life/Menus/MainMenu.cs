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
using System.Diagnostics;

namespace Mars_Life
{
    public class MainMenu:Menu
    {
        public Song backgroundMusic;

        GraphicsDevice device;
        ContentManager content;

        GameState newGameState;

        Texture2D space;
        Texture2D meteor1;
        Texture2D meteor2;
        Texture2D meteor3;
        Model marsModel;

        Color gameNameColor = Color.GhostWhite;

        RenderText gameName;

        float aspectRatio;
        float modelRotation;

        float meteorSpawnCtr;
        float miniMeteorSpawnCtr;
        const float METEOR_SPAWN = 10000f;
        const float MINI_METEOR_SPAWN = 1000f;
        float redFloat;

        Vector3 modelPosition;  // Set the position of the model in world space, and set the rotation.
        Vector3 cameraPosition = new Vector3(0.0f, 50.0f, 5000.0f); // Set the position of the camera in world space, for our view matrix.

        List<Meteor> meteors;

        public MainMenu(ContentManager Content, GraphicsDevice _Device)
            :base(_Device, Game1.segoeFont, new List<string> {})
        {
            content = Content;
            device = _Device;
            aspectRatio = _Device.Viewport.AspectRatio;

            newGameState = GameState.None;

            gameName = new RenderText("Empires of the IV\n          Mars Life", new Vector2(_Device.Viewport.Width / 2.9f, 25f), gameNameColor, Game1.spaceAndAstronomy);

            marsModel = Content.Load<Model>("Images/Titlescreen/Planet/Mars");
            space = Content.Load<Texture2D>("Images/Titlescreen/Space");
            meteor1 = Content.Load<Texture2D>("Images/Titlescreen/Meteor 1");
            meteor2 = Content.Load<Texture2D>("Images/Titlescreen/Meteor 2");
            meteor3 = Content.Load<Texture2D>("Images/Titlescreen/Meteor 3");

            AddOption("New Game");
            AddOption("Load Game");
            AddOption("Credits");
            AddOption("Exit Game");

            modelPosition = Vector3.Zero;
            modelPosition.X = 2f;
            modelPosition.Y = -8f;

            redFloat = 0f;

            backgroundMusic = Content.Load<Song>("Audio/Music/Dead Piano Island");
            MediaPlayer.Play(backgroundMusic);

            modelRotation = 0.0f;
            meteorSpawnCtr = 0f;
            miniMeteorSpawnCtr = 0f;

            fadeFloat = 1f;
            textFade = 0f;
            fadeStatus = FadeStatus.FadingIn;
            textFadeStatus = FadeStatus.FadingIn;

            meteors = new List<Meteor>();
        }

        public void UpdateBackground(GameTime gameTime)
        {
            if (MediaPlayer.Queue[0].Name != backgroundMusic.Name) { Debug.WriteLine("Changing Music to Main Menu Music"); MediaPlayer.Play(backgroundMusic); }

            // Update Elapsed Game Times
            modelRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.01f;// *MathHelper.ToRadians(0.01f);
            meteorSpawnCtr += (float)gameTime.ElapsedGameTime.Milliseconds;
            miniMeteorSpawnCtr += (float)gameTime.ElapsedGameTime.Milliseconds;
            //Spawn Scene Objects
            if (meteorSpawnCtr >= METEOR_SPAWN) { meteorSpawnCtr = 0f; SpawnMeteor(); }
            if (miniMeteorSpawnCtr >= MINI_METEOR_SPAWN) { miniMeteorSpawnCtr = 0f; SpawnMiniMeteor(); }

            //--Move Scene Objects
            foreach (Meteor m in meteors) { m.Update(gameTime); }
        }

        public override void Update(GameTime gameTime)
        {
            UpdateBackground(gameTime);

            //if (Game1.loginModule.loginModuleHasFocus) { Game1.loginModule.Update(gameTime); }
            //-- Update Planet Color
            if (fadeStatus == FadeStatus.FadingOut)
            {
                redFloat += (1f);// * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
            }

            base.Update(gameTime);
            
            if (!textReadyToTransition || !readyToTransition)
            {
                if (optionSelected)
                {
                    switch (ItemSelected)
                    {
                        case 1:
                            newGameState = GameState.NewGame;
                            //Game1.levelManager.StartGame(false);
                            break;
                        case 2:
                            newGameState = GameState.LoadGame;
                            //Game1.levelManager.StartGame(true);
                            break;
                        case 3:
                            newGameState = GameState.CreditsTransition;
                            //Game1.levelManager.StartGame(true);
                            break;
                        case 4:
                            newGameState = GameState.ExitGame;
                            break;
                    }
                    fadeStatus = FadeStatus.FadingOut;
                    textFadeStatus = FadeStatus.FadingOut;
                }
                //Game1.prevKeyState = keyState;
            }
            else
            {
                Debug.WriteLine("Switching State: " + newGameState);
                Game1.gameState = newGameState;
                newGameState = new GameState();

                fadeStatus = FadeStatus.FadingIn;
                textFadeStatus = FadeStatus.FadingIn;

                textReadyToTransition = false;
                readyToTransition = false;
                optionSelected = false;
                redFloat = 0f;
            }

            gameName.Colour = gameNameColor * textFade;
        }

        public void DrawBackground(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(space, Vector2.Zero, Color.White);
               
            foreach (Meteor m in meteors) { m.Draw(gameTime, spriteBatch); }
            spriteBatch.End();

            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[marsModel.Bones.Count];
            marsModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in marsModel.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    //effect.LightingEnabled = false;// EnableDefaultLighting();
                    effect.DiffuseColor = new Vector3(1, 1, 1);
                    if (fadeStatus == FadeStatus.FadingOut)
                    {
                        effect.EnableDefaultLighting();
                        effect.DiffuseColor = new Vector3((fadeFloat + redFloat) * 0.1f, 0.5f, 0.5f);
                        //effect.AmbientLightColor = new Vector3(redFloat, 0, 0);  
                    }
                    effect.PreferPerPixelLighting = true;
                    effect.World = transforms[mesh.ParentBone.Index]
                        * Matrix.CreateRotationY(MathHelper.ToRadians(modelRotation))
                        * Matrix.CreateRotationZ(MathHelper.ToRadians(5f))
                        * Matrix.CreateTranslation(modelPosition);
                    //* Matrix.CreateScale(20f);
                    effect.View = Matrix.CreateLookAt(cameraPosition,
                        Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(1.6f), aspectRatio, // 45
                        1.0f, 10000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(gameTime, spriteBatch);

            spriteBatch.Begin();
            spriteBatch.Draw(Game1.splashScreen.killerrinStudiosLogo, new Vector2(device.Viewport.Width - (0.3f * Game1.splashScreen.killerrinStudiosLogo.Width * 1.2f), device.Viewport.Height - (0.3f * Game1.splashScreen.killerrinStudiosLogo.Height * 2f)), null, Color.White, 0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0f);
            base.Draw(gameTime, spriteBatch);
            gameName.Draw(spriteBatch);

            //Game1.loginModule.loginModuleHasFocus = true;
            //if (Game1.loginModule.loginModuleHasFocus) { Game1.loginModule.Draw(spriteBatch); }
            spriteBatch.End();

        }



        private void SpawnMeteor()
        {
            for (int i = 0; i < Game1.random.Next(1, 3); ++i)
            {
                Texture2D tex = meteor1;
                switch (Game1.random.Next(0, 3))
                {
                    case 0:
                        tex = meteor1;
                        break;
                    case 1:
                        tex = meteor2;
                        break;
                    case 2:
                        tex = meteor3;
                        break;
                }

                switch (Game1.random.Next(0, 2))
                {
                    case 0:
                        meteors.Add(new Meteor(tex, new Vector2(Game1.random.Next(-25, (int)Game1.levelManager.CurrentLevel.mapView.Width), -25), new Vector2(50f, 50f), true));
                        break;
                    case 1:
                        meteors.Add(new Meteor(tex, new Vector2(-25, Game1.random.Next(-25, (int)Game1.levelManager.CurrentLevel.mapView.Height)), new Vector2(50f, 50f), true));
                        break;
                    case 2:
                        meteors.Add(new Meteor(tex, new Vector2(Game1.random.Next(-25, (int)Game1.levelManager.CurrentLevel.mapView.Width), Game1.levelManager.CurrentLevel.mapView.Height - 25), new Vector2(-50f, -50f), false));
                        break;
                }
            }
        }
        private void SpawnMiniMeteor()
        {

            for (int i = 0; i < Game1.random.Next(2, 5); ++i)
            {
                float scale = 0.005f;
                switch (Game1.random.Next(0, 4))
                {
                    case 0:
                        scale = 0.005f;
                        break;
                    case 1:
                        scale = 0.004f;
                        break;
                    case 2:
                        scale = 0.003f;
                        break;
                    case 3:
                        scale = 0.002f;
                        break;
                }

                Texture2D tex = meteor1;
                switch (Game1.random.Next(0, 3))
                {
                    case 0:
                        tex = meteor1;
                        break;
                    case 1:
                        tex = meteor2;
                        break;
                    case 2:
                        tex = meteor3;
                        break;
                }

                switch (Game1.random.Next(0, 3))
                {
                    case 0:
                        meteors.Add(new Meteor(tex, new Vector2(Game1.random.Next(-25, (int)Game1.levelManager.CurrentLevel.mapView.Width), -25), new Vector2(50f, 50f), scale, true));
                        break;
                    case 1:
                        meteors.Add(new Meteor(tex, new Vector2(-25, Game1.random.Next(-25, (int)Game1.levelManager.CurrentLevel.mapView.Height)), new Vector2(50f, 50f), scale, true));
                        break;
                    case 2:
                        meteors.Add(new Meteor(tex, new Vector2(Game1.random.Next(-25, (int)Game1.levelManager.CurrentLevel.mapView.Width), Game1.levelManager.CurrentLevel.mapView.Height - 25), new Vector2(-50f, -50f), scale, false));
                        break;
                }
            }
        }
    }

    public class Meteor:Sprite
    {
        bool tf;
        public Meteor(Texture2D texture, Vector2 position, Vector2 velocity, bool topleftOrBottomRight)
            :base(texture, position, velocity, true, 0.2f, 0.1f, SpriteEffects.None)
        {
            Rotation = ((float)Game1.random.NextDouble()) * RotationSpeed;
            RotationSpeed = Rotation;
            Scale = ((float)Game1.random.NextDouble()) * Scale;
            tf = topleftOrBottomRight;
        }
        public Meteor(Texture2D texture, Vector2 position, Vector2 velocity, float scale, bool topleftOrBottomRight)
            : base(texture, position, velocity, true, 0.2f, scale, SpriteEffects.None)
        {
            tf = topleftOrBottomRight;
        }

        public override void Update(GameTime gameTime)
        {
            if (tf) { Right(); Down(); }
            else { Right(); Down(); }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }
    }
}
