using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Items
{
    public class LiquidItem : Item
    {
        public readonly ushort LiquidID;
        public LiquidItem(int id, string name, string description, Texture2D image, ushort liquidID) : base(id, name, description, image, true, 0.15, true)
        {
            //TODO: liquid IDS like lava or water or whatever
            this.LiquidID = liquidID;
        }
        public override bool UseItem()
        {
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(Globals.TILESIZE);
            if (WorldGen.World.GetLiquid(mouseTilePosition.X, mouseTilePosition.Y) != 255)
            {
                WorldGen.World.SetLiquid(mouseTilePosition.X, mouseTilePosition.Y, 255);
                return true;
            }
            return false;
        }
    }
}
