using Microsoft.Xna.Framework;
using System;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles
{
    public class TorchData : TileData
    {
        public TorchData(int tileID, TileProperty properties, Color color, int itemID = -1) : base(tileID, properties, color, itemID)
        {
        }
        public override int VerifyTile(int x, int y)
        {
            ushort right = WorldGen.World.GetTileID(x + 1, y);
            ushort bottom = WorldGen.World.GetTileID(x, y + 1);
            ushort left = WorldGen.World.GetTileID(x - 1, y);
            ushort wall = WorldGen.World.GetWallID(x, y);
            if (Math.Sign(wall) == 1 || TileDatabase.TileHasProperty(right, TileProperty.Solid) || TileDatabase.TileHasProperty(bottom, TileProperty.Solid) || TileDatabase.TileHasProperty(left, TileProperty.Solid))
                return 1;
            return -1;
        }
        public override bool CanTileBeDamaged(int x, int y)
        {
            return true;
        }
        public override byte GetUpdatedTileState(int x, int y)
        {
            ushort bottom = WorldGen.World.GetTileID(x, y + 1);
            ushort left = WorldGen.World.GetTileID(x - 1, y);
            ushort right = WorldGen.World.GetTileID(x + 1, y);

            if (TileDatabase.TileHasProperty(bottom, TileProperty.Solid))
                return 0;
            else if (TileDatabase.TileHasProperty(left, TileProperty.Solid))
                return 34;
            else if (TileDatabase.TileHasProperty(right, TileProperty.Solid))
                return 62;
            else
                return 0;
        }
    }
}
