using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TheGreen.Game.Items;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration.WorldUpdaters;

namespace TheGreen.Game.WorldGeneration
{
    public class WorldGen
    {
        private static WorldGen _world = null;
        private WorldGen()
        {

        }
        public static WorldGen World
        {
            get
            {
                if (_world == null)
                {
                    _world = new WorldGen();
                }
                return _world;
            }
        }
        private Tile[] _tiles;
        private readonly Random _random = new Random();
        private Point _spawnTile;
        public Point SpawnTile
        {
            get { return _spawnTile; }
        }
        private int _dirtDepth = 20;
        private int _grassDepth = 8;
        private int _surfaceHeight;
        public Point WorldSize;
        private int[,,] gradients = new int[256, 256, 2];
        public Texture2D Map;
        /// <summary>
        /// Quick access to surrounding tile points
        /// </summary>
        public readonly Point[] SurroundingTiles = { new Point(0, -1), new Point(0, 1), new Point(-1, 0), new Point(1, 0) };

        /// <summary>
        /// Stores the location and damage information of any tiles that are actively being mined by the player
        /// </summary>
        private Dictionary<Point, DamagedTile> _minedTiles = new Dictionary<Point, DamagedTile>();
        
        private List<WorldUpdater> _worldUpdaters;
        private LiquidUpdater _liquidUpdater;
        private OverlayTileUpdater _overlayTileUpdater;
        private Dictionary<Point, Item[]> _tileInventories;
        public class DamagedTile
        {
            /// <summary>
            /// The health left on the tile
            /// </summary>
            public int Health;
            /// <summary>
            /// The time left before the tile is removed from the dictionary and any damage done is reset
            /// </summary>
            public double Time;
            public DamagedTile(int health, int time)
            {
                this.Health = health;
                this.Time = time;
            }
        }
        public void GenerateWorld(int size_x, int size_y)
        {
            WorldSize = new Point(size_x, size_y);
            _tiles = new Tile[size_x * size_y];
            _tileInventories = new Dictionary<Point, Item[]>();
            int[] surfaceNoise = Generate1DNoise(size_x, 50, 10, 6, 0.5f);
            int[] surfaceTerrain = new int[size_x];

            _surfaceHeight = size_y;
            //place stone and get surface height
            for (int i = 0; i < size_x; i++)
            {
                for (int j = size_y / 2 - size_y / 4 + surfaceNoise[i]; j < size_y; j++)
                {
                    SetInitialTile(i, j, 4);
                    SetInitialWall(i, j, 4);
                    surfaceTerrain[i] = size_y / 2 - size_y / 4 + surfaceNoise[i];

                    if (size_y - surfaceTerrain[i] < _surfaceHeight)
                        _surfaceHeight = size_y - surfaceTerrain[i];
                }
            }
            //place dirt
            for (int i = 0; i < size_x; i++)
            {
                for (int j = 0; j < _dirtDepth; j++)
                {
                    if (j > 4)
                    {
                        SetInitialWall(i, surfaceTerrain[i], 0);
                    }
                    SetInitialTile(i, surfaceTerrain[i] + j, 1);
                }
            }

            _spawnTile = new Point(size_x / 2, surfaceTerrain[size_x / 2]);
            //generate caves
            InitializeGradients();
            double[,] perlinNoise = GeneratePerlinNoiseWithOctaves(size_x, _surfaceHeight - _dirtDepth - 1, scale: 40, octaves: 5, persistence: 0.5);
            //threshhold cave noise
            for (int x = 0; x < size_x; x++)
            {
                for (int y = 0; y < _surfaceHeight - _dirtDepth - 1; y++)
                {
                    if (perlinNoise[y, x] < -0.1)
                    {
                        RemoveInitialTile(x, size_y - _surfaceHeight + _dirtDepth + y);
                    }
                }
            }

            perlinNoise = null;

            //calculate tile states
            for (int i = 1; i < size_x - 1; i++)
            {
                for (int j = 1; j < size_y - 1; j++)
                {
                    SetTileState(i, j, TileDatabase.GetTileData(GetTileID(i, j)).GetUpdatedTileState(i, j));
                    UpdateWallState(i, j);
                }
            }

            //spread grass
            for (int i = 0; i < size_x; i++)
            {
                for (int j = 0; j < _grassDepth; j++)
                {
                    if (GetTileID(i, surfaceTerrain[i] + j) == 1 && GetTileState(i, surfaceTerrain[i] + j) != 255)
                    {
                        SetInitialTile(i, surfaceTerrain[i] + j, 2);
                    }
                }
            }

            int minTreeDistance = 5;
            int lastTreeX = 10;
            //Plant Trees
            for (int i = 10; i < size_x - 10; i++)
            {
                if (_random.NextDouble() < 0.2 && i - lastTreeX > minTreeDistance)
                {
                    GenerateTree(i, surfaceTerrain[i] - 1);
                    lastTreeX = i;
                }
            }

            //generate map file
            Color[] colorData = new Color[size_x * size_y];
            for (int x = 0; x < size_x; x++)
            {
                for (int y = 0; y < size_y; y++)
                {
                    colorData[x + y * size_x] = TileDatabase.GetTileData(GetTileID(x, y)).MapColor;
                }
            }
            Map.SetData(colorData);
            string gamePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TheGreen");
            if (!Directory.Exists(gamePath))
            {
                Directory.CreateDirectory(gamePath);
            }
            Stream stream = File.Create(gamePath + "/map.jpg");
            Map.SaveAsJpeg(stream, size_x, size_y);
            stream.Close();
        }

        private byte[] _randTreeTileStates = [0, 2, 8, 10];
        private void GenerateTree(int x, int y)
        {
            
            //generate base
            SetInitialTile(x, y, 5);
            SetTileState(x, y, 128);
            if (GetTileID(x-1, y) == 0)
            {
                SetInitialTile(x - 1, y, 5);
                SetTileState(x - 1, y, 62);
            }
            if (GetTileID(x + 1, y) == 0)
            {
                SetInitialTile(x + 1, y, 5);
                SetTileState(x + 1, y, 130);
            }
            //Generate trunk
            int height = _random.Next(5, 20);
            for (int h = 1; h < height; h++)
            {
                SetInitialTile(x, y - h, 5);
                SetTileState(x, y - h, _randTreeTileStates[_random.Next(0, _randTreeTileStates.Length)]);
            }

            //Add tree top
            SetInitialTile(x, y - height, 6);
            SetTileState(x, y - height, 0);
        }

        public void LoadWorld()
        {
            //TODO: implementation
            //get size from file
            _tiles = new Tile[0];
        }
        /// <summary>
        /// Called when a player starts a world. Use this to start any frame or tick updates.
        /// </summary>
        public void InitializeGameUpdates()
        {
            _worldUpdaters = new List<WorldUpdater>();
            _liquidUpdater = new LiquidUpdater(0.01);
            _overlayTileUpdater = new OverlayTileUpdater(1);
            _worldUpdaters.AddRange([ _liquidUpdater, _overlayTileUpdater]);
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
                damagedTileData.Time += delta;
                if (damagedTileData.Time > 5 || GetTileID(point.X, point.Y) == 0)
                {
                    _minedTiles.Remove(point);
                }
                else
                    _minedTiles[point] = damagedTileData;
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
            TileData tileData = TileDatabase.GetTileData(tileID);
            if (!tileData.CanTileBeDamaged(coordinates.X, coordinates.Y))
                return;
            DamagedTile damagedTileData = _minedTiles.ContainsKey(coordinates)? _minedTiles[coordinates] : new DamagedTile(tileData.Health, 0);
            damagedTileData.Health = damagedTileData.Health - damage;
            damagedTileData.Time = 0;
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
        public bool IsTileInBounds(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < WorldSize.X && y < WorldSize.Y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="height"></param>
        /// <param name="frequency"></param>
        /// <param name="octaves">Number of passes. Will make the noise more detailed</param>
        /// <param name="persistance">Value less than 1. Reduces height of next octave.</param>
        /// <returns></returns>
        public int[] Generate1DNoise(int size, float height, float frequency, int octaves, float persistance)
        {
            float[] noise = new float[size];
            int[] intNoise = new int[size];
            for (int octave = 0; octave < octaves; octave++)
            {
                float[] values = new float[(int)frequency];
                int xOffset = (int)(4000 / frequency);
                int currentValue = 0;
                int nextValue = 1;

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (float)_random.NextDouble() * height + noise[(i * xOffset) % size];
                }

                int step = 0;
                for (int i = 0; i < size; i++)
                {
                    if (currentValue >= values.Length)
                    {
                        currentValue = 0;
                    }
                    if (nextValue >= values.Length)
                    {
                        nextValue = 0;
                    }
                    noise[i] = CubicInterpolation(currentValue * xOffset, values[currentValue], nextValue * xOffset, values[nextValue], i);
                    step++;
                    if (step == xOffset)
                    {
                        currentValue++;
                        nextValue++;
                        step = 0;
                    }
                }
                frequency *= 2;
                height *= persistance;
            }

            for (int i = 0; i < intNoise.Length; i++)
            {
                intNoise[i] = (int)(noise[i]);
            }

            return intNoise;
        }

        private float CubicInterpolation(float x0, float y0, float x1, float y1, float t)
        {
            float normalized_t = (t - x0) / (x1 - x0);

            float mu2 = (1.0f - (float)Math.Cos(normalized_t * Math.PI)) / 2.0f;

            return y0 * (1.0f - mu2) + y1 * mu2;
        }

        void InitializeGradients()
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

        double GetInfluenceValue(double x, double y, int Xgrad, int Ygrad)
        {
            return (gradients[Xgrad % 256, Ygrad % 256, 0] * (x - Xgrad)) +
                   (gradients[Xgrad % 256, Ygrad % 256, 1] * (y - Ygrad));
        }

        double Lerp(double v0, double v1, double t)
        {
            return (1 - t) * v0 + t * v1;
        }

        double Fade(double t)
        {
            return 3 * Math.Pow(t, 2) - 2 * Math.Pow(t, 3);
        }

        double Perlin(double x, double y)
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

        double[,] GeneratePerlinNoiseWithOctaves(int width, int height, double scale = 100.0, int octaves = 4, double persistence = 0.5)
        {
            double[,] noise = new double[height, width];
            double amplitude = 1.0;
            double frequency = 1.0;
            double maxValue = 0;  // To normalize the result

            for (int octave = 0; octave < octaves; octave++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // Apply frequency and scale to the coordinates for each octave
                        noise[y, x] += Perlin(x / scale * frequency, y / scale * frequency) * amplitude;
                    }
                }

                maxValue += amplitude;
                amplitude *= persistence;  // Amplitude decreases with each octave
                frequency *= 2;  // Frequency doubles for each octave
            }

            // Normalize the noise to be between -1 and 1
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    noise[y, x] /= maxValue;
                }
            }

            return noise;
        }
        public ushort GetTileID(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].ID;
        }
        public ushort GetWallID(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].WallID;
        }
        public bool SetTile(int x, int y, ushort ID)
        {
            if (ID != 0 && TileDatabase.GetTileData(ID).VerifyTile(x, y) != 1)
                return false;
            if (TileDatabase.TileHasProperty(ID, TileProperty.LargeTile))
            {
                SetLargeTile(x, y, ID);
                //TEMPORARY
                //TODO: add tiles updated to a list, and then update the tiles in the list and around the tiles in the list
                return true;
            }
            else 
                _tiles[y * WorldSize.X + x].ID = ID;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (GetLiquid(x + i, y + j) != 0)
                    {
                        _liquidUpdater.QueueLiquidUpdate(x + i, y + j);
                    }

                    ushort tileID = GetTileID(x + i, y + j);

                    if (TileDatabase.GetTileData(tileID).VerifyTile(x + i, y + j) == -1)
                    {
                        RemoveTile(x + i, y + j);
                        continue;
                    }

                    byte state = TileDatabase.GetTileData(tileID).GetUpdatedTileState(x + i, y + j);
                    SetTileState(x + i, y + j, state);

                    if (TileDatabase.TileHasProperty(tileID, TileProperty.Overlay))
                    {
                        if (state == 255)
                            _tiles[(y + j) * WorldSize.X + (x + i)].ID = TileDatabase.GetTileData(tileID).BaseTileID;
                        else
                            _overlayTileUpdater.EnqueueOverlayTile(x + i, y + j, tileID);
                    }
                }
            }
            return true;
        }
        public void RemoveTile(int x, int y)
        {
            ushort tileID = GetTileID(x, y);
            if (TileDatabase.TileHasProperty(tileID, TileProperty.LargeTile))
            {
                RemoveLargeTile(x, y, tileID);
            }
            else
            {
                SetTile(x, y, 0);
            }
            Item item = ItemDatabase.InstantiateItemByTileID(tileID);
            if (item != null)
            {
                Main.EntityManager.AddItemDrop(item, new Vector2(x, y) * Globals.TILESIZE);
            }

        }
        private void SetLargeTile(int x, int y, ushort ID)
        {
            if (!(TileDatabase.GetTileData(ID) is LargeTileData largeTileData))
                return;
            Point topLeft = largeTileData.GetTopLeft(x, y) ;
            for (int i = 0; i < largeTileData.TileSize.X; i++)
            {
                for (int j = 0; j < largeTileData.TileSize.Y; j++)
                {
                    _tiles[(topLeft.Y + j) * WorldSize.X + (topLeft.X + i)].ID = ID;
                    SetTileState(topLeft.X + i, topLeft.Y + j, (byte)(j * 10 + i));
                }
            }
        }
        private void RemoveLargeTile(int x, int y, ushort ID)
        {
            if (!(TileDatabase.GetTileData(ID) is LargeTileData largeTileData))
                return;
            Point topLeft = largeTileData.GetTopLeft(x, y);
            for (int i = 0; i < largeTileData.TileSize.X; i++)
            {
                for (int j = 0; j < largeTileData.TileSize.Y; j++)
                {
                    _tiles[(topLeft.Y + j) * WorldSize.X + (topLeft.X + i)].ID = 0;
                    SetTileState(topLeft.X + i, topLeft.Y + j, 0);
                }
            }
        }
        public void SetWall(int x, int y, byte WallID)
        {
            _tiles[y * WorldSize.X + x].WallID = WallID;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    UpdateWallState(x + i, y + j);
                }
            }
        }

        private void SetInitialTile(int x, int y, ushort ID)
        {
            _tiles[y * WorldSize.X + x].ID = ID;
        }

        private void SetInitialWall(int x, int y, byte WallID)
        {
            _tiles[y * WorldSize.X + x].WallID = WallID;
        }

        private void RemoveInitialTile(int x, int y)
        {
            _tiles[y * WorldSize.X + x].ID = 0;
        }

        public void SetTileState(int x, int y, byte state)
        {
            _tiles[y * WorldSize.X + x].State = state;
        }

        public Dictionary<Point, DamagedTile> GetMinedTiles()
        {
            return _minedTiles;
        }
        private void UpdateWallState(int x, int y)
        {
            if (GetWallID(x, y) == 0)
            {
                _tiles[y * WorldSize.X + x].WallState = 0;
                return;
            }
            //Important: if a corner doesn't have both sides touching it, it won't be counted
            ushort top = GetWallID(x, y - 1);
            ushort right = GetWallID(x + 1, y);
            ushort bottom = GetWallID(x, y + 1);
            ushort left = GetWallID(x - 1, y);

            _tiles[y * WorldSize.X + x].WallState = (byte)((Math.Sign(top) * 2) + (Math.Sign(right) * 8) + (Math.Sign(bottom) * 32) + (Math.Sign(left) * 128));
        }
        public byte GetTileState(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].State;
        }
        public byte GetWallState(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].WallState;
        }
        public byte GetLiquid(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].Liquid;
        }
        public void SetLiquid(int x, int y, byte amount)
        {
            if (amount > 0)
                _liquidUpdater.QueueLiquidUpdate(x, y);
            _tiles[y * WorldSize.X + x].Liquid = amount;
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
            if (_tileInventories.ContainsKey(coordinates))
                return _tileInventories[coordinates];
            return null;
        }
    }
}
