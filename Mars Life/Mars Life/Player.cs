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
    public class Player : SpriteFromSheet
    {
        public Vector2 runSpeed;

        #region What Was I thinking here! This obviously goes in LevelManager
        //private bool inCutscene;
        #region Communication Methods
        //private List<CommunicationData> dialogue;
        //public int ChatIndex { get; private set; }
        //public int ChatSectionResetIndex { get; protected set; }

        //public bool IncrimentChatIndex()
        //{
        //    if (ChatIndex >= dialogue.Count - 1) { ChatIndex = ChatSectionResetIndex; return false; }
        //    else { ++ChatIndex; return true; }
        //}
        //public CommunicationData GetCurrentCommunication()
        //{
        //    return dialogue[ChatIndex];
        //}
        //public CommunicationData GetNextCommunication()
        //{
        //    ++ChatIndex;
        //    if (ChatIndex >= dialogue.Count - 1) { ChatIndex = ChatSectionResetIndex; return GetCurrentCommunication(); }
        //    else { return dialogue[ChatIndex]; }
        //}
        //public CommunicationData GetPreviousCommunication()
        //{
        //    --ChatIndex;
        //    if (ChatIndex < 0) { ChatIndex = 0; return new CommunicationData(); }
        //    else { return dialogue[ChatIndex]; }
        //}
        //public CommunicationData GetCommunicationAtIndex(int index)
        //{
        //    if (index <= (dialogue.Count - 1)) { return dialogue[index]; }
        //    else { return new CommunicationData(); }
        //}
        #endregion
        #region AutoMovement Methods
        //private List<MovementData> movements;
        //private int maxMovementIndex;
        //private int currentMovementIndex;
        //private int currentNumMovement;

        //public void AddMovementData(MovementData data)
        //{
        //    movements.Add(data);
        //}
        #endregion
        #endregion

        public string Name;

        private bool standingStill;
        public int directionSheetRow;
        public bool runWalk;
        private const float RUN_TOTAL_TIME = 2f;
        private const float WALK_TOTAL_TIME = 4f;
        public bool overrideMovement;
        private GamePadState previGamePadState;
        private KeyboardState prevKeyState;

        public bool InCutscene { get; private set; }
        public void EnterCutscene(MovementData movementData, CommunicationData communicationData)
        {

        }

        public Rectangle GetFeetRectangle()
        {
            int widHei = 2;
            switch (directionSheetRow)
            {
                case 0: // Up
                    return new Rectangle((int)(Position.X),
                                                    (int)(Position.Y + (SpriteHeight / 1.2)),
                                                    (int)widHei,
                                                    (int)widHei);
                case 1: // Left
                    return new Rectangle((int)(Position.X),
                                                    (int)(Position.Y + SpriteHeight / 1.2),
                                                    (int)widHei,
                                                    (int)widHei);
                case 2: // Right
                    return new Rectangle((int)(Position.X),
                                                    (int)(Position.Y + SpriteHeight / 1.2),
                                                    (int)widHei,
                                                    (int)widHei);
                case 3: // Down
                    return new Rectangle((int)(Position.X),
                                                    (int)(Position.Y + SpriteHeight / 1.2),
                                                    (int)widHei,
                                                    (int)widHei);
            }
            return new Rectangle();
        }

        public Player(ContentManager content, Vector2 position)
            : base(content.Load<Texture2D>("Images/Characters/Emily Warion/Coat_Headband"),
                    position,
                    new Vector2(100, 100),
                    true,
                    0f,
                    1.0f,
                    SpriteEffects.None,
                    new Vector2(32, 32),
                    Vector2.Zero,
                    new Vector2(3, 4),
                    4f)
        {
            Name = "Emily Warion";
            runSpeed = new Vector2(50, 50);
            overrideMovement = false;
        }

        public void Walk()
        {
            if (runWalk == true)
            {
                runWalk = false;
                TotalTime = WALK_TOTAL_TIME;
                TimeToNextFrame = WALK_TOTAL_TIME / (sheetSize.X * sheetSize.Y);
                ElapsedTime = 0f;
            }
        }
        public void Run()
        {
            if (runWalk == false)
            {
                runWalk = true;
                TotalTime = RUN_TOTAL_TIME;
                TimeToNextFrame = RUN_TOTAL_TIME / (sheetSize.X * sheetSize.Y);
                ElapsedTime = 0f;
            }
        }

        private void UpdatePlayerInput(GameTime gameTime, GraphicsDevice Device)
        {
            bool keyPressed = false;

            KeyboardState keyState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            //if (keyState.IsKeyDown(Keys.LeftShift)
            //    || keyState.IsKeyDown(Keys.RightShift))
            //{
            //    Run();
            //}
            //else { Walk(); }

            //if (!previGamePadState.IsButtonDown(Buttons.LeftThumbstickDown))
            //{
            //    if (gamePadState.IsButtonDown(Buttons.LeftThumbstickDown))
            //    {
            //        if (runWalk) { Walk(); }
            //    }
            //    else { Run(); }
            //}

            if (keyState.IsKeyDown(Keys.W) ||
                gamePadState.ThumbSticks.Left.Y > +0.5f)
            {
                FaceUp();
                keyPressed = true;
            }
            if (keyState.IsKeyDown(Keys.S) ||
                gamePadState.ThumbSticks.Left.Y < -0.5f)
            {
                FaceDown();
                keyPressed = true;
            }
            if (keyState.IsKeyDown(Keys.A) ||
                gamePadState.ThumbSticks.Left.X < -0.5f)
            {
                FaceLeft();
                keyPressed = true;
            }
            if (keyState.IsKeyDown(Keys.D) ||
                gamePadState.ThumbSticks.Left.X > +0.5f)
            {
                FaceRight();
                keyPressed = true;
            }

            if (!keyPressed)
            {
                Idle();
            }

            if (!standingStill) { base.Update(gameTime, Device, directionSheetRow); }

            previGamePadState = gamePadState;
            prevKeyState = keyState;
        }

        public override void Update(GameTime gameTime, GraphicsDevice Device)
        {
            UpdatePlayerInput(gameTime, Device);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }

        public override void Down()
        {
            FaceDown();
            base.Down();
            if (runWalk) { velocity.Y += runSpeed.Y; }
        }
        public override void Up()
        {
            FaceUp();
            base.Up();
            if (runWalk) { velocity.Y -= runSpeed.Y; }
        }
        public override void Left()
        {
            FaceLeft();
            base.Left();
            if (runWalk) { velocity.X -= runSpeed.X; }
        }
        public override void Right()
        {
            FaceRight();
            base.Right();
            if (runWalk) { velocity.X += runSpeed.X; }
        }

        public void FaceDown()
        {
            standingStill = false;
            directionSheetRow = 0;
            //if (runWalk) { velocity.Y += runSpeed.Y; }
        }
        public void FaceUp()
        {
            standingStill = false;
            directionSheetRow = 3;
            //if (runWalk) { velocity.Y -= runSpeed.Y; }
        }
        public void FaceLeft()
        {
            standingStill = false;
            directionSheetRow = 1;
            //if (runWalk) { velocity.X -= runSpeed.X; }
        }
        public void FaceRight()
        {
            standingStill = false;
            directionSheetRow = 2;
            //if (runWalk) { velocity.X += runSpeed.X; }
        }
        public override void Idle()
        {
            standingStill = true;
            switch (directionSheetRow)
            {
                case 0:
                    currentFrame = new Vector2(1, 0);
                    break;
                case 1:
                    currentFrame = new Vector2(1, 1);
                    break;
                case 2:
                    currentFrame = new Vector2(1, 2);
                    break;
                case 3:
                    currentFrame = new Vector2(1, 3);
                    break;
            }
            base.Idle();
        }
    }
}