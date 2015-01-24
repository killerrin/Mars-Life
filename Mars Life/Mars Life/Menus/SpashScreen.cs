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
    public class SplashScreen:Menu
    {
        Song backgroundMusic;
        SoundEffect killerrinStudiosBlip;
        SoundEffectInstance killerrinStudiosBlipInstance;

        GraphicsDevice device;
        ContentManager content;

        GameState newGameState;

        public Texture2D killerrinStudiosLogo;

        float aspectRatio;
        float modelRotation;

        Vector3 modelPosition;  // Set the position of the model in world space, and set the rotation.
        Vector3 cameraPosition = new Vector3(0.0f, 50.0f, 5000.0f); // Set the position of the camera in world space, for our view matrix.

        public SplashScreen(ContentManager Content, GraphicsDevice _Device)
            :base(_Device, Game1.segoeFont, new List<string> {})
        {
            content = Content;
            device = _Device;
            aspectRatio = _Device.Viewport.AspectRatio;

            fadeFloat = -0.35f;
            fadeStatus = FadeStatus.FadingOut;

            timerUp = false;
            screenTimer = 1f;
            SCREEN_TIMER = 2900f;

            newGameState = GameState.None;

            killerrinStudiosLogo = Content.Load<Texture2D>("Images/Splash Screen/logo");
            killerrinStudiosBlip = Content.Load<SoundEffect>("Audio/Sound Effects/killerrin studios blip");
            killerrinStudiosBlipInstance = killerrinStudiosBlip.CreateInstance();
            killerrinStudiosBlipInstance.Volume = 0f;
            killerrinStudiosBlipInstance.Play();
            //killerrinStudiosBlip.Play();


            modelPosition = Vector3.Zero;
            modelPosition.X = 2f;
            modelPosition.Y = -5f;

            modelRotation = 0.0f;
        }

        public override void Update(GameTime gameTime)
        {
            // Update Elapsed Game Times
            modelRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.01f);

            float nextvalue;
            switch(fadeStatus)
            {
                case FadeStatus.FadingIn:
                    if (!readyToTransition)
                    {
                        if (fadeFloat <= 0.0f) { readyToTransition = true; }
                        fadeFloat -= FADE_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                        nextvalue = FADE_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (killerrinStudiosBlipInstance.Volume - nextvalue < 0) { killerrinStudiosBlipInstance.Volume = 0f; }
                        else { killerrinStudiosBlipInstance.Volume -= nextvalue; }
                    }
                    else
                    {
                        Debug.WriteLine("State Switch: Login");

                        Game1.gameState = GameState.LogIn;
                        textReadyToTransition = false;
                        readyToTransition = false;
                        timerUp = false;
                    }
                    break;
                case FadeStatus.Normal:
                    screenTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (screenTimer >= SCREEN_TIMER)
                    {
                        timerUp = true;
                        readyToTransition = false;
                        fadeStatus = FadeStatus.FadingIn;
                        textFadeStatus = FadeStatus.FadingIn;
                    }
                    //MenuSelect();
                    break;
                case FadeStatus.FadingOut:
                    if (fadeFloat >= 1.0f) { fadeStatus = FadeStatus.Normal; Debug.WriteLine("State Switch: Normal"); }
                    fadeFloat += FADE_OUT_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    nextvalue = FADE_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (killerrinStudiosBlipInstance.Volume + nextvalue > 1) { killerrinStudiosBlipInstance.Volume = 1f; }
                    else { killerrinStudiosBlipInstance.Volume += nextvalue; }
                    break;
            }
            #region Old Code
            //if (!readyToTransition || !timerUp) //!textReadyToTransition || 
            //{
            //    base.Update(gameTime);
            //    if (fadeStatus == FadeStatus.FadingIn)
            //    {
            //        readyToTransition = false;
            //        if (fadeFloat <= 0)
            //        {
            //            readyToTransition = true;
            //            Debug.WriteLine("Ready to Transition");
            //        }  
            //    }
            //    if (fadeStatus == FadeStatus.Normal)//optionSelected)
            //    {
            //        readyToTransition = false;
            //        screenTimer += (float)gameTime.ElapsedGameTime.Milliseconds;
            //        Debug.WriteLine(screenTimer);
            //        if (screenTimer >= SCREEN_TIMER) 
            //        {
            //            timerUp = true;
            //            fadeStatus = FadeStatus.FadingIn;
            //            textFadeStatus = FadeStatus.FadingIn;
            //        }

                    #region oldcode
                    //switch (ItemSelected)
                    //{
                    //    case 1:
                    //        newGameState = GameState.PlayGame;
                    //        break;
                    //    case 2:
                    //        newGameState = GameState.LoadGame;
                    //        break;
                    //    case 3:
                    //        newGameState = GameState.ExitGame;
                    //        break;
                    //}
                    #endregion
            //    }
            //    //Game1.prevKeyState = keyState;
            //}
            //else
            //{
            //    base.Update(gameTime);
            //    Debug.WriteLine("State Switch: Main Menu");

            //    Game1.gameState = GameState.MainMenu;
            //    textReadyToTransition = false;
            //    readyToTransition = false;
            //    timerUp = false;
            //}

            #region oldCode
            //switch (fadeStatus)
            //{
            //    case FadeStatus.Normal:
            //        splashScreenTimer += (float)gameTime.ElapsedGameTime.Milliseconds;
            //        if (splashScreenTimer >= SPLASH_SCREEN_TIMER)
            //        {
            //            newGameState = GameState.MainMenu; 
            //            fadeStatus = FadeStatus.FadingOut;
            //            textFadeStatus = FadeStatus.FadingOut;
            //        }
            //        break;
            //    case FadeStatus.FadingOut:
            //        if (textReadyToTransition || readyToTransition)
            //        {
            //            if (fadeFloat >= 1.0f)
            //            {
            //                Game1.gameState = newGameState;
            //                textReadyToTransition = false;
            //                readyToTransition = false;
            //            }
            //        }
            //        break;
            //}

            //if (splashScreenTimer >= SPLASH_SCREEN_TIMER) { gameState = GameState.MainMenu; }
            #endregion
            #endregion
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            base.Draw(gameTime, spriteBatch);
            spriteBatch.Draw(killerrinStudiosLogo, new Vector2((Device.Viewport.Width / 2f - (killerrinStudiosLogo.Width / 2.7f)), (Device.Viewport.Height / 2f - (killerrinStudiosLogo.Height / 2f))), null, Color.White * fadeFloat, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }
    }
}
