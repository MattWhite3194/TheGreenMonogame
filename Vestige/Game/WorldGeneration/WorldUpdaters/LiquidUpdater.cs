using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Vestige.Game.Tiles;

namespace Vestige.Game.WorldGeneration.WorldUpdaters
{
    //TODO: move liquid settling to here
    internal class LiquidUpdater : WorldUpdater
    {
        private Queue<Point> _liquidUpdateQueue = new Queue<Point>();
        private HashSet<Point> _liquidTiles = new HashSet<Point>();

        public LiquidUpdater(WorldGen world, double updateRate) : base(world, updateRate) { }

        public void SettleAll()
        {
            while (_liquidUpdateQueue.Count > 0)
            {
                Point queuedLiquidPoint = _liquidUpdateQueue.Dequeue();
                _liquidTiles.Remove(queuedLiquidPoint);
                SettleLiquid(queuedLiquidPoint.X, queuedLiquidPoint.Y);
            }
        }
        protected override void OnUpdate()
        {
            int numLiquidTiles = _liquidUpdateQueue.Count;
            for (int i = 0; i < numLiquidTiles; i++)
            {
                Point queuedLiquidPoint = _liquidUpdateQueue.Dequeue();
                _liquidTiles.Remove(queuedLiquidPoint);
                SettleLiquid(queuedLiquidPoint.X, queuedLiquidPoint.Y);
            }
        }
        public void QueueLiquidUpdate(int x, int y)
        {
            Point point = new Point(x, y);
            if (!_liquidTiles.Contains(point))
            {
                _liquidUpdateQueue.Enqueue(point);
                _liquidTiles.Add(point);
            }
        }
        //This is the most disgusting code I've ever written
        private void SettleLiquid(int x, int y)
        {
            int remainingMass = world.GetLiquid(x, y);
            if (world.IsTileInBounds(x, y + 1) && !TileDatabase.TileHasProperties(world.GetTileID(x, y + 1), TileProperty.Solid))
            {
                int flow = Math.Min(WorldGen.MaxLiquid - world.GetLiquid(x, y + 1), remainingMass);
                if (flow != 0)
                {
                    world.SetLiquid(x, y, (byte)(world.GetLiquid(x, y) - flow));
                    world.SetLiquid(x, y + 1, (byte)(world.GetLiquid(x, y + 1) + flow), true);
                    remainingMass -= flow;
                }
            }
            if (remainingMass > 0)
            {
                if (remainingMass < 3)
                {
                    remainingMass -= 1;
                }
                int left = 0;
                int right = 0;
                int totalLiquid = remainingMass;
                if (world.IsTileInBounds(x + 1, y) && !TileDatabase.TileHasProperties(world.GetTileID(x + 1, y), TileProperty.Solid))
                {
                    totalLiquid += world.GetLiquid(x + 1, y);
                    right++;
                }
                if (world.IsTileInBounds(x - 1, y) && !TileDatabase.TileHasProperties(world.GetTileID(x - 1, y), TileProperty.Solid))
                {
                    totalLiquid += world.GetLiquid(x - 1, y);
                    left++;
                }
                int averageLiquid = (int)Math.Round((float)totalLiquid / (left + right + 1));
                for (int i = -left; i <= right; i++)
                {
                    if (i == 0)
                        continue;
                    if (world.GetLiquid(x + i, y) != averageLiquid)
                        world.SetLiquid(x + i, y, (byte)averageLiquid, true);
                }
                if (averageLiquid == WorldGen.MaxLiquid - 1 && world.GetLiquid(x, y) == WorldGen.MaxLiquid)
                {
                    averageLiquid = WorldGen.MaxLiquid;
                }
                world.SetLiquid(x, y, (byte)averageLiquid);
            }
            else
            {
                if (world.GetLiquid(x - 1, y) != 0)
                {
                    QueueLiquidUpdate(x - 1, y);
                }
                if (world.GetLiquid(x + 1, y) != 0)
                {
                    QueueLiquidUpdate(x + 1, y);
                }
            }
            if (world.GetLiquid(x, y - 1) != 0 && world.GetLiquid(x, y) != WorldGen.MaxLiquid)
                QueueLiquidUpdate(x, y - 1);
        }
    }
}
