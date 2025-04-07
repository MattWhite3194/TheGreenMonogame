using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Items
{
    public class TileItem : Item
    {
        public readonly ushort TileID;
        public TileItem(int id, string name, string description, Texture2D image, ushort tileID) : base(id, name, description, image, true, 0.15, true)
        { 
            this.TileID = tileID;
        }
        public override bool UseItem()
        {
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(Globals.TILESIZE, Globals.TILESIZE);
            if (WorldGen.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y) == 0)
            {
                if (WorldGen.World.SetTile(mouseTilePosition.X, mouseTilePosition.Y, TileID))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
