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
    public class CommunicationManager
    {
        private GraphicsDevice Graphics;
        
        public Texture2D communicationBackground;
        private SpriteFont font;
        private Color fontColour;

        private Rectangle mapView;
        private Rectangle drawView;

        private Vector2 DrawOffsets;
        //{
        //    get { return DrawOffsets; }
        //    set
        //    {
        //        DrawOffsets = value;
        //        drawView = new Rectangle(mapView.X + (int)DrawOffsets.X, (int)DrawOffsets.Y, mapView.Width - (int)DrawOffsets.X, (int)DrawOffsets.Y);
        //    }
        //}

        public CommunicationData commData;
        public NPCStats NPCstats;

        public CommunicationManager(ContentManager Content, GraphicsDevice _Graphics, Vector2 _drawOffsets)
        {
            Graphics = _Graphics;
            communicationBackground = Content.Load<Texture2D>("Images/Other/ChatBackground");
            font = Game1.segoe16Font;
            fontColour = Color.Wheat;
            mapView = _Graphics.Viewport.Bounds;
            DrawOffsets = new Vector2(_drawOffsets.X, (float)mapView.Height - _drawOffsets.Y);
            drawView = new Rectangle((mapView.X + (int)DrawOffsets.X), (int)DrawOffsets.Y, (mapView.Width - (((int)DrawOffsets.X))*2), (int)DrawOffsets.Y);
        }

        public void Update(GameTime gameTime)
        {
        
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(communicationBackground, drawView, Color.White);
            int offset = 25;
            if (commData.type == "Message") 
            {
                RenderText.Draw(spriteBatch, NPCstats.Name, new Vector2(drawView.X + (offset/2), drawView.Y-offset-3), fontColour, font); 
                RenderText.Draw(spriteBatch, "\n" + commData.dialogue, new Vector2(drawView.X + offset, drawView.Y), fontColour, font); 
            }
            if (commData.type == "Question")
            {
                RenderText.Draw(spriteBatch, NPCstats.Name, new Vector2(drawView.X + (offset / 2), drawView.Y - offset-3), fontColour, font); 
                RenderText.Draw(spriteBatch, "\n"+commData.dialogue, new Vector2(MeasureMenu(commData.dialogue), drawView.Y), fontColour, font);
                RenderText.Draw(spriteBatch, "\n\n" + commData.option1 + "\n" + commData.option2, new Vector2(drawView.X + (int)(offset*1.5), drawView.Y + (offset / 1)), fontColour, font);
            }
        }

        public float MeasureMenu(string msg)
        {
            float width = 0f;
            Vector2 size = font.MeasureString(msg);
            width = size.X;
            return((mapView.Width - width) / 2f);
        }

    }
}
