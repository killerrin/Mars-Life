using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace Mars_Life
{
    public class Menu
    {
        protected enum FadeStatus
        {
            FadingIn,
            Normal,
            FadingOut,
        }
        protected enum TextPulsatingStatus
        {
            PulseIn,
            PulseOut,
        }
        protected bool textReadyToTransition;
        protected bool readyToTransition;
        protected bool optionSelected;
        protected bool timerUp;
        protected bool inSubMenu;

        protected FadeStatus fadeStatus;
        protected FadeStatus textFadeStatus;
        protected TextPulsatingStatus textPulseStatus;
        protected float fadeFloat;
        protected const float FADE_CONST = 0.003f;
        protected const float FADE_OUT_CONST = 0.0008f;
        protected const float PULSE_CONST = 0.00008f;

        protected float screenTimer;
        protected float SCREEN_TIMER = 0; // 1000 = 1 second

        protected Color fadeColor;

        protected List<string> menuItems;
        protected int selectedIndex;

        protected Color normal = Color.GhostWhite;
        protected Color selected = Color.DarkSeaGreen;//Color.Wheat; Color.LawnGreen, Color.DarkSeaGreen

        protected KeyboardState keyState;
        protected GamePadState padState;

        protected SpriteFont font;
        protected SpriteFont outlineFont = Game1.segoe16Font;

        protected Song song;

        protected int itemSelected = 0;

        protected Vector2 position;
        protected float width = 0f;
        protected float height = 0f;
        protected float textFade;
        protected float textSeperation;

        protected GraphicsDevice Device;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            protected set
            {
                selectedIndex = value;
                //keep it in range of the # items on the menu
                if (selectedIndex < 0)
                    selectedIndex = menuItems.Count - 1;
                if (selectedIndex >= menuItems.Count)
                    selectedIndex = 0;
            }
        }

        public int ItemSelected
        {
            get { return itemSelected; }
            protected set { itemSelected = value; }
        }

        public void AddOption(string _string)
        {
            menuItems.Add(_string);
            MeasureMenu();
        }

        public void AddOptionAt(string _string, int index)
        {
            menuItems.Insert(index, _string);
            MeasureMenu();
        }

        public void RenameOption(int index, string newString)
        {
            menuItems[index] = newString;
            MeasureMenu();
        }

        public void RemoveOption(int index)
        {
            menuItems.RemoveAt(index);
            MeasureMenu();
        }

        public Menu(GraphicsDevice _Device, SpriteFont spriteFont, List<string> _menuItems)
        {
            fadeStatus = FadeStatus.FadingIn;
            textFadeStatus = FadeStatus.FadingIn;
            textPulseStatus = TextPulsatingStatus.PulseOut;

            readyToTransition = false;
            textReadyToTransition = false;
            optionSelected = false;
            inSubMenu = false;
            timerUp = true;

            fadeFloat = 1f;
            textFade = 0f;
            screenTimer = 0f;
            SCREEN_TIMER = 0f;

            textSeperation = 15f;

            fadeColor = Color.Black;

            font = spriteFont;
            menuItems = _menuItems;
            Device = _Device;
            MeasureMenu();
        }

        public void MeasureMenu()
        {
            height = 0f;
            width = 0f;

            foreach (string item in menuItems)
            {
                Vector2 size = font.MeasureString(item);
                if (size.X > width)
                    width = size.X;
                height += font.LineSpacing + 5;
            }
            position = new Vector2((Device.Viewport.Width-width) / 2, (Device.Viewport.Height-height) / 2);
        }

        protected bool CheckKey (Keys key)
        {
            return (keyState.IsKeyUp(key) && Game1.prevKeyState.IsKeyDown(key)); 
        }
        protected bool CheckButton(Buttons button)
        {
            return (padState.IsButtonDown(button) && Game1.prevPadState.IsButtonUp(button));
        }

        public void MenuSelect()
        {
            keyState = Keyboard.GetState();
            padState = GamePad.GetState(PlayerIndex.One);

            if (CheckKey(Keys.Down) || CheckKey(Keys.S) || CheckButton(Buttons.DPadDown) || CheckButton(Buttons.LeftThumbstickDown))
            {
                ++SelectedIndex;
                if (selectedIndex > menuItems.Count)
                {
                    selectedIndex = 0;
                }
            }
            if (CheckKey(Keys.Up) || CheckKey(Keys.W) || CheckButton(Buttons.DPadUp) || CheckButton(Buttons.LeftThumbstickUp))
            {
                --SelectedIndex;
                if (SelectedIndex < 0)
                {
                    SelectedIndex = menuItems.Count - 1;
                }
            }

            if (CheckKey(Keys.Enter) || CheckKey(Keys.Space) || CheckButton(Buttons.A) || CheckButton(Buttons.Start))
            {
                optionSelected = true;
                ItemSelected = SelectedIndex + 1;
            }

            Game1.prevKeyState = keyState;
            Game1.prevPadState = padState;
        }

        public virtual void Update(GameTime gameTime)
        {
            switch(fadeStatus)
            {
                case FadeStatus.FadingIn:
                    if (!readyToTransition)
                    {
                        if (fadeFloat <= 0.0f) { fadeStatus = FadeStatus.Normal; Debug.WriteLine("State Switch: Normal"); }
                        fadeFloat -= FADE_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        MediaPlayer.Volume += FADE_OUT_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                    break;
                case FadeStatus.Normal:
                    MenuSelect();
                    break;
                case FadeStatus.FadingOut:
                    if (fadeFloat >= 1.0f) { fadeStatus = FadeStatus.FadingIn; readyToTransition = true; Debug.WriteLine("State Switch: FadingIn"); }
                    fadeFloat += FADE_OUT_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    MediaPlayer.Volume -= FADE_OUT_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    break;
            }

            switch (textFadeStatus)
            {
                case FadeStatus.FadingOut:
                    if (textFade <= 0.0f) { textFadeStatus = FadeStatus.FadingOut; textReadyToTransition = true; }
                    textFade -= FADE_OUT_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    break;
                case FadeStatus.Normal:
                    switch (textPulseStatus)
                    {
                        case TextPulsatingStatus.PulseIn:
                            if (textFade <= 0.7f) { textPulseStatus = TextPulsatingStatus.PulseOut; }
                            textFade -= PULSE_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                            break;
                        case TextPulsatingStatus.PulseOut:
                            if (textFade >= 1.0f) { textPulseStatus = TextPulsatingStatus.PulseIn; }
                            textFade += PULSE_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                            break;
                    }
                    break;
                case FadeStatus.FadingIn:
                    if (!textReadyToTransition)
                    {
                        if (textFade >= 1.0f) { textFadeStatus = FadeStatus.Normal; }
                        textFade += FADE_CONST * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                    break;
            }
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 location = position;
            Color hilight;
            for (int i = 0; i < menuItems.Count; i++)
            {
                if (i == SelectedIndex)
                {
                    hilight = selected;
                }
                else
                {
                    hilight = normal;
                }

                //spriteBatch.DrawString(outlineFont, menuItems[i], location + new Vector2(0, 0 * i), Color.Black * textFade);
                spriteBatch.DrawString(outlineFont, menuItems[i], location + new Vector2(0, textSeperation * i), hilight * textFade);
                location.Y += font.LineSpacing + 5;

                spriteBatch.Draw(Game1.blankTexture, Device.Viewport.Bounds, fadeColor*fadeFloat);
            }
        }
    }
}
