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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Mars_Life
{
    public class SpriteFromSheet : Sprite
    {
        protected Vector2 frameSize;      // Size of frames in sprite sheet
        public Vector2 FrameSize
        {
            get { return frameSize; }
            set { frameSize = value; }
        }
        protected Vector2 currentFrame;   // Index of current frame in sprite sheet
        public Vector2 CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = value; }
        }
        protected Vector2 sheetSize;      // Number of columns and rows in the sprite sheet
        public Vector2 SheetSize
        {
            get { return sheetSize; }
            set { sheetSize = value; }
        }
        // Time when we need to goto next frame
        protected float TimeToNextFrame { get; set; }
        // time so far towards next frame
        protected float ElapsedTime { get; set; }
        // total Time for whole sheet to play
        public float TotalTime { get; set; }


        //rectangle occupied by texture
        public override Rectangle CollisionRectangle
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, (int)FrameSize.X, (int)FrameSize.Y);
            }
        }

        //sprite sheet version
        public SpriteFromSheet(Texture2D textureImage, Vector2 position, Vector2 velocity,
            bool setOrigin, float rotationSpeed, float scale, SpriteEffects spriteEffect,
            Vector2 frameSize, Vector2 currentFrame, Vector2 sheetSize, float totalTime)
            : base(textureImage, position, velocity, setOrigin, rotationSpeed, scale, spriteEffect)
        {
            FrameSize = frameSize;
            CurrentFrame = currentFrame;
            SheetSize = sheetSize;
            //time to next frame is total time to play the sheet divided by number of frames
            TotalTime = totalTime;
            TimeToNextFrame = TotalTime / (sheetSize.X * sheetSize.Y);
            if (SetOrigin)
            {
                SpriteOrigin = new Vector2(FrameSize.X / 2, FrameSize.Y / 2);
            }
            SpriteWidth = (int)FrameSize.X;
            SpriteHeight = (int)FrameSize.Y;

        }

        //loops through entire spritesheet, called by autosprites in dodgeblade so they
        //can use polymorphism and check against player position
        public virtual void Update(GameTime gameTime, Vector2 player)
        {
            Update(gameTime);
        }

        //loops through entire spritesheet, called by autosprites in dodgeblade so they
        //can use polymorphism and check against player position
        //--this version keeps sprite on screen
        public virtual void Update(GameTime gameTime, GraphicsDevice Device, Vector2 player)
        {
            Update(gameTime, Device);
        }

        //loops through entire spritesheet
        public override void Update(GameTime gameTime)
        {
            //call base method to do rotation and basic movement
            base.Update(gameTime);
            //how long since last frame displayed
            ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            //if it's time to display the next one, do so, then reset time back to 0
            if (ElapsedTime > TimeToNextFrame)
            {
                if (currentFrame.X >= sheetSize.X - 1)
                {
                    currentFrame.X = 0;
                    if (currentFrame.Y >= sheetSize.Y - 1)
                    {
                        currentFrame.Y = 0;
                    }
                    else
                    {
                        currentFrame.Y++;
                    }
                }
                else
                {
                    currentFrame.X++;
                }
                ElapsedTime = 0f;
            }
        }

        //loops through entire spritesheet
        //--this version keeps sprite on screen
        public override void Update(GameTime gameTime, GraphicsDevice Device)
        {
            if (Active)
            {
                //call overload to do rotation, basic movement and frame updating
                Update(gameTime);
                //keep on screen
                if (Position.X > Device.Viewport.Width - FrameSize.X + SpriteOrigin.X)
                {
                    //GraphicsDevice.Viewport.Width or Window.ClientBounds.Width will both give us the width of the screen
                    position.X = Device.Viewport.Width - FrameSize.X + SpriteOrigin.X;
                    velocity.X = -Velocity.X;
                }
                else if (Position.X < SpriteOrigin.X)
                {
                    position.X = SpriteOrigin.X;
                    velocity.X = -Velocity.X;
                }

                if (Position.Y > Device.Viewport.Height - FrameSize.Y + SpriteOrigin.Y)
                {
                    position.Y = Device.Viewport.Height - FrameSize.Y + SpriteOrigin.Y;
                    velocity.Y = -Velocity.Y;
                }
                else if (Position.Y < SpriteOrigin.Y)
                {
                    position.Y = SpriteOrigin.Y;
                    velocity.Y = -Velocity.Y;
                }
            }
        }

        //loops through one row of spritesheet (first row is row 0)
        // This one keeps the sprite on the sceen
        public virtual void Update(GameTime gameTime, GraphicsDevice Device, int row)
        {
            base.Update(gameTime, Device);
            //how long since last frame displayed
            ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            //if it's time to display the next one, do so, then reset time back to 0
            if (ElapsedTime > TimeToNextFrame)
            {
                currentFrame.Y = row;

                if (currentFrame.X >= sheetSize.X - 1)
                {
                    currentFrame.X = 0;
                }
                else
                {
                    currentFrame.X++;
                }
                ElapsedTime = 0f;
            }
        }

        //loops through one row of spritesheet (first row is row 0)
        public virtual void Update(GameTime gameTime, int row)
        {
            base.Update(gameTime);
            //how long since last frame displayed
            ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            //if it's time to display the next one, do so, then reset time back to 0
            if (ElapsedTime > TimeToNextFrame)
            {
                currentFrame.Y = row;

                if (currentFrame.X >= sheetSize.X - 1)
                {
                    currentFrame.X = 0;
                }
                else
                {
                    currentFrame.X++;
                }
                ElapsedTime = 0f;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Active)
            {
                spriteBatch.Draw(TextureImage,
                                 Position,
                                 new Rectangle((int)(currentFrame.X * frameSize.X),
                                               (int)(currentFrame.Y * frameSize.Y),
                                               (int)frameSize.X,
                                               (int)frameSize.Y),
                                 Color.White,
                                 Rotation,
                                 SpriteOrigin,
                                 Scale,
                                 SpriteEffect,
                                 0);
            }
        }

        public override bool IsOffScreen(GraphicsDevice Device)
        {
            if (Position.X < -FrameSize.X ||
                Position.X > Device.Viewport.Width ||
                Position.Y < -FrameSize.Y ||
                Position.Y > Device.Viewport.Height)
            {
                return true;
            }
            return false;
        }
    }
}
