using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGreen.Game.Tiles
{
    public class TreeTopData : TreeData
    {
        private Vector2 _offset;
        public TreeTopData(int tileID, TileProperty properties, Color color, Vector2 offset) : base(tileID, properties, color)
        {
            _offset = offset;
        }
        public override void Draw(SpriteBatch spriteBatch, byte tileState, int x, int y)
        {
            spriteBatch.Draw(ContentLoader.TileTextures[TileID], new Vector2(x * Globals.TILESIZE, y * Globals.TILESIZE) + _offset, Main.LightEngine.GetLight(x, y));
        }
    }
}
