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
    public class PauseMenu:Menu
    {
        GraphicsDevice device;
        ContentManager content;

        Color colorOverlay;
        public float colorOverlayFloat;

        public bool transferToMainMenu;

        #region Main Category Variables
        float mRotation;
        float mScale;
        Vector2 mPosition;
        Vector2 mOrgin;

        #endregion
        #region Sub Category Variables
        float sScale;
        float sRotation;
        Vector2 sPosition;
        Vector2 sOrgin;
        #endregion

        public PauseMenu(ContentManager Content, GraphicsDevice _Device)
            :base(_Device, Game1.segoeFont, new List<string> {})
        {
            content = Content;
            device = _Device;

            AddOption("Resume");
            AddOption("Same Game");
            AddOption("Load Game");
            AddOption("Title Screen");
            AddOption("Exit Game");

            //position = new Vector2(0, position.Y);
            position.X = 7f;
            position.Y -= 20f;
            textSeperation = 15f;

            transferToMainMenu = false;

            colorOverlay = Color.Black;
            colorOverlayFloat = 0.5f;

            fadeFloat = 0;
            textFade = 1f;
            fadeStatus = FadeStatus.Normal;

            #region Main Category
            mRotation = MathHelper.ToRadians(90f);
            mScale = 0.35f;
            mPosition = new Vector2(0f + (Game1.levelManager.communicationManager.communicationBackground.Width * mScale) / 2f, Device.Viewport.Height / 4.5f);
            mOrgin = new Vector2((Game1.levelManager.communicationManager.communicationBackground.Width * mScale) / 2f, (Game1.levelManager.communicationManager.communicationBackground.Height *mScale) / 2f);
            #endregion
            #region Sub Category
            sRotation = MathHelper.ToRadians(0f);
            sScale = 0.5f;
            sPosition = new Vector2(0f - (mPosition.X / 2f) + (Game1.levelManager.communicationManager.communicationBackground.Width * sScale) / 2f, Device.Viewport.Height / 4.5f);
            sOrgin = new Vector2((Game1.levelManager.communicationManager.communicationBackground.Width * sScale) / 2f, (Game1.levelManager.communicationManager.communicationBackground.Height * sScale) / 2f);
            #endregion

        }

        public override void Update(GameTime gameTime)
        {
            keyState = Keyboard.GetState();
            padState = GamePad.GetState(PlayerIndex.One);

            if (transferToMainMenu)
            {
                Game1.levelManager.FadeToMenu(gameTime);
            }
            else
            {
                if (CheckKey(Keys.Escape) || CheckButton(Buttons.Back) || CheckButton(Buttons.B))
                {
                    Game1.gameState = GameState.PlayGame;
                    optionSelected = false;
                    inSubMenu = false;
                    ItemSelected = 0;

                }

                MenuSelect();

                if (optionSelected)
                {
                    switch (ItemSelected)
                    {
                        case 1:
                            Game1.gameState = GameState.PlayGame;
                            optionSelected = false;
                            inSubMenu = false;
                            ItemSelected = 0;
                            break;
                        case 2:
                            Game1.saveGame.SaveToDatabase();
                            //while (!Game1.saveGame.SaveToFile(0)) { }

                            //inSubMenu = true;
                            optionSelected = false;
                            break;
                        case 3:
                            Game1.levelManager.StartGame(true);

                            //while (true)
                            //{
                            //    int i = Game1.saveGame.LoadFile(0);
                            //    if (i == 1) { break; }
                            //    if (i == -1) { break; }
                            //}

                            //inSubMenu = true;
                            optionSelected = false;
                            break;
                        case 4:
                            transferToMainMenu = true;
                            optionSelected = false;
                            inSubMenu = false;
                            ItemSelected = 0;
                            break;
                        case 5:
                            Game1.gameState = GameState.ExitGame;
                            optionSelected = false;
                            inSubMenu = false;
                            ItemSelected = 0;
                            break;
                    }
                }
            }

            Game1.prevKeyState = keyState;
            Game1.prevPadState = padState;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Game1.blankTexture, device.Viewport.Bounds, colorOverlay * colorOverlayFloat);

            // Draw Background For Main Categories
            spriteBatch.Draw(Game1.levelManager.communicationManager.communicationBackground, mPosition, null, Color.GhostWhite * colorOverlayFloat, mRotation, Vector2.Zero, mScale, SpriteEffects.None, 0f);

            if (inSubMenu)
            {
                spriteBatch.Draw(Game1.levelManager.communicationManager.communicationBackground, sPosition, null, Color.GhostWhite * colorOverlayFloat, sRotation, Vector2.Zero, sScale, SpriteEffects.None, 0f);
            }

            base.Draw(gameTime, spriteBatch);
            spriteBatch.End();
        }
    }
}
