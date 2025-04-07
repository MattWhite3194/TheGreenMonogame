using Microsoft.Xna.Framework;
using TheGreen.Game.Items;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles
{
    public class InventoryTileData : LargeTileData, IInteractableTile
    {
        //TODO: change this file to InventoryTileData, and make it reusable for other inventory tile types
        public readonly int Cols;
        private readonly int Rows;
        public InventoryTileData(int tileID, TileProperty properties, Color color, int itemID = -1, int cols = 8, int rows = 5) : base(tileID, properties, color, new Point(2, 2), new Point(0, 1), itemID, 0)
        {
            Cols = cols;
            Rows = rows;
        }

        public override bool CanTileBeDamaged(int x, int y)
        {
            Point worldOrigin = GetTopLeft(x, y);
            return WorldGen.World.GetTileInventory(worldOrigin) == null;
        }
        public void CloseInventory(int x, int y)
        {
            Point worldOrigin = GetTopLeft(x, y);
            for (int i = 0; i < TileSize.X; i++)
            {
                for (int j = 0; j < TileSize.Y; j++)
                {
                    WorldGen.World.SetTileState(worldOrigin.X + i, worldOrigin.Y + j, (byte)(j * 10 + i));
                }
            }
            Item[] items = WorldGen.World.GetTileInventory(worldOrigin);
            bool emptyInventory = true;
            foreach (Item item in items)
            {
                if (item != null)
                {
                    emptyInventory = false;
                    break;
                }
            }
            if (emptyInventory)
            {
                WorldGen.World.RemoveTileInventory(worldOrigin);
            }
        }

        public void OnRightClick(int x, int y)
        {
            //Inventory Tiles should only have 2 frames, one for closed and one for open, maybe in the future I'll do animations
            Point worldOrigin = GetTopLeft(x, y);
            for (int i = 0; i < TileSize.X; i++)
            {
                for (int j = 0; j < TileSize.Y; j++)
                {
                    WorldGen.World.SetTileState(worldOrigin.X + i, worldOrigin.Y + j, (byte)(j * 10 + i + TileSize.X));
                }
            }
            Item[] items = WorldGen.World.GetTileInventory(worldOrigin);
            if (items == null)
            {
                items = new Item[Rows * Cols];
                WorldGen.World.AddTileInventory(worldOrigin, items);
            }
            Main.EntityManager.GetPlayer().Inventory.DisplayTileInventory(this, new Point(worldOrigin.X, worldOrigin.Y), items);
        }
    }
}
