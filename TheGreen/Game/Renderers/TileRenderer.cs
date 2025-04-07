using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Renderer
{
    public class TileRenderer
    {
        private Point _drawBoxMin;
        private Point _drawBoxMax;
        public void DrawWalls(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    if (i >= 0 && i < WorldGen.World.WorldSize.X && j >= 0 && j < WorldGen.World.WorldSize.Y)
                    {
                        //TEMPORARY
                        ushort wallID = WorldGen.World.GetWallID(i, j);
                        if (wallID != 0)
                        {
                            Color light = Main.LightEngine.GetLight(i, j);
                            light.R = (byte)Math.Max(0, light.R - 30);
                            light.G = (byte)Math.Max(0, light.G - 30);
                            light.B = (byte)Math.Max(0, light.B - 30);
                            spriteBatch.Draw(ContentLoader.TileTextures[wallID], new Vector2(i * Globals.TILESIZE, j * Globals.TILESIZE), TileDatabase.GetTileTextureAtlas(255), light);
                        }
                    }
                }
            }
        }

        public void DrawBackgroundTiles(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort tileID = WorldGen.World.GetTileID(i, j);
                    //Draw all tiles that are not solid in the wall layer
                    if (TileDatabase.TileHasProperty(tileID, TileProperty.Solid) || tileID == 0)
                        continue;
                    TileDatabase.GetTileData(tileID).Draw(spriteBatch, WorldGen.World.GetTileState(i, j), i, j);
                }
            }
        }

        public void DrawTiles(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort tileID = WorldGen.World.GetTileID(i, j);
                    if (!TileDatabase.TileHasProperty(tileID, TileProperty.Solid))
                        continue;
                    TileDatabase.GetTileData(tileID).Draw(spriteBatch, WorldGen.World.GetTileState(i, j), i, j);
                }
            }
            foreach (Point crackPoint in WorldGen.World.GetMinedTiles().Keys)
            {
                spriteBatch.Draw(ContentLoader.Cracks, crackPoint.ToVector2() * Globals.TILESIZE, Main.LightEngine.GetLight(crackPoint.X, crackPoint.Y));
            }
        }

        public void DrawLiquids(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    if (WorldGen.World.GetLiquid(i, j) != 0)
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2(i * Globals.TILESIZE, j * Globals.TILESIZE), Main.LightEngine.GetLight(i, j));
                }
            }
        }

        /// <summary>
        /// Debugging purposes only, use for showing tile values displayed over tiles
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawDebug(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                { 
                    spriteBatch.DrawString(ContentLoader.GameFont, WorldGen.World.GetTileID(i, j) + "", new Vector2(i, j) * Globals.TILESIZE, Color.White, 0.0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0.0f);
                }
            }
        }

        public void SetDrawBox(Point drawBoxMin, Point drawBoxMax)
        {
            this._drawBoxMin = drawBoxMin;
            this._drawBoxMax = drawBoxMax;
        }
    }
}