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

namespace Mars_Life
{
    public class Chest
    {
        private Texture2D closed;
        private Texture2D opened;
        public Rectangle mapView { private set; get; }
        private bool ChestOpened;

        public Chest (Texture2D cclosed, Texture2D oopened, Rectangle llocation, bool closedOpened)
        {
            closed = cclosed;
            opened = oopened;
            ChestOpened = closedOpened;
            mapView = llocation;
         }
        public Chest (Texture2D cclosed, Texture2D oopened, Vector2 tileLocation, bool closedOpened)
        {
            closed = cclosed;
            opened = oopened;
            ChestOpened = closedOpened;

            mapView = new Rectangle((int)tileLocation.X, (int)tileLocation.Y, (int)(closedOpened ? opened.Width : closed.Width), (int)(closedOpened ? opened.Height : closed.Height));
        }

        public void OpenChest()
        {
            if (ChestOpened) { return; }
            else
            {
                ChestOpened = true;
                //Insert code to put items into inventory
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (ChestOpened) { spriteBatch.Draw(closed, mapView, Color.White); }
            else { spriteBatch.Draw(opened, mapView, Color.White); }
        }
    }
}
