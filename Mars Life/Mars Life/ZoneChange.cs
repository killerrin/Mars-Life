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
    public class ZoneChange
    {
        public Level NewZone {get; private set;}
        public Rectangle CurrentMapLocation {get; private set;}
        public Rectangle NewMapView { get; private set; }
        public Vector2 PlayerLocation { get; private set; }

        /// <summary>
        /// The main Constructor for ZoneChange
        /// </summary>
        /// <param name="level">The reference of the level that the zone goes to</param>
        /// <param name="location">The location on the current map that the zone transfer point is at</param>
        /// <param name="mapView">Where on the new map that the map should center</param>
        /// <param name="actorLocation">Where on the new map that the player should go</param>
        public ZoneChange(ref Level level, Rectangle location, Rectangle mapView, Vector2 actorLocation)
        {
            NewZone = level;
            CurrentMapLocation = location;
            NewMapView = mapView;
            PlayerLocation = actorLocation;
        }

        /// <summary>
        /// The overloaded Constructor for ZoneChange
        /// </summary>
        /// <param name="level">The reference of the level that the zone goes to</param>
        /// <param name="tileLocation">The location and size, in tiles, of where the zone transfer point is on the current map</param>
        /// <param name="mapView">Where on the new map that the map should center</param>
        /// <param name="actorLocation">Where on the new map that the player should go</param>
        public ZoneChange(ref Level level, Vector4 tileLocation, Rectangle mapView, Vector2 actorPosition)
        {
            NewZone = level;
            CurrentMapLocation = new Rectangle((int)(tileLocation.X * NewZone.map.TileWidth),
                                                                   (int)(tileLocation.Y * NewZone.map.TileHeight),
                                                                   (int)(tileLocation.Z * NewZone.map.TileWidth),
                                                                   (int)(tileLocation.W * NewZone.map.TileHeight));
            NewMapView = mapView;
            PlayerLocation = actorPosition;
        }
    }
}
