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

namespace Mars_Life
{
    public class Level
    {
        public float MapID { get; private set; }
        public int CutsceneID;
        public int MaxCutsceneIDS;

        public Map map {get; private set;}
        public Rectangle mapView;// { get; private set; }
        public Player actor;// { get; private set; }
        public Color overlayColor;
        public float overlayColorOpacity;

        public Song levelSong;

        private List<ZoneChange> zones;
        private List<Chest> chests;
        private List<NPC> npcs;
        private List<NPC> flyingNPCs;

        private bool ableToMoveLeft;
        private bool ableToMoveRight;
        private bool ableToMoveUp;
        private bool ableToMoveDown;

        //-- Time of Day Settings
        private const float MAX_NIGHTTIME_TRANSPARANCY = 0.65f;

        //-- Zones --\\
        public void AddZone(ZoneChange zone)
        {
            zones.Add(zone);
        }
        public List<ZoneChange> GetZones()
        {
            return (zones);
        }

        //-- Chests --\\
        public void AddChest(Chest chest)
        {
            chests.Add(chest);
        }
        public List<Chest> GetChests()
        {
            return (chests);
        }

        //-- NPC's --\\
        public void AddFlyingNPC(NPC bird)
        {
            flyingNPCs.Add(bird);
        }
        public void AddNPC(NPC npc)
        {
            npcs.Add(npc);
        }
        public void AddNPC(NPC npc, Rectangle rect)
        {
            NPC Npc = npc;
            Npc.Position = new Vector2(Npc.Position.X - rect.X, Npc.Position.Y - rect.Y);
            npcs.Add(Npc);
        }
        public void AddFlyingNPC(NPC bird, Rectangle rect)
        {
            NPC Npc = bird;
            Npc.Position = new Vector2(Npc.Position.X - rect.X, Npc.Position.Y - rect.Y);
            flyingNPCs.Add(Npc);
        }
        public void AddNPC(List<NPC> npcList)
        {
            foreach (NPC i in npcList)
            {
                npcs.Add(i);
            }
        }
        public List<NPC> GetNPCs()
        {
            return (npcs);
        }
        public List<NPC> GetFlyingNPCs()
        {
            return (flyingNPCs);
        }

        public Level(float mapID, Map mmap, Rectangle view, Player aactor, Song song)
        {
            MapID = mapID;
            CutsceneID = 0;
            MaxCutsceneIDS = 0;
            
            map = mmap;

            overlayColor = Color.Black;
            overlayColorOpacity = 0.0f;

            mapView = view;
            //mapView.X = (view.Width / 2);
            //mapView.Y = (view.Height / 2);// +50;
            actor = aactor;

            levelSong = song;

            zones = new List<ZoneChange> { };
            chests = new List<Chest> { };
            npcs = new List<NPC> { };
            flyingNPCs = new List<NPC> { };
        }

        public Level(float mapID, Map mmap, Rectangle view, Player aactor, Song song,  Color _overlayColor, float _overlayColorOpacity)
        {
            MapID = mapID;
            CutsceneID = 0;
            MaxCutsceneIDS = 0;

            map = mmap;

            levelSong = song;

            overlayColor = _overlayColor;
            overlayColorOpacity = _overlayColorOpacity;

            mapView = view;
            //mapView.X = (view.Width / 2);
            //mapView.Y = (view.Height / 2);// +50;
            actor = aactor;

            zones = new List<ZoneChange> { };
            chests = new List<Chest> { };
            npcs = new List<NPC> { };
            flyingNPCs = new List<NPC> { };
        }

        public void Update(GameTime gameTime, GraphicsDevice device)
        {
            CheckMovementCollisions();

            ScrollCamera(gameTime);

            foreach (NPC i in npcs)
            {
                i.Update(gameTime);
            }
            foreach (NPC i in flyingNPCs)
            {
                i.Update(gameTime);
            }

            switch (Game1.levelManager.InCutscene)
            {
                case false:
                    actor.Active = true; Game1.levelManager.cutsceneActor.Active = false;
                    actor.Update(gameTime, device);
                    break;
                case true:
                    actor.Active = false; Game1.levelManager.cutsceneActor.Active = true;
                    Game1.levelManager.cutsceneActor.Update(gameTime);
                    break;
            }
        }


        private void CheckMovementCollisions()
        {
            ableToMoveLeft = true;
            ableToMoveRight = true;
            ableToMoveUp = true;
            ableToMoveDown = true;

            int collisionLayer = 0;
            int widHei = 2;

            Rectangle leftOfPlayer = new Rectangle((int)(mapView.X + actor.Position.X + (actor.SpriteWidth / 1.2)) - map.TileWidth,
                                                                       (int)(mapView.Y + actor.Position.Y) + (actor.SpriteHeight / 2),// - map.TileHeight,
                                                                       (int)widHei,//map.TileWidth,
                                                                       (int)widHei);//map.TileHeight);
            Rectangle rightOfPlayer = new Rectangle((int)(mapView.X + actor.Position.X - (actor.SpriteWidth / 1.1)) + map.TileWidth,
                                                                        (int)(mapView.Y + actor.Position.Y) + (actor.SpriteHeight / 2),
                                                                        (int)widHei,//map.TileWidth,
                                                                        (int)widHei);//map.TileHeight);
            Rectangle abovePlayer = new Rectangle((int)(mapView.X + actor.Position.X),
                                                                       (int)(mapView.Y + actor.Position.Y + (actor.SpriteHeight / 0.8)) - map.TileHeight,// / 2),
                                                                       (int)widHei,//map.TileWidth,
                                                                       (int)widHei);//map.TileHeight);
            Rectangle belowPlayer = new Rectangle((int)(mapView.X + actor.Position.X),
                                                                       (int)(mapView.Y + actor.Position.Y - actor.SpriteHeight) + map.TileHeight + (actor.SpriteHeight / 2) - 2,
                                                                       (int)widHei,//map.TileWidth,
                                                                       (int)widHei);//map.TileHeight);


            // Do Collision Code Here using "map.GetTilesInRegion (Int32 tileLayerID, Rectangle region)" 
            // followed by parsing the returned collection and setting the booleans accordingly
            IEnumerable<TileData> tiledataCollection;
            //List<TileData> tileData;

            tiledataCollection = map.GetTilesInRegion(collisionLayer, leftOfPlayer);
            if (tiledataCollection != null)
            {
                //tileData = tiledataCollection.ToList();
                //map.Tilesets[collisionLayer].Texture
                if (tiledataCollection != null && tiledataCollection.Count() > 0) { ableToMoveLeft = false; }
            }

            tiledataCollection = map.GetTilesInRegion(collisionLayer, rightOfPlayer);
            if (tiledataCollection != null)
            {
                //tileData = tiledataCollection.ToList();
                if (tiledataCollection != null && tiledataCollection.Count() > 0) { ableToMoveRight = false; }
            }

            tiledataCollection = map.GetTilesInRegion(collisionLayer, abovePlayer);
            if (tiledataCollection != null)
            {
                //tileData = tiledataCollection.ToList();
                if (tiledataCollection != null && tiledataCollection.Count() > 0) { ableToMoveUp = false; }
            }

            tiledataCollection = map.GetTilesInRegion(collisionLayer, belowPlayer);
            if (tiledataCollection != null)
            {
                //tileData = tiledataCollection.ToList();
                if (tiledataCollection != null && tiledataCollection.Count() > 0) { ableToMoveDown = false; }
            }
        }

        /// <Summary>
        /// This method is used to scroll the camera in relation to Player and Map inputs.
        /// </Summary>
        private void ScrollCamera(GameTime gameTime)
        {
            KeyboardState keys = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            Rectangle delta = mapView;
            Vector2 deltaObjectOffset = Vector2.Zero;
            int scrollSpeed = Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 4);
            //float actorSpeed = scrollSpeed / 2f;

            switch (Game1.levelManager.InCutscene)
            {
                case false:
                    #region Normal Actor Movement
                    if (keys.IsKeyDown(Keys.LeftShift) || keys.IsKeyDown(Keys.RightShift) || gamePadState.Triggers.Left > 0.5f)
                    {
                        actor.Run();
                    }
                    else { actor.Walk(); }

                    if (keys.IsKeyDown(Keys.S) || gamePadState.ThumbSticks.Left.Y < -0.5f)
                    {
                        if (ableToMoveDown)
                        {
                            if (actor.Position.Y <= (delta.Height / 2)) // 300
                            {
                                actor.Down();
                            }
                            else if (delta.Y >= (map.Bounds.Height - delta.Height))
                            {
                                actor.Down();
                            }
                            else
                            {
                                delta.Y += scrollSpeed;
                                deltaObjectOffset.Y -= scrollSpeed;
                                if (actor.runWalk) { delta.Y += (scrollSpeed / 2); deltaObjectOffset.Y -= (scrollSpeed / 2); }
                            }
                        }
                    }

                    if (keys.IsKeyDown(Keys.W) || gamePadState.ThumbSticks.Left.Y > +0.5f)
                    {
                        if (ableToMoveUp)
                        {
                            if (actor.Position.Y >= (delta.Height / 2)) // 300
                            {
                                actor.Up();
                            }
                            else if (delta.Y <= 5)
                            {
                                actor.Up();
                            }
                            else
                            {
                                delta.Y -= scrollSpeed;
                                deltaObjectOffset.Y += scrollSpeed;
                                if (actor.runWalk) { delta.Y -= (scrollSpeed / 2); deltaObjectOffset.Y += (scrollSpeed / 2); }

                            }
                        }
                    }

                    if (keys.IsKeyDown(Keys.D) || gamePadState.ThumbSticks.Left.X > +0.5f)
                    {
                        if (ableToMoveRight)
                        {
                            if (actor.Position.X <= (delta.Width / 2)) // 400
                            {
                                actor.Right();
                            }
                            else if (delta.X >= (map.Bounds.Width - delta.Width))
                            {
                                actor.Right();
                            }
                            else
                            {
                                delta.X += scrollSpeed;
                                deltaObjectOffset.X -= scrollSpeed;
                                if (actor.runWalk) { delta.X += (scrollSpeed / 2); deltaObjectOffset.X -= (scrollSpeed / 2); }
                            }
                        }
                    }

                    if (keys.IsKeyDown(Keys.A) || gamePadState.ThumbSticks.Left.X < -0.5f)
                    {
                        if (ableToMoveLeft)
                        {
                            if (actor.Position.X >= (delta.Width / 2)) // 400
                            {
                                actor.Left();
                            }
                            else if (delta.X <= 5)
                            {
                                actor.Left();
                            }
                            else// (actor.Position.X <= 400)
                            {
                                delta.X -= scrollSpeed;
                                deltaObjectOffset.X += scrollSpeed;
                                if (actor.runWalk) { delta.X -= (scrollSpeed / 2); deltaObjectOffset.X += (scrollSpeed / 2); }
                            }
                        }
                    }
                    break;
                    #endregion
                case true:
                    #region Cutscene Actor Movement
                    #endregion
                    break;
            }

            if (map.Bounds.Contains(delta))
            {
                //mapView.X = (int)actor.Position.X + (mapView.Width/2) ;
                //mapView.Y = (int)actor.Position.Y *2;
                mapView = delta;

                foreach (NPC i in npcs)
                {
                    i.MoveMap(deltaObjectOffset);
                }
                foreach (NPC i in flyingNPCs)
                {
                    i.MoveMap(deltaObjectOffset);
                }

                actor.overrideMovement = false;
            }
            else
            {
                actor.overrideMovement = true;

                if (mapView.X == (map.Bounds.Width - mapView.Width) - 2) { mapView.X += 2; }
                if (mapView.X == 2) { mapView.X = 0; }
                //if (mapView.X == 1698) { mapView.X += 2; }
                if (actor.Position.X == 502f) { actor.Position = new Vector2(actor.Position.X - 2, actor.Position.Y); }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
                //map.Draw(spriteBatch, mapView);
                map.DrawLayer(spriteBatch, 1, mapView, 0f); //-- Draw Ground Layer 1
                map.DrawLayer(spriteBatch, 2, mapView, 0f); //-- Draw Ground Layer 2
                map.DrawLayer(spriteBatch, 3, mapView, 0f); //-- Draw Ground Layer 3

                foreach (NPC i in npcs)
                {
                    i.Draw(gameTime, spriteBatch);
                }
                
                switch (Game1.levelManager.InCutscene)
                {
                    case false:
                        actor.Draw(gameTime, spriteBatch); //-- Draw the player
                        break;
                    case true:
                        Game1.levelManager.cutsceneActor.Draw(gameTime, spriteBatch);
                        break;
                }


                map.DrawLayer(spriteBatch, 4, mapView, 0f); //-- Draw Above Player
                map.DrawLayer(spriteBatch, 5, mapView, 0f); //-- Draw Shadows
                foreach (NPC i in flyingNPCs)
                {
                    i.Draw(gameTime, spriteBatch);
                }
                spriteBatch.Draw(Game1.blankTexture, map.Bounds, overlayColor * overlayColorOpacity);
            spriteBatch.End();
        }

        public void PlayLevelSong()
        {
            if (levelSong != null)
            {
                MediaPlayer.Play(levelSong);
            }
        }
    }
}
