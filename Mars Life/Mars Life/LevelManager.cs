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
using FuncWorks.XNA.XTiled;
using System.Diagnostics;

namespace Mars_Life
{
    public class LevelManager
    {
        #region Declarations
        enum LevelState
        {
            Cutscene,
            Playing,
            Talking
        }

        enum CutsceneState
        {
            Playing,
            Talking
        }
        ContentManager content;

        LevelState levelState;
        CutsceneState cutsceneState;

        public CommunicationManager communicationManager;

        public static Rectangle viewportBounds;
        public TimeSpan RunningTime;

        public bool InCutscene;

        Player actor;
        public TalkingNPC cutsceneActor;

        #region Levels
        public Level CurrentLevel;// { get; private set; }
        public List<Level> Levels;
        #region Maps
        private Level Epolouge;
        private Level emilysHouse;
        private Level marsCity;
        #endregion

        #region Test
        private Level lakeSideTest;
        private Level lakeSide2Test;
        private Level houseTest;
        #endregion
        #endregion
        #region Sound Effects
        SoundEffect alarmClock1;
        #endregion
        #region Textures
        //-- Independent Textures to save space on ram
        // Chests
        private Texture2D chestHorizontalClosed;
        private Texture2D chestVerticalClosed;
        private Texture2D chestOpenedUp;
        private Texture2D chestOpenedDown;
        private Texture2D chestOpenedLeft;
        private Texture2D chestOpenedRight;

        //  Animals
        private Texture2D pigeon;
        private Texture2D seagull;
        #endregion

        // Screen Fade on Map Change
        public Texture2D blankTexture;
        ZoneChange zoneChange;
        private bool levelTransfer = true;
        private bool sceneIsFading = false;
        private bool sceneIsUnFading = true;
        //private int segmentIndex;
        private float currentTransparency = 1f;
        private const float TRANSPARENCY_RATE_OF_CHANGE = 0.003f;
        private float cutsceneTimer;
        private int cutsceneMovementCtr;



        private KeyboardState keyState;// = Keyboard.GetState();
        private GamePadState gamePadState;// = GamePad.GetState(PlayerIndex.One);

        CommunicationData commData = new CommunicationData("");

        int animalSpawnCtr = 0;
        #endregion

        //int savectr = 0;

        public LevelManager(ContentManager Content, GraphicsDevice device, bool loadGame)
        {
            content = Content;
            levelState = LevelState.Playing;
            cutsceneState = CutsceneState.Playing;
            RunningTime = new TimeSpan();

            communicationManager = new CommunicationManager(Content, device, new Vector2(0f, 200f));

            blankTexture = new Texture2D(device, 1, 1);
            blankTexture.SetData(new Color[] { Color.White });
            viewportBounds = device.Viewport.Bounds;

            cutsceneTimer = 0f;
            cutsceneMovementCtr = 0;

            #region Textures
            chestHorizontalClosed = Content.Load<Texture2D>("Images/Chests/Horizontal Closed");
            chestVerticalClosed = Content.Load<Texture2D>("Images/Chests/Vertical Closed");
            chestOpenedUp = Content.Load<Texture2D>("Images/Chests/Opened Up");
            chestOpenedDown = Content.Load<Texture2D>("Images/Chests/Opened Down");
            chestOpenedLeft = Content.Load<Texture2D>("Images/Chests/Opened Left");
            chestOpenedRight = Content.Load<Texture2D>("Images/Chests/Opened Right");

            pigeon = Content.Load<Texture2D>("Images/Animals/Birds/Pigeon");
            seagull = Content.Load<Texture2D>("Images/Animals/Birds/Seagull");
            #endregion
            #region Sound Effects
            alarmClock1 = Content.Load<SoundEffect>("Audio/Sound Effects/Alarm Clock");
            #endregion

            actor = new Player(Content, new Vector2((float)(viewportBounds.Width / 2), (float)(viewportBounds.Height / 2)));
            cutsceneActor = new TalkingNPC(new NPCStats("Emily Warian"), actor.TextureImage, actor.Position, new List<MovementData> { }, new List<CommunicationData> { });
            //CurrentLevel = new Level(0f, null, viewportBounds, actor);

            Levels = new List<Level>();
            LoadTestLevels(Content);
            LoadGameLevels(Content);

            StartGame(false);

            //CurrentLevel = lakeSideTest;

        }

        #region Load Levels
        private void LoadTestLevels(ContentManager Content)
        {
            lakeSideTest = new Level(-1f, Content.Load<Map>("Maps/Test/Lakeside/LakeSide"), viewportBounds, actor, null);//, Color.White, 0.8f);
            lakeSide2Test = new Level(-2f, Content.Load<Map>("Maps/Test/Lakeside2/Lakeside"), viewportBounds, actor, null); //TopLeft(x-2650 y-2006) Bottom Left(x-2650 y-2069)
            houseTest = new Level(-1.1f, Content.Load<Map>("Maps/Test/House/House"), viewportBounds, actor, null);

            //lakeSideTest.AddZone(new ZoneChange(ref marsCity, new Rectangle(2650, 2006, 50, 63), new Rectangle(4, 3352, viewportBounds.Width, viewportBounds.Height), new Vector2(74, 297)));
            lakeSideTest.AddZone(new ZoneChange(ref houseTest, new Rectangle(408 + 498, 596 + 298 - 16, 36, 36), new Rectangle(0, 0, viewportBounds.Width, viewportBounds.Height), new Vector2(498, 500)));
            lakeSide2Test.AddZone(new ZoneChange(ref lakeSideTest, new Rectangle(0, 2248, 50, 63), new Rectangle(1700, 1744, viewportBounds.Width, viewportBounds.Height), new Vector2(900, 301)));
            houseTest.AddZone(new ZoneChange(ref lakeSideTest, new Vector4(13, 16, 2, 1), new Rectangle(408, 596, viewportBounds.Width, viewportBounds.Height), new Vector2(498 + 12, 320)));

            lakeSideTest.AddNPC(new List<NPC> { new NPC(actor.TextureImage, new Vector2(350,950), new List<MovementData> { 
                                                                                                                                                                            new MovementData("Run", 1),
                                                                                                                                                                            new MovementData("Move Right", 250),
                                                                                                                                                                            new MovementData("Move Down", 50),
                                                                                                                                                                            new MovementData("Move Left", 250),
                                                                                                                                                                            new MovementData("Move Up", 50)
                                                                                                                                                                            }),
                                                                new NPC(actor.TextureImage,  new Vector2(100,300), new List<MovementData> { 
                                                                                                                                                                            new MovementData("Walk", 1),
                                                                                                                                                                            new MovementData("Move Right", 50),
                                                                                                                                                                            new MovementData("Move Down", 100),
                                                                                                                                                                            new MovementData("Move Left", 50),
                                                                                                                                                                            new MovementData("Move Up", 100)
                                                                                                                                                                            }),
                                                                new NPC(actor.TextureImage,  new Vector2(300,500), new List<MovementData> { 
                                                                                                                                                                            new MovementData("Face Right", 50),
                                                                                                                                                                            new MovementData("Face Down", 50),
                                                                                                                                                                            new MovementData("Face Left", 50),
                                                                                                                                                                            new MovementData("Face Up", 50)
                                                                                                                                                                            }),
                                                                new NPC(actor.TextureImage,  new Vector2(1045,300), new List<MovementData> { 
                                                                                                                                                                            new MovementData("Move Down", 150),
                                                                                                                                                                            new MovementData("Stop Movement", 1)
                                                                                                                                                                            }),
                                                                new TalkingNPC( new NPCStats("Rin Hoshira"), actor.TextureImage, new Vector2(400,200), new List<CommunicationData> {new CommunicationData("Hello There, How are you?"), new CommunicationData("I'm Fine")}),
                                                                new TalkingNPC( new NPCStats("Flynn Rivera"), actor.TextureImage, new Vector2(450,200), new List<CommunicationData> { new CommunicationData("Who Are You?"), new CommunicationData("Question Time!", "Who am I? Who are You?!", "I'm You!")})
                                                              
            });
        }
        private void LoadGameLevels(ContentManager Content)
        {
            #region Setup Variables
            viewportBounds.X = 4040;
            viewportBounds.Y = 1000;
            cutsceneActor.Position = new Vector2((float)viewportBounds.Width, cutsceneActor.Position.Y);
            #endregion

            Epolouge = new Level(0f, Content.Load<Map>("Maps/Epolouge/Epolouge"), viewportBounds, actor, Content.Load<Song>("Audio/Music/UrgentPianoWind"), Color.Navy, 0.5f); //- Epolouge.MaxCutsceneIDs = 3;
            emilysHouse = new Level(1.1f, Content.Load<Map>("Maps/Emilys House/House"), viewportBounds, actor, Content.Load<Song>("Audio/Music/Robins Song"), Color.White, 0.1f);
            marsCity = new Level(1f, Content.Load<Map>("Maps/Cities/City of Mars"), viewportBounds, actor, Content.Load<Song>("Audio/Music/Robins Song"), Color.White, 0.1f);

            Epolouge.AddZone(new ZoneChange(ref emilysHouse, new Rectangle(354 + 457, 1008 + 359, 36, 36), new Rectangle(0, 0, viewportBounds.Width, viewportBounds.Height), new Vector2(175, 144)));
            emilysHouse.AddZone(new ZoneChange(ref marsCity, new Rectangle(8 + 828, 12 + 551, 72, 36), new Rectangle(1678, 2494, viewportBounds.Width, viewportBounds.Height), new Vector2(500, 300)));
            marsCity.AddZone(new ZoneChange(ref emilysHouse, new Rectangle(1678 + 500, 2442 + 298, 36, 6), new Rectangle(8, 12, viewportBounds.Width, viewportBounds.Height), new Vector2(853, 516)));
            marsCity.AddZone(new ZoneChange(ref lakeSideTest, new Rectangle(0, 3322 + 299, 36, 72), new Rectangle(1700, 1744, viewportBounds.Width, viewportBounds.Height), new Vector2(900, 301)));
            lakeSideTest.AddZone(new ZoneChange(ref marsCity, new Rectangle(2650, 2006, 50, 63), new Rectangle(4, 3342, viewportBounds.Width, viewportBounds.Height), new Vector2(74, 297)));

            ///Test NPC's for Lynda to try out
            marsCity.AddNPC(new List<NPC> { new NPC(actor.TextureImage, new Vector2(350,1150), new List<MovementData> { 
                                                                                                                                                                            new MovementData("Run", 1),
                                                                                                                                                                            new MovementData("Move Right", 250),
                                                                                                                                                                            new MovementData("Move Down", 50),
                                                                                                                                                                            new MovementData("Move Left", 250),
                                                                                                                                                                            new MovementData("Move Up", 50)
                                                                                                                                                                            }),
                                                                new NPC(actor.TextureImage,  new Vector2(100,500), new List<MovementData> { 
                                                                                                                                                                            new MovementData("Walk", 1),
                                                                                                                                                                            new MovementData("Move Right", 50),
                                                                                                                                                                            new MovementData("Move Down", 100),
                                                                                                                                                                            new MovementData("Move Left", 50),
                                                                                                                                                                            new MovementData("Move Up", 100)
                                                                                                                                                                            }),
                                                                new NPC(actor.TextureImage,  new Vector2(300,500), new List<MovementData> { 
                                                                                                                                                                            new MovementData("Face Right", 50),
                                                                                                                                                                            new MovementData("Face Down", 50),
                                                                                                                                                                            new MovementData("Face Left", 50),
                                                                                                                                                                            new MovementData("Face Up", 50)
                                                                                                                                                                            }),
                                                                new NPC(actor.TextureImage,  new Vector2(1045,300), new List<MovementData> { 
                                                                                                                                                                            new MovementData("Move Down", 150),
                                                                                                                                                                            new MovementData("Stop Movement", 1)
                                                                                                                                                                            }),
                                                                new TalkingNPC( new NPCStats("Rin Hoshira"), actor.TextureImage, new Vector2(400,500), new List<CommunicationData> {new CommunicationData("Hello There, How are you?"), new CommunicationData("I'm Fine")}),
                                                                new TalkingNPC( new NPCStats("Flynn Rivera"), actor.TextureImage, new Vector2(450,500), new List<CommunicationData> { new CommunicationData("Who Are You?"), new CommunicationData("Question Time!", "Who am I? Who are You?!", "I'm You!")})
                                                              
            });

            Levels.Add(Epolouge);
            Levels.Add(emilysHouse);
            Levels.Add(marsCity);
            Levels.Add(lakeSideTest);
            Levels.Add(houseTest);
        }

        /// <summary>
        /// Used to reset LevelManager and PlayerManger data to a state of New Game or Load Game
        /// </summary>
        /// <param name="loadGame">If True, the game will go into the playerSaves and save a game</param>
        /// <param name="database">If True, the game will load from the database, if False, the game will load from a the file</param>
        public void StartGame(bool loadGame, bool database = true)
        {
            if (loadGame == false)
            {
                viewportBounds.X = 4040;
                viewportBounds.Y = 1000;
                cutsceneActor.Position = new Vector2((float)viewportBounds.Width, cutsceneActor.Position.Y);

                Epolouge.mapView.X = viewportBounds.X;
                Epolouge.mapView.Y = viewportBounds.Y;
                Epolouge.CutsceneID = 0;

                CurrentLevel = Epolouge;

                SetupCutscene();
            }
            else
            {
                #region Database Load
                //if (database == true)
                //{
                    if (Game1.saveGame.ReadFromDatabase())//;//"test", "test");
                    {
                        SaveData save = Game1.saveGame.save;

                        // Determine which level based on mapID
                        foreach (Level i in Levels)
                        {
                            Debug.WriteLine(save.mapID.ToString() + " | " + i.MapID);
                            if (i.MapID == save.mapID)
                            {
                                CurrentLevel = i;
                                break;
                            }
                            else { CurrentLevel = Epolouge; }
                        }

                        // Set variables
                        CurrentLevel.actor.Position = save.playerPosition;
                        CurrentLevel.mapView.X = (int)save.mapPosition.X - 0;
                        CurrentLevel.mapView.Y = (int)save.mapPosition.Y;
                        CurrentLevel.CutsceneID = (int)save.cutsceneID;
                        Debug.WriteLine(CurrentLevel.CutsceneID);

                        InCutscene = false;
                        levelState = LevelState.Playing;
                        cutsceneState = CutsceneState.Playing;
                    }
                    else { StartGame(false, true); return; }
                //}
                #endregion
                #region File Load
                //else
                //{
                //    while (true)
                //    {
                //        int _i = Game1.saveGame.LoadFile(0);
                //        if (_i == 1) 
                //        {
                //            SaveData save = Game1.saveGame.save;

                //            // Determine which level based on mapID
                //            foreach (Level i in Levels)
                //            {
                //                Debug.WriteLine(save.mapID.ToString() + " | " + i.MapID);
                //                if (i.MapID == save.mapID)
                //                {
                //                    CurrentLevel = i;
                //                    break;
                //                }
                //                else { CurrentLevel = Epolouge; }
                //            }

                //            // Set variables
                //            CurrentLevel.actor.Position = save.playerPosition;
                //            CurrentLevel.mapView.X = (int)save.mapPosition.X - 0;
                //            CurrentLevel.mapView.Y = (int)save.mapPosition.Y;
                //            CurrentLevel.CutsceneID = (int)save.cutsceneID;
                //            Debug.WriteLine(CurrentLevel.CutsceneID);

                //            InCutscene = false;
                //            levelState = LevelState.Playing;
                //            cutsceneState = CutsceneState.Playing;
                //            break; 
                //        }
                //        if (_i == -1) 
                //        { 
                //            StartGame(false, false); 
                //            return;
                //            break; 
                //        }
                //    }
                //}
                #endregion
            }
            MediaPlayer.Play(CurrentLevel.levelSong);
            MediaPlayer.Volume = 1f;
        }
        #endregion

        public void Update(GameTime gameTime, GraphicsDevice device)
        {
            keyState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            //Debug.WriteLine(CheckKey(Keys.Enter));
            if (CheckKey(Keys.Escape) || CheckButton(Buttons.Start))
            {
                Game1.gameState = GameState.Paused;
                Debug.WriteLine("Switching State: " + Game1.gameState);
            }

            switch (levelState)
            {
                case LevelState.Cutscene:
                    CurrentLevel.Update(gameTime, device);
                    PlayCutscene(gameTime);
                    //Debug.WriteLine(cutsceneActor.CurrentMovementData().movement + ": " + cutsceneActor.currentNumMovement + " | " + cutsceneActor.Position.ToString());
                    break;
                case (LevelState.Playing):
                    //Debug.WriteLine("In Playing");
                    CurrentLevel.Update(gameTime, device);
                    CheckForTalkingNPCs(gameTime);

                    //if (savectr >= 3000) { Game1.saveGame.SaveToDatabase(); savectr = 0; }
                    //else { savectr += gameTime.ElapsedGameTime.Milliseconds; }
                    CutsceneCheck();
                    break;

                case (LevelState.Talking):
                    TalkToNPCUpdate(gameTime);
                    break;
            }

            Game1.prevKeyState = keyState;
            Game1.prevPadState = gamePadState;

            AutomatedAISpawn(gameTime);

            if (levelTransfer) { LevelTransition(gameTime); }
            if (levelState == LevelState.Playing)
            {
                if (!levelTransfer) { CheckLevelTransition(); }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            CurrentLevel.Draw(gameTime, spriteBatch);

            switch (levelState)
            {
                case LevelState.Cutscene:
                    spriteBatch.Begin();
                    spriteBatch.Draw(blankTexture, new Rectangle(0, 0, viewportBounds.Width, viewportBounds.Height), Color.Black * currentTransparency);//fadeColour);
                    if (cutsceneState == CutsceneState.Talking) { communicationManager.Draw(spriteBatch); }
                    spriteBatch.End();
                    break;
                case LevelState.Playing:
                    spriteBatch.Begin();
                    spriteBatch.Draw(blankTexture, new Rectangle(0, 0, viewportBounds.Width, viewportBounds.Height), Color.Black * currentTransparency);//fadeColour);
                    spriteBatch.End();
                    break;
                case LevelState.Talking:
                    spriteBatch.Begin();
                    spriteBatch.Draw(blankTexture, new Rectangle(0, 0, viewportBounds.Width, viewportBounds.Height), Color.Black * currentTransparency);//fadeColour);
                    communicationManager.Draw(spriteBatch);
                    spriteBatch.End();
                    break;
            }

            spriteBatch.Begin();
            spriteBatch.End();
        }

        #region Cutscenes
        private void CutsceneCheck()
        {
            #region Epologoue
            if (CurrentLevel.MapID == 0f)
            {
                switch (CurrentLevel.CutsceneID)
                {
                    case 2:
                        if (CurrentLevel.mapView.X <= 354) //364 // 355
                        {
                            SetupCutscene();
                        }
                        break;
                }
            }
            #endregion
            #region Emilys House
            if (CurrentLevel.MapID == 1.1f)
            {
                switch (CurrentLevel.CutsceneID)
                {
                    case 0:
                        SetupCutscene();
                        break;
                }
            }
            #endregion
        }

        private void SetupCutscene()
        {
            levelState = LevelState.Cutscene;

            CurrentLevel.actor.Active = false;
            cutsceneActor.Active = true;

            cutsceneActor.directionSheetRow = actor.directionSheetRow;
            cutsceneActor.Position = actor.Position;

            cutsceneMovementCtr = 0;

            #region Level Specific Values
            #region Epolouge
            if (CurrentLevel.MapID == 0f)
            {
                switch (CurrentLevel.CutsceneID) //segmentIndex)
                {
                    case 0:
                        Epolouge.CutsceneID = 0;  //segmentIndex = 0;
                        CurrentLevel.CutsceneID = 0;
                        cutsceneActor.Position = new Vector2(actor.Position.X + (viewportBounds.Width / 2) + 25, actor.Position.Y);
                        cutsceneActor.ResetMovementData();
                        cutsceneActor.AddMovementData(new List<MovementData> { new MovementData("Idle", 200), new MovementData("Run", 1), new MovementData("Move Left", 150), new MovementData("Stop Movement", 1) });
                        break;
                    case 2:
                        cutsceneActor.ResetMovementData();
                        cutsceneActor.AddMovementData(new MovementData("Run", 1));
                        cutsceneActor.AddMovementData(new MovementData("Move Down Left", 15));
                        cutsceneActor.AddMovementData(new MovementData("Face Up", 1));
                        cutsceneActor.AddMovementData(new MovementData("Stop Movement", 1));
                        break;
                }
            }
            #endregion
            #region Emilys House
            if (CurrentLevel.MapID == 1.1f)
            {
                switch (CurrentLevel.CutsceneID)
                {
                    case 0:
                        emilysHouse.CutsceneID = 0;  //segmentIndex = 0;
                        CurrentLevel.CutsceneID = 0;
                        cutsceneActor.ResetMovementData();
                        //cutsceneActor.AddMovementData(new MovementData("Run", 1));
                        cutsceneActor.AddMovementData(new MovementData("Move Down", 1));
                        cutsceneActor.AddMovementData(new MovementData("Stop Movement", 1));
                        break;
                }
            }
            #endregion
            #endregion

            InCutscene = true;
        }

        private void PlayCutscene(GameTime gameTime)
        {
            #region Epologue
            if (CurrentLevel.MapID == 0f)
            {
                switch (CurrentLevel.CutsceneID) // segmentIndex)
                {
                    case 0:
                        if (cutsceneActor.Position.X <= 648)
                        {
                            //++Epolouge.CutsceneID;
                            ++CurrentLevel.CutsceneID; // segmentIndex;
                            cutsceneActor.Idle();
                            communicationManager.NPCstats = new NPCStats(actor.Name);
                            communicationManager.commData = new CommunicationData("I need to run!");
                            cutsceneState = CutsceneState.Talking;
                            cutsceneActor.ResetMovementData();
                            cutsceneActor.AddMovementData(new MovementData("Idle", 1));
                            Debug.WriteLine("Current Cutscene: " + CurrentLevel.CutsceneID);
                        }
                        break;
                    case 1:
                        switch (cutsceneState)
                        {
                            case CutsceneState.Playing:
                                //++Epolouge.CutsceneID;
                                ++CurrentLevel.CutsceneID; // segmentIndex;
                                Debug.WriteLine("Current Cutscene: " + CurrentLevel.CutsceneID);
                                EndCutScene();
                                break;
                            case CutsceneState.Talking:
                                cutsceneTimer += (float)gameTime.ElapsedGameTime.Milliseconds;
                                if (cutsceneTimer >= 750) { cutsceneState = CutsceneState.Playing; cutsceneTimer = 0f; }

                                break;
                        }
                        break;
                    case 2:
                        //++Epolouge.CutsceneID;
                        ++cutsceneMovementCtr;
                        if (cutsceneMovementCtr <= 18) { cutsceneActor.Position = new Vector2(cutsceneActor.Position.X, cutsceneActor.Position.Y + 1); } //22 //44
                        else if (cutsceneMovementCtr <= 42 && cutsceneMovementCtr >= 30) { cutsceneActor.Position = new Vector2(cutsceneActor.Position.X, cutsceneActor.Position.Y + 3); } //65
                        else if (cutsceneMovementCtr <= 86)
                        {
                            ++CurrentLevel.CutsceneID;

                            cutsceneActor.Idle();
                            communicationManager.NPCstats = new NPCStats(actor.Name);
                            communicationManager.commData = new CommunicationData("Ah...aaaaa..");
                            cutsceneState = CutsceneState.Talking;
                            cutsceneActor.ResetMovementData();
                            cutsceneActor.AddMovementData(new MovementData("Idle", 45));
                            cutsceneActor.AddMovementData(new MovementData("Face Down", 1));
                            Debug.WriteLine("Current Cutscene: " + CurrentLevel.CutsceneID);
                        }
                        /// 1052
                        /// 1008
                        break;
                    case 3:
                        switch (cutsceneState)
                        {
                            case CutsceneState.Playing:
                                //++Epolouge.CutsceneID;
                                ++CurrentLevel.CutsceneID; // segmentIndex;
                                Debug.WriteLine("Current Cutscene: " + CurrentLevel.CutsceneID);
                                EndCutScene();
                                break;
                            case CutsceneState.Talking:
                                cutsceneTimer += (float)gameTime.ElapsedGameTime.Milliseconds;
                                if (cutsceneTimer >= 750) { cutsceneState = CutsceneState.Playing; cutsceneTimer = 0f; }
                                break;
                        }
                        break;
                }
            }
            #endregion
            #region Emilys House
            if (CurrentLevel.MapID == 1.1f)
            {
                switch (CurrentLevel.CutsceneID)
                {
                    case 0:
                        //++emilysHouse.CutsceneID;
                        ++CurrentLevel.CutsceneID; // segmentIndex;
                        cutsceneActor.Idle();
                        communicationManager.NPCstats = new NPCStats(actor.Name);
                        communicationManager.commData = new CommunicationData("AH!");
                        cutsceneState = CutsceneState.Talking;
                        cutsceneActor.ResetMovementData();
                        cutsceneActor.AddMovementData(new MovementData("Idle", 1));
                        Debug.WriteLine("Current Cutscene: " + CurrentLevel.CutsceneID);
                        break;
                    case 1:
                        switch (cutsceneState)
                        {
                            case CutsceneState.Playing:
                                //++emilysHouse.CutsceneID;
                                ++CurrentLevel.CutsceneID; // segmentIndex;
                                Debug.WriteLine("Current Cutscene: " + CurrentLevel.CutsceneID);
                                EndCutScene();
                                break;
                            case CutsceneState.Talking:
                                cutsceneTimer += (float)gameTime.ElapsedGameTime.Milliseconds;
                                if (cutsceneTimer >= 1000 && cutsceneTimer <= 1999) { communicationManager.commData = new CommunicationData("..."); }
                                if (cutsceneTimer >= 2000 && cutsceneTimer <= 2999) { communicationManager.commData = new CommunicationData("Phew, It was just a dream."); }
                                if (cutsceneTimer >= 2100 && cutsceneTimer <= 2200) { alarmClock1.Play(); }
                                if (cutsceneTimer >= 2900 && cutsceneTimer <= 3000) { alarmClock1.Play(); }
                                if (cutsceneTimer >= 3000 && cutsceneTimer <= 3999) { communicationManager.commData = new CommunicationData("..."); }
                                if (cutsceneTimer >= 3500 && cutsceneTimer <= 3600) { alarmClock1.Play(); }
                                if (cutsceneTimer >= 4000 && cutsceneTimer <= 4999) { communicationManager.commData = new CommunicationData("*sigh* Time to go."); }
                                if (cutsceneTimer >= 5000 && cutsceneTimer <= 5999) { cutsceneState = CutsceneState.Playing; cutsceneTimer = 0f; }
                                break;
                        }
                        break;
                }
            }
            #endregion
        }

        private void EndCutScene()
        {
            levelState = LevelState.Playing;
            CurrentLevel.actor.Active = true;
            cutsceneActor.Active = false;

            actor.directionSheetRow = cutsceneActor.directionSheetRow;
            actor.Position = cutsceneActor.Position;

            //-----------------------------------------
            // Do Last Minute Code Updates Here
            //----------------------------------------

            //-----------------------------------------
            //
            //----------------------------------------
            Debug.WriteLine("Current Cutscene: " + CurrentLevel.CutsceneID);

            InCutscene = false;
        }
        #endregion

        #region Communications
        private void TalkToNPCUpdate(GameTime gameTime)
        {
            //keyState = Keyboard.GetState();
            //gamePadState = GamePad.GetState(PlayerIndex.One);

            if (CheckButton(Buttons.A))
            {
                List<TalkingNPC> talkingNPCs = new List<TalkingNPC> { };
                foreach (NPC i in CurrentLevel.GetNPCs())
                {
                    if (i is TalkingNPC)
                    {
                        talkingNPCs.Add((TalkingNPC)(i));
                    }
                }

                foreach (TalkingNPC i in talkingNPCs)
                {
                    if (actor.CollisionRectangle.Intersects(i.CollisionRectangle))
                    {
                        if (i.IncrimentChatIndex())
                        {
                            communicationManager.commData = i.GetCurrentCommunication();
                            communicationManager.NPCstats = i.stats;
                        }
                        else
                        {
                            levelState = LevelState.Playing;
                        }
                    }
                }
            }
            else if (keyState.IsKeyDown(Keys.Space))
            {
                if (!Game1.prevKeyState.IsKeyDown(Keys.Space))
                {
                    List<TalkingNPC> talkingNPCs = new List<TalkingNPC> { };
                    foreach (NPC i in CurrentLevel.GetNPCs())
                    {
                        if (i is TalkingNPC)
                        {
                            talkingNPCs.Add((TalkingNPC)(i));
                        }
                    }

                    foreach (TalkingNPC i in talkingNPCs)
                    {
                        if (actor.CollisionRectangle.Intersects(i.CollisionRectangle))
                        {
                            if (i.IncrimentChatIndex())
                            {
                                communicationManager.commData = i.GetCurrentCommunication();
                                communicationManager.NPCstats = i.stats;
                            }
                            else
                            {
                                levelState = LevelState.Playing;
                            }
                        }
                    }
                }
            }

            //Game1.prevPadState = gamePadState;
            //Game1.prevKeyState = keyState; 
        }

        private void CheckForTalkingNPCs(GameTime gameTime)
        {
            //keyState = Keyboard.GetState();
            //gamePadState = GamePad.GetState(PlayerIndex.One);
            if (CheckButton(Buttons.A))
            {
                List<TalkingNPC> talkingNPCs = new List<TalkingNPC> { };
                foreach (NPC i in CurrentLevel.GetNPCs())
                {
                    if (i is TalkingNPC)
                    {
                        talkingNPCs.Add((TalkingNPC)(i));
                    }
                }

                foreach (TalkingNPC i in talkingNPCs)
                {
                    if (actor.CollisionRectangle.Intersects(i.CollisionRectangle))
                    {
                        communicationManager.commData = i.GetCurrentCommunication();
                        communicationManager.NPCstats = i.stats;
                        levelState = LevelState.Talking;
                        break;
                    }
                }
            }
            if (keyState.IsKeyDown(Keys.Space))
            {
                if (!Game1.prevKeyState.IsKeyDown(Keys.Space))
                {
                    List<TalkingNPC> talkingNPCs = new List<TalkingNPC> { };
                    foreach (NPC i in CurrentLevel.GetNPCs())
                    {
                        if (i is TalkingNPC)
                        {
                            talkingNPCs.Add((TalkingNPC)(i));
                        }
                    }

                    foreach (TalkingNPC i in talkingNPCs)
                    {
                        if (actor.CollisionRectangle.Intersects(i.CollisionRectangle))
                        {
                            communicationManager.commData = i.GetCurrentCommunication();
                            communicationManager.NPCstats = i.stats;
                            levelState = LevelState.Talking;
                            break;
                        }
                    }
                }
            }

            //Game1.prevPadState = gamePadState;
            //Game1.prevKeyState = keyState;  
        }
        #endregion

        #region Level Transition
        private void LevelTransition(GameTime gameTime)
        {
            if (sceneIsFading)
            {
                currentTransparency += TRANSPARENCY_RATE_OF_CHANGE * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                MediaPlayer.Volume -= TRANSPARENCY_RATE_OF_CHANGE * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (currentTransparency >= 1)
                {
                    CurrentLevel = zoneChange.NewZone;
                    CurrentLevel.mapView = zoneChange.NewMapView;
                    CurrentLevel.actor.Position = zoneChange.PlayerLocation;
                    CurrentLevel.PlayLevelSong();
                    sceneIsFading = false;
                    sceneIsUnFading = true;
                }
            }
            if (sceneIsUnFading)
            {
                currentTransparency -= TRANSPARENCY_RATE_OF_CHANGE * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                MediaPlayer.Volume += TRANSPARENCY_RATE_OF_CHANGE * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (currentTransparency <= 0)
                {
                    currentTransparency = 0;
                    sceneIsFading = false;
                    sceneIsUnFading = false;
                    levelTransfer = false;
                }
            }
        }

        public void FadeToMenu(GameTime gameTime)
        {
            currentTransparency += TRANSPARENCY_RATE_OF_CHANGE * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            MediaPlayer.Volume -= TRANSPARENCY_RATE_OF_CHANGE * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (currentTransparency >= 1)
            {
                Game1.gameState = GameState.MainMenu;
                levelTransfer = true;
                sceneIsFading = false;
                sceneIsUnFading = true;
                Game1.pauseMenu.transferToMainMenu = false;
            }
        }

        private void CheckLevelTransition()
        {
            zoneChange = null;
            Rectangle playRect = CurrentLevel.actor.CollisionRectangle;
            playRect.X += CurrentLevel.mapView.X;
            playRect.Y += CurrentLevel.mapView.Y;

            foreach (ZoneChange z in CurrentLevel.GetZones())
            {
                if (z.CurrentMapLocation.Intersects(playRect))
                {
                    zoneChange = z;
                    break;
                }
            }

            if (zoneChange != null)
            {
                levelTransfer = true;
                sceneIsFading = true;
            }
        }

        private void CheckCutsceneLevelTransition(Rectangle transitionRectangle)
        {
            zoneChange = null;
            Rectangle playRect = CurrentLevel.actor.CollisionRectangle;
            playRect.X += CurrentLevel.mapView.X;
            playRect.Y += CurrentLevel.mapView.Y;

            foreach (ZoneChange z in CurrentLevel.GetZones())
            {
                if (z.CurrentMapLocation.Intersects(playRect))
                {
                    zoneChange = z;
                    break;
                }
            }

            if (zoneChange != null)
            {
                levelTransfer = true;
                sceneIsFading = true;
            }
        }
        #endregion

        #region Helper Methods
        protected bool CheckKey(Keys key)
        {
            return (keyState.IsKeyUp(key) && Game1.prevKeyState.IsKeyDown(key));
        }
        protected bool CheckButton(Buttons button)
        {
            //Debug.WriteLine(button.ToString());
            return (gamePadState.IsButtonDown(button) && Game1.prevPadState.IsButtonUp(button));
        }
        #endregion

        private void AutomatedAISpawn(GameTime gameTime)
        {
            if (CurrentLevel.MapID == 0f)
            {
                if (animalSpawnCtr >= 25)
                {
                    animalSpawnCtr = 0;
                    int rand = Game1.random.Next(0, 2); // new NPC((rand == 1) ? pigeon : seagull, new Vector2(CurrentLevel.map.Bounds.Width+32, Game1.random.Next(0, CurrentLevel.map.Bounds.Height - 32))
                    CurrentLevel.AddFlyingNPC(new NPC((rand == 1) ? pigeon : seagull, new Vector2(CurrentLevel.map.Bounds.Width + 10, Game1.random.Next(700, CurrentLevel.map.Bounds.Height - 500)), new List<MovementData> { new MovementData("Move Left", 1) }), CurrentLevel.mapView);
                }
                else { ++animalSpawnCtr; }
            }
        }

    }
}
