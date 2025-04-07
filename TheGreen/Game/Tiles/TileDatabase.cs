using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TheGreen.Game.Tiles
{
    /// <summary>
    /// Holds tile information that would be considered redundant and memory inefficient in the Tile struct. 
    /// Reference this to retrieve properties of a tile type, and methods for drawing and updating a specific tile type
    /// </summary>
    public static class TileDatabase { 
        
        
        /// <summary>
        /// List of static tile type information
        /// Use & on TileProperties to check if it has a property.
        /// e.x. Dirt: _tileProperties[1].TileProperties & Solid; returns true
        /// </summary>
        private static readonly TileData[] _tileProperties = [
            new TileData(0, TileProperty.None, Color.CornflowerBlue),                     //Air
            new TileData(1, TileProperty.Solid | TileProperty.PickaxeMineable, Color.Brown, itemID: 0, health: 40),           //Dirt
            new TileData(2, TileProperty.Solid | TileProperty.Overlay | TileProperty.PickaxeMineable, Color.Green, itemID: 0, health: 60, baseTileID: 1),           //Grass
            new TileData(3, TileProperty.Solid | TileProperty.PickaxeMineable, Color.Gray, itemID: 1, health : 100), //CobbleStone
            new TileData(4, TileProperty.Solid | TileProperty.PickaxeMineable, Color.Gray, itemID: 1, health: 100),  //Stone
            new TreeData(5, TileProperty.AxeMineable, Color.Brown, health: 80),    //Tree
            new TreeTopData(6, TileProperty.AxeMineable, Color.Green, offset: new Vector2(-48, -152)), //TreeTop
            new TorchData(7, TileProperty.LightEmitting | TileProperty.PickaxeMineable, Color.Yellow, itemID: 3),  //Torch
            new InventoryTileData(8, TileProperty.PickaxeMineable | TileProperty.LargeTile, Color.Brown, itemID: 5, cols: 5, rows: 3)
            ];
        private static Rectangle CreateAtlasRect(int x, int y)
        {
            return new Rectangle(x * Globals.TILESIZE, y * Globals.TILESIZE, Globals.TILESIZE, Globals.TILESIZE);
        }
        /// <summary>
        /// Stores the texture atlas coords of a standard tile with the give tile state.
        /// </summary>
        private static Dictionary<byte, Rectangle> _textureAtlasRects = new Dictionary<byte, Rectangle>()
        {
            {0, CreateAtlasRect(0,0)}, {2, CreateAtlasRect(1,0)}, {8, CreateAtlasRect(2,0)}, {10, CreateAtlasRect(3,0)}, {14, CreateAtlasRect(4,0)}, {32, CreateAtlasRect(5,0)},
            {34, CreateAtlasRect(0,1)}, {40, CreateAtlasRect(1,1)}, {42, CreateAtlasRect(2,1)}, {46, CreateAtlasRect(3,1)}, {56, CreateAtlasRect(4,1)}, {58, CreateAtlasRect(5,1)},
            {62, CreateAtlasRect(0,2)}, {128, CreateAtlasRect(1,2)}, {130, CreateAtlasRect(2,2)}, {131, CreateAtlasRect(3,2)}, {136, CreateAtlasRect(4,2)}, {138, CreateAtlasRect(5,2)},
            {139, CreateAtlasRect(0,3)}, {142, CreateAtlasRect(1,3)}, {143, CreateAtlasRect(2,3)}, {160, CreateAtlasRect(3,3)}, {162, CreateAtlasRect(4,3)}, {163, CreateAtlasRect(5,3)},
            {168, CreateAtlasRect(0,4)}, {170, CreateAtlasRect(1,4)}, {171, CreateAtlasRect(2,4)}, {174, CreateAtlasRect(3,4)}, {175, CreateAtlasRect(4,4)}, {184, CreateAtlasRect(5,4)},
            {186, CreateAtlasRect(0,5)}, {187, CreateAtlasRect(1,5)}, {190, CreateAtlasRect(2,5)}, {191, CreateAtlasRect(3,5)}, {224, CreateAtlasRect(4,5)}, {226, CreateAtlasRect(5,5)},
            {227, CreateAtlasRect(0,6)}, {232, CreateAtlasRect(1,6)}, {234, CreateAtlasRect(2,6)}, {235, CreateAtlasRect(3,6)}, {238, CreateAtlasRect(4,6)}, {239, CreateAtlasRect(5,6)},
            {248, CreateAtlasRect(0,7)}, {250, CreateAtlasRect(1,7)}, {251, CreateAtlasRect(2,7)}, {254, CreateAtlasRect(3,7)}, {255, CreateAtlasRect(4,7)}
        };

        public static bool TileHasProperty(ushort tileID, TileProperty property)
        {
            if (property == TileProperty.None) return _tileProperties[tileID].Properties == TileProperty.None;
            return (_tileProperties[tileID].Properties & property) == property;
        }

        public static TileData GetTileData(ushort tileID)
        {
            return _tileProperties[tileID];
        }
        public static Rectangle GetTileTextureAtlas(byte state)
        {
            return _textureAtlasRects[state];
        }
    }
}
