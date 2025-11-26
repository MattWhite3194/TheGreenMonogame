using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Vestige.Game.Items;
using Vestige.Game.Tiles;
using Vestige.Game.Tiles.TileData;
using Vestige.Game.Tiles.WallData;
using Vestige.Game.WorldGeneration.WorldUpdaters;

namespace Vestige.Game.WorldGeneration
{
    public class WorldGen
    {
        private Tile[] _tiles;
        private Random _random = new Random();
        public Point SpawnTile;
        private int _dirtDepth;
        private int _grassDepth = 8;
        /// <summary>
        /// The lowest point of the surface in the world. Relative to the bottom of the world
        /// </summary>
        private int _surfaceHeight;
        /// <summary>
        /// The Lowest point of the surface in the world. Relative to the top of the world
        /// </summary>
        public int SurfaceDepth;
        public int CaveDepth;
        public Point WorldSize;
        public int SpaceHeight;
        public static readonly byte MaxLiquid = 127;
        private int[,,] gradients = new int[256, 256, 2];

        /// <summary>
        /// Stores the location and damage information of any tiles that are actively being mined by the player
        /// </summary>
        private Dictionary<Point, DamagedTile> _minedTiles = new Dictionary<Point, DamagedTile>();
        /// <summary>
        /// Stores the location and damage information of any walls that are actively being mined by the player
        /// </summary>
        private Dictionary<Point, DamagedTile> _minedWalls = new Dictionary<Point, DamagedTile>();

        private List<WorldUpdater> _worldUpdaters;
        private LiquidUpdater _liquidUpdater;
        private OverlayTileUpdater _overlayTileUpdater;
        private Dictionary<Point, Item[]> _tileInventories;
        /// <summary>
        /// Event called every time the world is updated. Members assigned will persist until they return true
        /// </summary>
        public event Func<bool> OnWorldUpdate;
        private List<Structure> _structures;
        private string _generationStatus = "";

        public WorldGen(int sizeX, int sizeY)
        {
            _tiles = new Tile[sizeX * sizeY];
            _tileInventories = new Dictionary<Point, Item[]>();
            WorldSize = new Point(sizeX, sizeY);
            _worldUpdaters = new List<WorldUpdater>();
            _liquidUpdater = new LiquidUpdater(this, 0.04);
            _overlayTileUpdater = new OverlayTileUpdater(this, 5);
            _worldUpdaters.AddRange([_liquidUpdater, _overlayTileUpdater]);
        }

        public void GenerateWorld(int seed = 0)
        {
            _random = seed != 0 ? new Random(seed) : new Random();
            _structures = new List<Structure>();
            int[] surfaceTerrain = new int[WorldSize.X];
            _surfaceHeight = WorldSize.Y;
            InitializeGradients();

            _generationStatus = "Generating Terrain";
            //place stone and get surface height
            int fullHillsDistance = WorldSize.X / 12;
            int hillStartDistance = WorldSize.X / 16;
            int fullMountainsDistance = WorldSize.X / 3;
            int mountainStartDistance = WorldSize.X / 4;

            float plainsHeight = 10.0f;
            float hillsHeight = 80.0f;
            float mountainsHeight = 130.0f;
            for (int i = 0; i < WorldSize.X; i++)
            {
                int distanceFromCenter = Math.Abs(i - WorldSize.X / 2);
                //plains
                int plainsNoise = (int)(PerlinNoiseAt(i, 50.0, 3, 0.5) * plainsHeight);
                //hills - stretch from 1/12th of the world from the center to the edge
                float hillGradient = 1.0f;
                if (distanceFromCenter < fullHillsDistance)
                {
                    hillGradient = Math.Max(0, distanceFromCenter - hillStartDistance) / (float)( fullHillsDistance - hillStartDistance);
                }
                int hillNoise = (int)(PerlinNoiseAt(i, 300.0, 6, 0.5) * hillsHeight * hillGradient);
                //mountains - stretch from 1/3rd of the worlds width from the center to the edge
                float mountainGradient = 1.0f;
                if (distanceFromCenter < fullMountainsDistance)
                {
                    mountainGradient = Math.Max(0, distanceFromCenter - mountainStartDistance) / (float)(fullMountainsDistance - mountainStartDistance);
                }
                int mountainsNoise = (int)(PerlinNoiseAt(i, 500.0, 3, 0.5) * mountainsHeight * mountainGradient);
                //total height
                int height = plainsNoise + hillNoise + mountainsNoise;
                for (int j = (WorldSize.Y / 2) - (WorldSize.Y / 4) + height; j < WorldSize.Y; j++)
                {
                    SetTile(i, j, 4);
                    SetWall(i, j, 2);
                    surfaceTerrain[i] = (WorldSize.Y / 2) - (WorldSize.Y / 4) + height;

                    if (WorldSize.Y - surfaceTerrain[i] < _surfaceHeight)
                        _surfaceHeight = WorldSize.Y - surfaceTerrain[i];
                }
            }
            SurfaceDepth = WorldSize.Y - _surfaceHeight;
            _dirtDepth = _surfaceHeight / 6;
            //place dirt
            for (int i = 0; i < WorldSize.X; i++)
            {

                for (int j = 0; j < _dirtDepth + _random.Next(0, 3); j++)
                {
                    if (surfaceTerrain[i] < SurfaceDepth - 100 - _random.Next(0, 30))
                        continue;
                    if (j > 3)
                    {
                        SetWall(i, surfaceTerrain[i] + j, 1);
                    }
                    else
                    {
                        SetWall(i, surfaceTerrain[i] + j, 0);
                    }
                    SetTile(i, surfaceTerrain[i] + j, 1);
                }
            }
            SpawnTile = new Point(WorldSize.X / 2, surfaceTerrain[WorldSize.X / 2]);

            _generationStatus = "Generating Caves";
            //Perlin noise caves
            for (int x = 0; x < WorldSize.X; x++)
            {
                for (int y = 0; y < _surfaceHeight; y++)
                {
                    //cave gradient, caves will progressively get larger from 0 to surfaceHeight / 4.0
                    float gradient = Math.Max(_surfaceHeight / 4.0f - y, 0.0f) / (_surfaceHeight / 4.0f) / 3.0f;
                    if (PerlinNoiseAt(x, y, 25.0, 4, 0.5) + gradient < -0.1)
                        SetTile(x, WorldSize.Y - _surfaceHeight + y, 0);
                }
            }
            CaveDepth = WorldSize.Y - _surfaceHeight + (int)(_surfaceHeight / 4.0f);
            //TODO: worm caves from surface
            int numCaves = (int)(WorldSize.X * 0.002f);
            while (numCaves > 0)
            {
                int caveX = _random.Next(40, WorldSize.X - 40);
                if (Math.Abs((WorldSize.X / 2) - caveX) < 40)
                    continue;
                Rectangle cave = new Rectangle(caveX, surfaceTerrain[WorldSize.X / 2] - 5, 10, 20);
                bool caveIsClear = true;
                foreach (Structure structure in _structures)
                {
                    if (cave.Intersects(structure.Bounds))
                    {
                        caveIsClear = false;
                        break;
                    }
                }
                if (!caveIsClear)
                    continue;
                Structure newCave = new Structure(cave, 0);
                _structures.Add(newCave);
                WormCave(caveX, surfaceTerrain[caveX], 10, (CaveDepth - SurfaceDepth + _random.Next(-10, 30)), 0.01f, 0.8f);
                numCaves--;
            }

            _generationStatus = "Generating Rocks";
            //Generate stone chunks in dirt layer
            for (int i = 0; i < WorldSize.X * WorldSize.Y * 0.0002; i++)
            {
                TileBlobber(_random.Next(0, WorldSize.X), _random.Next(SurfaceDepth, SurfaceDepth + _dirtDepth), _random.Next(4, 15), _random.Next(5, 30), 4);
            }

            _generationStatus = "Generating Dirts";
            //Generate dirt chunks in stone layer
            for (int i = 0; i < WorldSize.X * WorldSize.Y * 0.0002; i++)
            {
                TileBlobber(_random.Next(0, WorldSize.X), _random.Next(SurfaceDepth + _dirtDepth, SurfaceDepth + _dirtDepth + ((_surfaceHeight - _dirtDepth) / 2)), _random.Next(4, 10), _random.Next(5, 30), 1);
            }
            for (int i = 0; i < WorldSize.X * WorldSize.Y * 0.0001; i++)
            {
                TileBlobber(_random.Next(0, WorldSize.X), _random.Next(SurfaceDepth + _dirtDepth + ((_surfaceHeight - _dirtDepth) / 2), WorldSize.Y), _random.Next(4, 10), _random.Next(5, 30), 1);
            }

            _generationStatus = "Generating Ores";
            //Generate Coal
            for (int i = 0; i < WorldSize.X * WorldSize.Y * 0.0005; i++)
            {
                TileBlobber(_random.Next(0, WorldSize.X), _random.Next(SurfaceDepth + (_dirtDepth / 2), SurfaceDepth + _dirtDepth + ((_surfaceHeight - _dirtDepth) / 2)), _random.Next(3, 5), _random.Next(5, 8), 12, replaceTiles: new HashSet<int> { 4 });
            }

            _generationStatus = "Adding Buildings";
            //Generate buildings
            for (int i = _random.Next(20, 40); i < WorldSize.X - _random.Next(80, 100); i += (int)Math.Max(200.0f, WorldSize.X * _random.NextDouble() / 5.0f))
            {
                if (_random.NextDouble() < 0.25)
                    continue;
                if (Math.Abs((WorldSize.X / 2) - i) < 40)
                    continue;
                //TODO: move this into building function for proper door placement and connection
                Rectangle building = GenerateBuilding(i, surfaceTerrain[i]);
                for (int j = 0; j < _random.Next(0, 2); j++)
                {
                    Rectangle nextBuilding = GenerateBuilding(i + building.Width - 1, surfaceTerrain[i + building.Width - 1]);
                    building = new Rectangle(building.X, Math.Min(building.Y, nextBuilding.Y), building.Width + nextBuilding.Width - 1, Math.Max(building.Height, nextBuilding.Height));
                }
                Structure structure = new Structure(building, 0);
                _structures.Add(structure);
            }

            _generationStatus = "Adding Lakes";
            //Generate lakes
            int numLakes = (int)Math.Ceiling(WorldSize.X / 4200.0f * _random.Next(3, 5));
            for (int i = 0; i < numLakes; i++)
            {
                int x = 0;
                bool foundSuccessfulPlacement = false;
                while (!foundSuccessfulPlacement)
                {
                    x = _random.Next(50, WorldSize.X - 50);
                    foundSuccessfulPlacement = true;
                    if (x >= WorldSize.X / 2 - 50 && x <= WorldSize.X / 2 + 50)
                        foundSuccessfulPlacement = false;
                    foreach (Structure structure in _structures)
                    {
                        if (new Rectangle(x - 20, surfaceTerrain[x] - 20, 40, 40).Intersects(structure.Bounds))
                        {
                            foundSuccessfulPlacement = false;
                            break;
                        }
                    }
                }
                GenerateLake(x, surfaceTerrain[x]);
            }

            _generationStatus = "Calculating Tile States";
            //calculate tile states
            for (int i = 1; i < WorldSize.X - 1; i++)
            {
                for (int j = 1; j < WorldSize.Y - 1; j++)
                {
                    SetTileState(i, j, TileDatabase.GetTileData(GetTileID(i, j)).GetUpdatedTileState(this, i, j));
                    SetWallState(i, j, TileDatabase.GetWallData(GetWallID(i, j)).GetUpdatedWallState(this, i, j));
                }
            }

            _generationStatus = "Spreading Grass";
            //spread grass
            for (int i = 0; i < WorldSize.X; i++)
            {
                for (int j = 0; j < _grassDepth; j++)
                {
                    if (GetTileID(i, surfaceTerrain[i] + j) == 1 && GetTileState(i, surfaceTerrain[i] + j) != 255)
                    {
                        SetTile(i, surfaceTerrain[i] + j, 2);
                    }
                }
            }

            _generationStatus = "Planting Trees";
            int minTreeDistance = 5;
            int lastTreeX = 10;
            //Plant Trees
            for (int i = 10; i < WorldSize.X - 10; i++)
            {
                if (_random.NextDouble() < 0.2 && i - lastTreeX > minTreeDistance)
                {
                    if (GetTileID(i, surfaceTerrain[i]) != 2)
                        continue;
                    GenerateTree(i, surfaceTerrain[i] - 1);
                    lastTreeX = i;
                }
            }

            _generationStatus = "Growing Grass";
            byte[] randomGrassStates = [0, 10, 20, 30];
            for (int i = 0; i < WorldSize.X; i++)
            {
                if (_random.NextDouble() < 0.2)
                {
                    if (GetTileID(i, surfaceTerrain[i]) != 2 || GetTileID(i, surfaceTerrain[i] - 1) != 0)
                        continue;
                    SetTile(i, surfaceTerrain[i] - 1, 15);
                    SetTileState(i, surfaceTerrain[i] - 1, randomGrassStates[_random.Next(0, 4)]);
                }
            }

            _generationStatus = "Taming The Seas";
            //Settle all liquids
            _liquidUpdater.SettleAll();
        }
        public string GetGenerationStatus()
        {
            return _generationStatus;
        }
        public void Update(double delta)
        {
            foreach (WorldUpdater worldUpdater in _worldUpdaters)
            {
                worldUpdater.Update(delta);
            }
            foreach (Point point in _minedTiles.Keys)
            {
                DamagedTile damagedTileData = _minedTiles[point];
                damagedTileData.TimeLeft += delta;
                if (damagedTileData.TimeLeft > 5 || GetTileID(point.X, point.Y) == 0)
                {
                    _minedTiles.Remove(point);
                }
                else
                    _minedTiles[point] = damagedTileData;
            }
            foreach (Point point in _minedWalls.Keys)
            {
                DamagedTile damagedWallData = _minedWalls[point];
                damagedWallData.TimeLeft += delta;
                if (damagedWallData.TimeLeft > 5 || GetWallID(point.X, point.Y) == 0)
                {
                    _minedWalls.Remove(point);
                }
                else
                    _minedWalls[point] = damagedWallData;
            }
            if (OnWorldUpdate != null)
            {
                foreach (Func<bool> tileUpdate in OnWorldUpdate.GetInvocationList())
                {
                    if (tileUpdate.Invoke())
                    {
                        OnWorldUpdate -= tileUpdate;
                    }
                }
            }
        }

        /// <summary>
        /// Damages a tile at the specified point. If the tile health is depleted to 0, it will be removed.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="damage"></param>
        public void DamageTile(Point coordinates, int damage)
        {
            if (!IsTileInBounds(coordinates.X, coordinates.Y))
                return;
            ushort tileID = GetTileID(coordinates.X, coordinates.Y);
            if (tileID == 0)
                return;
            DefaultTileData tileData = TileDatabase.GetTileData(tileID);
            if (!tileData.CanTileBeDamaged(this, coordinates.X, coordinates.Y))
                return;
            DamagedTile damagedTileData = _minedTiles.ContainsKey(coordinates) ? _minedTiles[coordinates] : new DamagedTile(coordinates.X, coordinates.Y, tileID, tileData.Health, 0);
            damagedTileData.Health = damagedTileData.Health - damage;
            damagedTileData.TimeLeft = 0;
            if (damagedTileData.Health <= 0)
            {
                RemoveTile(coordinates.X, coordinates.Y);
                _minedTiles.Remove(coordinates);
            }
            else
            {
                _minedTiles[coordinates] = damagedTileData;
            }
            //TODO: play tile specific damage sound here, so it only plays if the tile was actually damaged. any mining sounds or item use sounds will be playes by the item collider or the inventory useItem
        }
        public void DamageWall(Point coordinates, int damage)
        {
            if (!IsTileInBounds(coordinates.X, coordinates.Y))
                return;
            ushort wallID = GetWallID(coordinates.X, coordinates.Y);
            if (wallID == 0)
                return;
            //Reusing DamagedTile since it would be redundant to add another
            DefaultWallData wallData = TileDatabase.GetWallData(wallID);
            DamagedTile damagedWallData = _minedWalls.ContainsKey(coordinates) ? _minedWalls[coordinates] : new DamagedTile(coordinates.X, coordinates.Y, wallID, wallData.Health, 0);
            damagedWallData.Health = damagedWallData.Health - damage;
            damagedWallData.TimeLeft = 0;
            if (damagedWallData.Health <= 0)
            {
                RemoveWall(coordinates.X, coordinates.Y);
                _minedWalls.Remove(coordinates);
            }
            else
            {
                _minedWalls[coordinates] = damagedWallData;
            }
            //TODO: play tile specific damage sound here, so it only plays if the tile was actually damaged. any mining sounds or item use sounds will be playes by the item collider or the inventory useItem
        }
        public bool IsTileInBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < WorldSize.X && y < WorldSize.Y;
        }

        private byte[] _randTreeTileStates = [0, 2, 8, 10];
        private void GenerateTree(int x, int y)
        {
            //Generate trunk
            int height = _random.Next(5, 20);
            //check if the tree can generate
            for (int i = 0; i < height; i++)
            {
                if (GetTileID(x, y - i) != 0)
                    return;
            }
            //generate base
            SetTile(x, y, 5);
            SetTileState(x, y, 143);
            bool left = GetTileID(x - 1, y) == 0 && GetTileID(x - 1, y + 1) == 2;
            bool right = GetTileID(x + 1, y) == 0 && GetTileID(x + 1, y + 1) == 2;
            if (left)
            {
                SetTile(x - 1, y, 5);
                SetTileState(x - 1, y, 62);
                SetTileState(x, y, 139);
            }
            if (right)
            {
                SetTile(x + 1, y, 5);
                SetTileState(x + 1, y, 130);
                if (left)
                    SetTileState(x, y, 128);
                else
                    SetTileState(x, y, 142);
            }
            //generate trunk
            for (int h = 1; h < height; h++)
            {
                SetTile(x, y - h, 5);
                SetTileState(x, y - h, _randTreeTileStates[_random.Next(0, _randTreeTileStates.Length)]);
            }
            //Add tree top
            SetTile(x, y - height, 6);
            SetTileState(x, y - height, 0);
        }

        private void InitializeGradients()
        {
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    gradients[x, y, 0] = _random.Next(-1, 2);
                    gradients[x, y, 1] = _random.Next(-1, 2);
                }
            }
        }

        private double GetInfluenceValue(double x, double y, int Xgrad, int Ygrad)
        {
            return (gradients[Xgrad % 256, Ygrad % 256, 0] * (x - Xgrad)) +
                   (gradients[Xgrad % 256, Ygrad % 256, 1] * (y - Ygrad));
        }

        private double Lerp(double v0, double v1, double t)
        {
            return ((1 - t) * v0) + (t * v1);
        }

        private double Fade(double t)
        {
            return (3 * Math.Pow(t, 2)) - (2 * Math.Pow(t, 3));
        }

        private double Perlin(double x, double y)
        {
            int X0 = (int)x;
            int Y0 = (int)y;
            int X1 = X0 + 1;
            int Y1 = Y0 + 1;

            double sx = Fade(x - X0);
            double sy = Fade(y - Y0);

            double topLeftDot = GetInfluenceValue(x, y, X0, Y1);
            double topRightDot = GetInfluenceValue(x, y, X1, Y1);
            double bottomLeftDot = GetInfluenceValue(x, y, X0, Y0);
            double bottomRightDot = GetInfluenceValue(x, y, X1, Y0);
            return Lerp(Lerp(bottomLeftDot, bottomRightDot, sx), Lerp(topLeftDot, topRightDot, sx), sy);
        }

        private double PerlinNoiseAt(int x, double scale = 100.0, int octaves = 4, double persistence = 0.5)
        {
            double amplitude = 1.0;
            double frequency = 1.0;
            double maxValue = 0;
            double noise = 0.0;
            for (int octave = 0; octave < octaves; octave++)
            {
                // Apply frequency and scale to the coordinates for each octave
                noise += Perlin(x / scale * frequency, 0) * amplitude;

                maxValue += amplitude;
                amplitude *= persistence;  // Amplitude decreases with each octave
                frequency *= 2;  // Frequency doubles for each octave
            }

            noise /= maxValue;

            return noise;
        }

        private double PerlinNoiseAt(int x, int y, double scale = 100.0, int octaves = 4, double persistence = 0.5)
        {
            double amplitude = 1.0;
            double frequency = 1.0;
            double maxValue = 0;
            double noise = 0.0;
            for (int octave = 0; octave < octaves; octave++)
            {
                // Apply frequency and scale to the coordinates for each octave
                noise += Perlin(x / scale * frequency, y / scale * frequency) * amplitude;

                maxValue += amplitude;
                amplitude *= persistence;  // Amplitude decreases with each octave
                frequency *= 2;  // Frequency doubles for each octave
            }

            noise /= maxValue;

            return noise;
        }

        public ushort GetTileID(int x, int y)
        {
            return _tiles[(y * WorldSize.X) + x].ID;
        }

        public ushort GetWallID(int x, int y)
        {
            return _tiles[(y * WorldSize.X) + x].WallID;
        }

        public bool PlaceTile(int x, int y, ushort tileID)
        {
            if (tileID != 0 && TileDatabase.GetTileData(tileID).VerifyTile(this, x, y) != 1)
                return false;
            if (TileDatabase.TileHasProperties(tileID, TileProperty.LargeTile))
            {
                SetLargeTile(x, y, tileID);
                //TEMPORARY
                //TODO: add tiles updated to a list, and then update the tiles in the list and around the tiles in the list
                return true;
            }
            else
            {
                SetTile(x, y, tileID);
            }
            //tile states need to be updated first before calling any other checks
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    SetTileState(x + i, y + j, TileDatabase.GetTileData(GetTileID(x + i, y + j)).GetUpdatedTileState(this, x + i, y + j));
                }
            }
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    UpdateTile(x + i, y + j);
                }
            }
            return true;
        }
        private void UpdateTile(int x, int y)
        {
            DefaultTileData tileData = TileDatabase.GetTileData(GetTileID(x, y));
            if (tileData.VerifyTile(this, x, y) == -1)
            {
                //Don't drop an item if the tile is a falling tile, it will create it's own projectile when it's removed
                RemoveTile(x, y, tileData is not FallingTileData);
            }
            if (GetLiquid(x, y) != 0)
            {
                _liquidUpdater.QueueLiquidUpdate(x, y);
            }
            if (tileData is OverlayTileData overlayTile)
            {
                if (GetTileState(x, y) == 255)
                    _tiles[(y * WorldSize.X) + x].ID = overlayTile.BaseTileID;
                else
                    _overlayTileUpdater.EnqueueOverlayTile(x, y, GetTileID(x, y));
            }
        }
        /// <summary>
        /// Will remove the tile at the specified position and drop its item. This function does not check if the tile can be removed.
        /// Call damage tile to ensure tiles that shouldn't be removed aren't removed.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RemoveTile(int x, int y, bool dropItem = true)
        {
            ushort tileID = GetTileID(x, y);
            if (TileDatabase.TileHasProperties(tileID, TileProperty.LargeTile))
            {
                RemoveLargeTile(x, y, tileID);
            }
            else
            {
                SetTile(x, y, TileDatabase.GetTileData(tileID) is OverlayTileData overlayTile ? overlayTile.BaseTileID : (ushort)0);
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        SetTileState(x + i, y + j, TileDatabase.GetTileData(GetTileID(x + i, y + j)).GetUpdatedTileState(this, x + i, y + j));
                    }
                }
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        UpdateTile(x + i, y + j);
                    }
                }
            }
            if (!dropItem)
                return;
            int itemID = TileDatabase.GetTileData(tileID).ItemID;
            if (itemID == -1)
                return;
            Item item = Item.InstantiateItemByID(itemID);
            if (item != null)
            {
                Main.EntityManager.CreateItemDrop(item, (new Vector2(x, y) * Vestige.TILESIZE) + new Vector2(Vestige.TILESIZE / 2), new Vector2(((float)Main.Random.NextDouble() - 0.5f) * 2.0f * 20, 0));
            }
        }
        private void SetLargeTile(int x, int y, ushort tileID)
        {
            if (TileDatabase.GetTileData(tileID) is not LargeTileData largeTileData)
                return;
            Point topLeft = largeTileData.GetTopLeft(this, x, y);
            for (int i = 0; i < largeTileData.TileSize.X; i++)
            {
                for (int j = 0; j < largeTileData.TileSize.Y; j++)
                {
                    SetTile(topLeft.X + i, topLeft.Y + j, tileID);
                    SetTileState(topLeft.X + i, topLeft.Y + j, (byte)((j * 10) + i));
                    if (TileDatabase.TileHasProperties(tileID, TileProperty.Solid))
                    {
                        SetLiquid(topLeft.X + i, topLeft.Y + j, 0);
                    }
                }
            }
        }
        private void RemoveLargeTile(int x, int y, ushort tileID)
        {
            if (TileDatabase.GetTileData(tileID) is not LargeTileData largeTileData)
                return;
            Point topLeft = largeTileData.GetTopLeft(this, x, y);
            for (int i = 0; i < largeTileData.TileSize.X; i++)
            {
                for (int j = 0; j < largeTileData.TileSize.Y; j++)
                {
                    _tiles[((topLeft.Y + j) * WorldSize.X) + topLeft.X + i].ID = 0;
                    SetTileState(topLeft.X + i, topLeft.Y + j, 0);
                }
            }
        }
        public bool PlaceWall(int x, int y, ushort wallID)
        {
            if (wallID != 0 && TileDatabase.GetWallData(wallID).VerifyWall(this, x, y) != 1)
                return false;
            SetWall(x, y, wallID);
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    SetWallState(x + i, y + j, TileDatabase.GetWallData(wallID).GetUpdatedWallState(this, x + i, y + j));
                    UpdateTile(x + i, y + j);
                }
            }
            return true;
        }
        public void RemoveWall(int x, int y)
        {
            ushort wallID = GetWallID(x, y);
            PlaceWall(x, y, 0);
            int itemID = TileDatabase.GetWallData(wallID).ItemID;
            if (itemID == -1)
                return;
            Item item = Item.InstantiateItemByID(itemID);
            if (item != null)
            {
                Main.EntityManager.CreateItemDrop(item, (new Vector2(x, y) * Vestige.TILESIZE) + new Vector2(Vestige.TILESIZE / 2), new Vector2(((float)Main.Random.NextDouble() - 0.5f) * 2.0f * 20, 0));
            }
        }
        public void SetTileState(int x, int y, byte state)
        {
            _tiles[(y * WorldSize.X) + x].State = state;
        }
        public void SetWallState(int x, int y, byte state)
        {
            _tiles[(y * WorldSize.X) + x].WallState = state;
        }
        public void SetTile(int x, int y, ushort tileID)
        {
            _tiles[(y * WorldSize.X) + x].ID = tileID;
            if (TileDatabase.TileHasProperties(tileID, TileProperty.Solid))
                SetLiquid(x, y, 0);
        }
        public void SetWall(int x, int y, ushort wallID)
        {
            _tiles[(y * WorldSize.X) + x].WallID = wallID;
        }
        public Dictionary<Point, DamagedTile> GetDamagedTiles()
        {
            return _minedTiles;
        }
        public Dictionary<Point, DamagedTile> GetDamagedWalls()
        {
            return _minedWalls;
        }
        public byte GetTileState(int x, int y)
        {
            return _tiles[(y * WorldSize.X) + x].State;
        }
        public byte GetWallState(int x, int y)
        {
            return _tiles[(y * WorldSize.X) + x].WallState;
        }
        public byte GetLiquid(int x, int y)
        {
            return _tiles[(y * WorldSize.X) + x].Liquid;
        }
        public void SetLiquid(int x, int y, byte amount, bool forceUpdate = false)
        {
            if (forceUpdate)
            {
                _liquidUpdater.QueueLiquidUpdate(x, y);
                UpdateTile(x, y);
            }
            _tiles[(y * WorldSize.X) + x].Liquid = amount;
        }
        public void AddTileInventory(Point coordinates, Item[] items)
        {
            _tileInventories[coordinates] = items;
        }
        public void RemoveTileInventory(Point coordinates)
        {
            _tileInventories.Remove(coordinates);
        }
        public Item[] GetTileInventory(Point coordinates)
        {
            return _tileInventories.ContainsKey(coordinates) ? _tileInventories[coordinates] : null;
        }
        public Dictionary<Point, Item[]> GetAllTileInventories()
        {
            return _tileInventories;
        }
        private void TileBlobber(int x, int y, double size, int passes, ushort tileID, bool replaceOnly = true, HashSet<int> replaceTiles = null)
        {
            double remainingSize = size;
            double remainingPasses = passes;
            Vector2 currentTile = new Vector2(x, y);
            Vector2 tileOffset = new Vector2(_random.Next(-10, 11) * 0.1f, _random.Next(-10, 11) * 0.1f);
            while (remainingSize > 0.0 && remainingPasses > 0.0)
            {
                //decrease strength each pass
                remainingSize = size * (remainingPasses / passes);
                remainingPasses -= 1.0;
                //Rectangle around point with width = remainingStrength and height = remainingStrength
                int leftBound = Math.Max(0, (int)(currentTile.X - (remainingSize / 2)));
                int rightBound = Math.Min(WorldSize.X - 1, (int)(currentTile.X + (remainingSize / 2)));
                int topBound = Math.Max(0, (int)(currentTile.Y - (remainingSize / 2)));
                int bottomBound = Math.Min(WorldSize.Y - 1, (int)(currentTile.Y + (remainingSize / 2)));
                for (int i = leftBound; i <= rightBound; i++)
                {
                    for (int j = topBound; j <= bottomBound; j++)
                    {
                        //check distance in a diamond shape + a random offset
                        if (Math.Abs((double)i - currentTile.X) + Math.Abs((double)j - currentTile.Y) < size / 2 * (1.0 + (_random.Next(-10, 11) * 0.015)))
                        {
                            if (!replaceOnly)
                                SetTile(i, j, tileID);
                            else if (replaceTiles?.Contains(GetTileID(i, j)) ?? TileDatabase.TileHasProperties(GetTileID(i, j), TileProperty.Solid))
                                SetTile(i, j, tileID);
                        }
                    }
                }
                currentTile += tileOffset;
                tileOffset.X += _random.Next(-10, 11) * 0.05f;
                tileOffset.X = float.Clamp(tileOffset.X, -1, 1);
            }
        }
        private void WormCave(int x, int y, double size, int passes, float influenceX, float influenceY)
        {
            double remainingSize = size;
            double remainingPasses = passes;
            Vector2 currentTile = new Vector2(x, y);
            Vector2 tileOffset = new Vector2(influenceX, influenceY);
            while (remainingSize > 0.0 && remainingPasses > 0.0)
            {
                
                //Rectangle around point with width = remainingStrength and height = remainingStrength
                int leftBound = Math.Max(0, (int)(currentTile.X - (remainingSize / 2)));
                int rightBound = Math.Min(WorldSize.X - 1, (int)(currentTile.X + (remainingSize / 2)));
                int topBound = Math.Max(0, (int)(currentTile.Y - (remainingSize / 2)));
                int bottomBound = Math.Min(WorldSize.Y - 1, (int)(currentTile.Y + (remainingSize / 2)));

                //decrease strength each pass
                remainingSize = size * (remainingPasses / passes);
                remainingPasses -= 1.0;

                for (int i = leftBound; i <= rightBound; i++)
                {
                    for (int j = topBound; j <= bottomBound; j++)
                    {
                        //check distance in a diamond shape + a random offset
                        if (Math.Abs((double)i - currentTile.X) + Math.Abs((double)j - currentTile.Y) < size / 2 * (1.0 + (_random.Next(-10, 11) * 0.015)))
                        {
                            SetTile(i, j, 0);
                        }
                    }
                }
                currentTile += tileOffset;
                tileOffset.X += _random.Next(-10, 11) * 0.05f;
                tileOffset.X = float.Clamp(tileOffset.X, -1, 1);
            }
        }
        private void GenerateRoom(int x, int y, int width, int height, ushort tileID, ushort wallID, bool platforms = false, bool chest = false, bool leftDoor = false, bool rightDoor = false, bool windows = false)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                    {
                        SetTile(x + i, y - j, tileID);
                    }
                    else
                    {
                        SetTile(x + i, y - j, 0);
                        SetWall(x + i, y - j, wallID);
                    }
                }
            }
            if (platforms)
            {
                int platformWidth = _random.Next(2, 5);
                int platformLocation = _random.Next(1, width - platformWidth);
                for (int i = platformLocation; i < platformLocation + platformWidth; i++)
                {
                    SetTile(x + i, y, 13);
                    SetWall(x + i, y, wallID);
                }
            }
            if (chest)
            {
                int chestLocation = _random.Next(1, width - 2);
                SetLargeTile(x + chestLocation, y - 1, 8);
            }
            if (leftDoor)
            {
                SetLargeTile(x, y - 1, 9);
            }
            if (rightDoor)
            {
                SetLargeTile(x + width - 1, y - 1, 9);
            }
            if (windows)
            {
                for (int i = -1; i < 1; i++)
                {
                    for (int j = -1; j < 1; j++)
                    {
                        SetWall(x + width / 2 - width / 4 + i, y - height / 2 + j, 0);
                        SetWall(x + width / 2 + width / 4 + i, y - height / 2 + j, 0);
                    }
                }
            }
        }
        private Rectangle GenerateBuilding(int x, int y)
        {
            //TODO: make an entrance to the building before if there is one by adjusting the floor height of one of the layers
            //generate building
            int numFloors = _random.Next(1, 5);
            int width = _random.Next(10, 15);
            int floorHeight = _random.Next(6, 8);
            
            for (int i = 0; i <= floorHeight * (numFloors - 1); i += floorHeight - 1)
            {
                GenerateRoom(x, y - i, width, floorHeight, 14, 5, i != 0, _random.NextSingle() < 0.25f && i != 0, i == 0, windows: i != 0);
            }
            if (_random.Next(0, 2) == 0)
            {
                int topOffset = _random.Next(-5, 5);
                Point topMiddle = new Point(x + (width / 2) + topOffset, y - numFloors * (floorHeight - 1));
                for (int i = topMiddle.X - width; i < topMiddle.X + (width / 2); i++)
                {
                    for (int j = topMiddle.Y; j < topMiddle.Y + (width / 2); j++)
                    {
                        if (Math.Sqrt(Math.Pow(i - topMiddle.X, 2) + Math.Pow(j - topMiddle.Y, 2)) < width / 2 + _random.Next(-2, 3))
                        {
                            if (GetTileID(i, j) != 9)
                            {
                                SetTile(i, j, 0);
                            }
                            SetWall(i, j, 0);
                        }
                    }
                }
            }
            //fill in bottom
            for (int i = 0; i < width; i++)
            {
                int j = 1;
                while (!TileDatabase.TileHasProperties(GetTileID(x + i, y + j), TileProperty.Solid))
                {
                    SetTile(x + i, y + j, 14);
                    j++;
                }
            }
            return new Rectangle(x, y - (floorHeight * numFloors), width, floorHeight * numFloors);
        }
        private void GenerateLake(int x, int y)
        {
            float size = _random.Next(8, 15);
            int passes = _random.Next(5, 10);
            float remainingSize = size;
            float remainingPasses = passes;
            Vector2 currentTile = new Vector2(x, y);
            Vector2 tileOffset = new Vector2(_random.Next(-10, 11) * 0.01f, 0.5f);
            while (remainingSize > 0.0 && remainingPasses > 0.0)
            {
                float xRadius = (remainingSize / 2.0f) * MathHelper.Lerp(0.3f, 1.4f, remainingPasses / passes);
                float yRadius = (remainingSize / 4.0f) * MathHelper.Lerp(5.0f, 1.0f, remainingPasses / passes);
                int leftBound = Math.Max(0, (int)(currentTile.X - xRadius));
                int rightBound = Math.Min(WorldSize.X - 1, (int)(currentTile.X + xRadius));
                int topBound = Math.Max(0, (int)(currentTile.Y - yRadius));
                int bottomBound = Math.Min(WorldSize.Y - 1, (int)(currentTile.Y + yRadius));

                remainingSize = size * (remainingPasses / passes);
                remainingPasses -= 1.0f;
                for (int i = leftBound; i <= rightBound; i++)
                {
                    for (int j = topBound; j <= bottomBound; j++)
                    {
                        float dx = (i - currentTile.X) / xRadius;
                        float dy = (j - currentTile.Y) / yRadius;
                        if (dx * dx + dy * dy < 1)
                        {
                            SetTile(i, j, 0);
                            if (j >= y)
                            {
                                SetLiquid(i, j, MaxLiquid, true);
                            }
                        }
                    }
                }
                currentTile += tileOffset;
                tileOffset.X += _random.Next(-100, 101) * 0.05f;
                tileOffset.Y += _random.Next(-100, 101) * 0.01f;
                tileOffset.X = float.Clamp(tileOffset.X, -1, 1);
                tileOffset.Y = float.Clamp(tileOffset.Y, -1, 1);
            }
        }
    }
}
