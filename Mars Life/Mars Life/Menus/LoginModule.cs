using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
    public class LoginModule
    {
        protected enum FadeStatus
        {
            FadingIn,
            Normal,
            FadingOut,
        }
        FadeStatus fadeStatus;
        GameState newGameState;

        protected float fadeFloat;
        protected const float FADE_CONST = 0.003f;
        protected const float FADE_OUT_CONST = 0.005f;

        public int selectedIndex;
        public bool loginModuleHasFocus;
        bool readyToTransition;

        GraphicsDevice device;

        string username;
        string password;

        float mRotation;
        float mScale;
        Vector2 mPosition;
        Vector2 mOrgin;
        Vector2 textPos;
        Color fadeColor;

        KeyboardState keyState;
        GamePadState padState;

        public LoginModule(ContentManager content, GraphicsDevice Device)
        {
            fadeStatus = FadeStatus.FadingIn;
            fadeFloat = 1f;

            selectedIndex = 0;
            username = "";
            password = "";
            loginModuleHasFocus = true;

            readyToTransition = false;

            device = Device;
            fadeColor = Color.Black;

            mRotation = 0f;
            mScale = 0.35f;
            mPosition = new Vector2((Device.Viewport.Width / 2f) + 4 - (Game1.levelManager.communicationManager.communicationBackground.Width * mScale) / 2f, Device.Viewport.Height / 4.5f - 50 + (Game1.levelManager.communicationManager.communicationBackground.Height * mScale) / 2f);
            mOrgin = new Vector2((Game1.levelManager.communicationManager.communicationBackground.Width * mScale) / 2f, (Game1.levelManager.communicationManager.communicationBackground.Height * mScale) / 2f);

            textPos = mPosition + new Vector2(25f, 50f);
            Init();
        }

        private void Init()
        {
            //Append characters to the typedText string when the player types stuff on the keyboard.
            KeyGrabber.InboundCharEvent += (inboundCharacter) =>
            {
                if (inboundCharacter == 8)
                {

                    if (loginModuleHasFocus)
                    {
                        Debug.WriteLine("Escape Pressed");
                        if (selectedIndex == 0)
                        {
                            if (username.Length > 0)
                            {
                                username = username.Substring(0, username.Length - 1);
                            }
                        }
                        if (selectedIndex == 1)
                        {
                            if (password.Length > 0)
                            {
                                password = password.Substring(0, password.Length - 1);
                            }
                        }
                    }
                }
                //check for enter key, add text if not
                if (inboundCharacter != 13)
                {
                    //Only append letters
                    if (inboundCharacter < 65)
                        return;

                    if (inboundCharacter > 122)
                        return;

                    if (loginModuleHasFocus)
                    {
                        if (selectedIndex == 0) { if (username.Length <= 11) { username += inboundCharacter; } }
                        if (selectedIndex == 1) { if (password.Length <= 11) { password += inboundCharacter; } }
                    }
                }
                else
                {
                    if (loginModuleHasFocus)
                    {
                        if (fadeStatus == FadeStatus.Normal)
                        {
                            Game1.saveGame.loginInfo = new LoginInfo(username, password);
                            Debug.WriteLine("Database Login Saved");
                            loginModuleHasFocus = false;
                            Debug.WriteLine(Game1.saveGame.loginInfo.username + " | " + Game1.saveGame.loginInfo.password);
                            fadeStatus = FadeStatus.FadingOut;
                            newGameState = GameState.MainMenu;
                        }
                    }
                }
            };
        }

        protected bool CheckKey(Microsoft.Xna.Framework.Input.Keys key)
        {
            return (keyState.IsKeyUp(key) && Game1.prevKeyState.IsKeyDown(key));
        }
        protected bool CheckButton(Buttons button)
        {
            return (padState.IsButtonDown(button) && Game1.prevPadState.IsButtonUp(button));
        }

        public void Update(GameTime gameTime)
        {
            keyState = Keyboard.GetState();
            padState = GamePad.GetState(PlayerIndex.One);

            if (!readyToTransition)//loginModuleHasFocus)
            {
                switch (fadeStatus)
                {
                    case FadeStatus.FadingIn:
                        if (!readyToTransition)
                        {
                            if (MediaPlayer.Queue[0].Name != Game1.mainMenu.backgroundMusic.Name) { Debug.WriteLine("Changing Music to Main Menu Music"); MediaPlayer.Play(Game1.mainMenu.backgroundMusic); }
                            if (fadeFloat <= 0.0f) { fadeStatus = FadeStatus.Normal; Debug.WriteLine("State Switch: Normal"); }
                            fadeFloat -= FADE_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                            MediaPlayer.Volume += FADE_OUT_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        }
                        break;
                    case FadeStatus.Normal:
                        //Debug.WriteLine("Selected Index: "+selectedIndex);
                        if (CheckKey(Microsoft.Xna.Framework.Input.Keys.Up) || CheckButton(Buttons.DPadUp))
                        {
                            Debug.WriteLine("Moving Index Up");
                            //selectedIndex = 0;
                            if (selectedIndex == 0) { selectedIndex = 1; }
                            else { selectedIndex = 0; }
                        }
                        if (CheckKey(Microsoft.Xna.Framework.Input.Keys.Down) || CheckButton(Buttons.DPadDown))
                        {
                            Debug.WriteLine("Move Index Down");
                            //selectedIndex = 1;
                            if (selectedIndex == 1) { selectedIndex = 0; }
                            else { selectedIndex = 1; }
                        }
                        break;
                    case FadeStatus.FadingOut:
                        if (fadeFloat >= 1.0f) { fadeStatus = FadeStatus.FadingIn; readyToTransition = true; Debug.WriteLine("State Switch: FadingIn"); }
                        fadeFloat += FADE_OUT_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        MediaPlayer.Volume -= FADE_OUT_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        break;
                }
            }
            else
            {
                Debug.WriteLine("State Switch: Main Menu");
                Game1.gameState = newGameState;

                Game1.saveGame.CreatePlayer();

                fadeStatus = FadeStatus.FadingIn;
            }

            Game1.prevKeyState = keyState;
            Game1.prevPadState = padState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Game1.levelManager.communicationManager.communicationBackground, mPosition, null, Color.GhostWhite * Game1.pauseMenu.colorOverlayFloat, mRotation, Vector2.Zero, mScale, SpriteEffects.None, 0f);

            if (selectedIndex == 0) 
            {
                RenderText.Draw(spriteBatch, "Username: " + username, textPos, Color.DarkSeaGreen, Game1.segoe16Font);
                RenderText.Draw(spriteBatch, "Password: " + password, textPos + new Vector2(0, 50f), Color.GhostWhite, Game1.segoe16Font);
            }
            if (selectedIndex == 1)
            {
                RenderText.Draw(spriteBatch, "Username: " + username, textPos, Color.GhostWhite, Game1.segoe16Font);
                RenderText.Draw(spriteBatch, "Password: " + password, textPos + new Vector2(0, 50f), Color.DarkSeaGreen, Game1.segoe16Font);
            }

            spriteBatch.Draw(Game1.blankTexture, device.Viewport.Bounds, fadeColor * fadeFloat);
            spriteBatch.End();
        }

    }

    public class KeyGrabber
    {
        public class KeyFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (Game1.loginModule.loginModuleHasFocus)
                {

                    /*
                        These are the message constants we will be watching for.
                    */
                    const int WM_KEYDOWN = 0x0100;
                    const int WCHAR_EVENT = 0x0102;

                    if (m.Msg == WM_KEYDOWN)
                    {
                        /*
                            The TranslateMessage function requires a pointer to be passed to it.
                            Since C# doesn't typically use pointers, we have to make use of the Marshal
                            class to store the Message into a pointer. We can then pass this pointer
                            over to the native function.
                        */
                        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(m));
                        Marshal.StructureToPtr(m, pointer, true);
                        TranslateMessage(pointer);
                    }
                    else if (m.Msg == WCHAR_EVENT)
                    {
                        //The WParam parameter contains the true character value
                        //we are after. Print this out to the screen and call the
                        //InboundCharEvent so any events hooked up to this will be
                        //notifed that there is a char ready to be processed.
                        Debug.WriteLine("Key Pressed: " + m.WParam);
                        char trueCharacter = (char)m.WParam;
                        Console.WriteLine(trueCharacter);

                        if (InboundCharEvent != null)
                            InboundCharEvent(trueCharacter);
                    }
                }
                //Returning false allows the message to continue to the next filter or control.
                return false;
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern bool TranslateMessage(IntPtr message);
        }

        public static event Action<char> InboundCharEvent;
        static KeyGrabber()
        {
            Application.AddMessageFilter(new KeyFilter());
        }
    }

}
