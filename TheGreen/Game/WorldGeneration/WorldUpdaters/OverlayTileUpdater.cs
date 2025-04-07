using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TheGreen.Game.Tiles;

namespace TheGreen.Game.WorldGeneration.WorldUpdaters
{
    /// <summary>
    /// Spreads overlay tiles marked as spreadable. Examples: grass
    /// </summary>
    internal class OverlayTileUpdater : WorldUpdater
    {
        private Queue<(int, int, ushort)> _overlayUpdateQueue = new Queue<(int, int, ushort)>();
        private HashSet<(int, int, ushort)> _baseTiles = new HashSet<(int, int, ushort)>();
        public OverlayTileUpdater(double updateRate) : base(updateRate)
        {

        }
        public void EnqueueOverlayTile(int x, int y, ushort overlayTileID)
        {
            foreach (Point point in WorldGen.World.SurroundingTiles)
            {
                if (!WorldGen.World.IsTileInBounds(x + point.X, y + point.Y))
                {
                    continue;
                }

                if (_baseTiles.Contains((x + point.X, y + point.Y, overlayTileID)))
                {
                    continue;
                }
                if (WorldGen.World.GetTileID(x + point.X, y + point.Y) != TileDatabase.GetTileData(overlayTileID).BaseTileID || WorldGen.World.GetTileState(x + point.X, y + point.Y) == 255)
                    continue;
                _overlayUpdateQueue.Enqueue((x + point.X, y + point.Y, overlayTileID));
                _baseTiles.Add((x + point.X, y + point.Y, overlayTileID));
            }
        }

        protected override void OnUpdate()
        {
            if (_overlayUpdateQueue.Count == 0)
                return;
            (int x, int y, ushort overlayTileID) = _overlayUpdateQueue.Dequeue();
            _baseTiles.Remove((x, y, overlayTileID));
            if (WorldGen.World.GetTileState(x, y) != 255 && WorldGen.World.GetTileID(x, y) == TileDatabase.GetTileData(overlayTileID).BaseTileID)
            {
                WorldGen.World.SetTile(x, y, overlayTileID);
            }
        }
    }
}
