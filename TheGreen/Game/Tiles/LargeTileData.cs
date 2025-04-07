using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles
{
    public class LargeTileData : TileData
    {
        public readonly Point TileSize;
        /// <summary>
        /// The tiles origin point when placing this tile, (0, 0) is the top left corner.
        /// Defaults to the bottom left corner.
        /// </summary>
        public readonly Point Origin;
        private int _animations;
        public LargeTileData(int tileID, TileProperty properties, Color color, Point tileSize, Point origin = default, int itemID = -1, int health = 0, int animations = 0) : base(tileID, properties, color, itemID, health)
        {
            this.TileSize = tileSize;
            this.Origin = origin == default ? new Point(0, TileSize.Y - 1) : origin;
            _animations = animations;
        }
        public override int VerifyTile(int x, int y)
        {
            //World origin is always the bottom left corner
            Point bottomLeft = GetTopLeft(x, y) + new Point(0, TileSize.Y - 1);

            int verification = 1;
            for (int i = 0; i < TileSize.X; i++)
            {
                if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(bottomLeft.X + i, bottomLeft.Y + 1), TileProperty.Solid))
                    return -1;
                for (int j = 0; j < TileSize.Y; j++)
                {
                    //TODO: change to check if it's a replaceable tile like grass or something
                    if (WorldGen.World.GetTileID(bottomLeft.X + i, bottomLeft.Y - j) != 0)
                        verification = 0;
                }
            }
            return verification;
        }
        public override bool CanTileBeDamaged(int x, int y)
        {
            return true;
        }
        public override byte GetUpdatedTileState(int x, int y)
        {
            return WorldGen.World.GetTileState(x, y);
        }
        public virtual Point GetTopLeft(int x, int y)
        {
            if (WorldGen.World.GetTileID(x, y) != TileID)
                return new Point(x, y) - Origin;
            int tileState = WorldGen.World.GetTileState(x, y);
            int xOff = (tileState % 10) % TileSize.X;
            int yOff = (tileState / 10) % TileSize.Y;
            return new Point(x - xOff, y - yOff);
        }
        public override void Draw(SpriteBatch spriteBatch, byte tileState, int x, int y)
        {
            Rectangle textureAtlas = new Rectangle(tileState % 10 * Globals.TILESIZE, tileState / 10 * Globals.TILESIZE, Globals.TILESIZE, Globals.TILESIZE);
            spriteBatch.Draw(ContentLoader.TileTextures[TileID], new Vector2(x, y) * Globals.TILESIZE, textureAtlas, Main.LightEngine.GetLight(x, y));
        }
    }
}
