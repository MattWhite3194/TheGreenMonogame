using Microsoft.Xna.Framework;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles
{
    public class TreeData : TileData
    {
        public TreeData(int tileID, TileProperty properties, Color color, int itemID = -1, int health = 0) : base(tileID, properties, color, itemID, health)
        {
        }
        public override bool CanTileBeDamaged(int x, int y)
        {
            return true;
        }
        public override int VerifyTile(int x, int y)
        {
            ushort bottom = WorldGen.World.GetTileID(x, y + 1);
            ushort left = WorldGen.World.GetTileID(x - 1, y);
            ushort right = WorldGen.World.GetTileID(x + 1, y);

            if (bottom != TileID && !TileDatabase.TileHasProperty(bottom, TileProperty.Solid))
                return -1;
            else if ((WorldGen.World.GetTileState(x, y) == 62 || WorldGen.World.GetTileState(x, y) == 130) && left != TileID && right != TileID)
                return -1;
            return 1;
        }
        public override byte GetUpdatedTileState(int x, int y)
        {
            return WorldGen.World.GetTileState(x, y);
        }
    }
}
