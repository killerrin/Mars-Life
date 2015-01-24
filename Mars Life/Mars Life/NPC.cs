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
    public struct MovementData
    {
        public string movement;
        public int numberOfTimes;
        public MovementData (String movementWord, int numOfTimes)
        {
            movement = movementWord;
            numberOfTimes = numOfTimes;
        }
    }

    public class NPC : SpriteFromSheet
    {
        public Vector2 runSpeed;

        private bool standingStill;
        public int directionSheetRow;
        public bool runWalk;
        public bool stopMoving;
        private const float RUN_TOTAL_TIME = 2f;
        private const float WALK_TOTAL_TIME = 4f;

        private List<MovementData> movements;
        private int maxMovementIndex;
        public int currentMovementIndex;
        public int currentNumMovement;

        public void AddMovementData(MovementData data)
        {
            movements.Add(data);
            ++maxMovementIndex;
        }
        public void AddMovementData(List<MovementData> data)
        {
            movements.AddRange(data);
            foreach (MovementData d in data) { ++maxMovementIndex; }
        }
        public void ResetMovementData()
        {
            movements = new List<MovementData>();
            maxMovementIndex = 0;
            currentMovementIndex = 0;
            currentNumMovement = 0;
        }
        public MovementData CurrentMovementData()
        {
            return movements[currentMovementIndex];
        }

        public Rectangle GetFeetRectangle()
        {
            //int widHei = 2;
            switch (directionSheetRow)
            {
                case 0: // Up
                    return new Rectangle((int)(Position.X),
                                                    (int)(Position.Y + (SpriteHeight / 1.2)),
                                                    (int)TextureImage.Width,
                                                    (int)(TextureImage.Height - (SpriteHeight / 1.2)));
                case 1: // Left
                    return new Rectangle((int)(Position.X),
                                                    (int)(Position.Y + SpriteHeight / 1.2),
                                                    (int)TextureImage.Width,
                                                    (int)(TextureImage.Height - (SpriteHeight / 1.2)));
                case 2: // Right
                    return new Rectangle((int)(Position.X),
                                                    (int)(Position.Y + SpriteHeight / 1.2),
                                                    (int)TextureImage.Width,
                                                    (int)(TextureImage.Height - (SpriteHeight / 1.2)));
                case 3: // Down
                    return new Rectangle((int)(Position.X),
                                                    (int)(Position.Y + SpriteHeight / 1.2),
                                                    (int)TextureImage.Width,
                                                    (int)(TextureImage.Height - (SpriteHeight / 1.2)));
            }
            return new Rectangle();
        }

        public NPC(Texture2D texture, Vector2 locationOnMap, List<MovementData> movement)
            : base(texture,
                    locationOnMap,
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
            runSpeed = new Vector2(50, 50);
            movements = movement;
            stopMoving = false;
            maxMovementIndex = movements.Count();
            currentMovementIndex = 0;
            directionSheetRow = 2;
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

        public void MoveMap(Vector2 movement)
        {
            position += movement;
        }

        public override void Update(GameTime gameTime)
        {
            if (!stopMoving)
            {
                switch (movements[currentMovementIndex].movement)
                {
                    case "Face Down":
                        FaceDown();
                        Idle();
                        break;
                    case "Face Up":
                        FaceUp();
                        Idle();
                        break;
                    case "Face Left":
                        FaceLeft();
                        Idle();
                        break;
                    case "Face Right":
                        FaceRight();
                        Idle();
                        break;
                    case "Move Up":
                        Up();
                        break;
                    case "Move Down":
                        Down();
                        break;
                    case "Move Left":
                        Left();
                        break;
                    case "Move Right":
                        Right();
                        break;
                    case "Move Down Right":
                        Down();
                        Right();
                        break;
                    case "Move Down Left":
                        Down();
                        Left();
                        break;
                    case "Move Up Right":
                        Up();
                        Right();
                        break;
                    case "Move Up Left":
                        Up();
                        Left();
                        break;
                    case "Stop":
                        Idle();
                        break;
                    case "Stop Movement":
                        stopMoving = true;
                        break;
                    case "Toggle Active":
                        if (Active) { Active = false; }
                        else { Active = true; }
                        break;
                    case "Run":
                        Run();
                        break;
                    case "Walk":
                        Walk();
                        break;
                    default:
                        Idle();
                        break;
                }

                ++currentNumMovement;
                if (currentNumMovement > movements[currentMovementIndex].numberOfTimes)
                {
                    currentNumMovement = 0;
                    ++currentMovementIndex;
                    if (currentMovementIndex > maxMovementIndex - 1)
                    {
                        currentMovementIndex = 0;
                    }
                }

                //base.Update(gameTime, directionSheetRow);
                if (!standingStill) { base.Update(gameTime, directionSheetRow); }
            }
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
        }
        public void FaceUp()
        {
            standingStill = false;
            directionSheetRow = 3;
        }
        public void FaceLeft()
        {
            standingStill = false;
            directionSheetRow = 1;
        }
        public void FaceRight()
        {
            standingStill = false;
            directionSheetRow = 2;
        }
        public override void Idle()
        {
            standingStill = true;
            switch (directionSheetRow)
            {
                case 0: // Up
                    currentFrame = new Vector2(1, 0);
                    break;
                case 1: // Left
                    currentFrame = new Vector2(1, 1);
                    break;
                case 2: // Right
                    currentFrame = new Vector2(1, 2);
                    break;
                case 3: // Down
                    currentFrame = new Vector2(1, 3);
                    break;
            }
            base.Idle();
        }
    }
}