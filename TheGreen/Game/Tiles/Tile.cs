namespace TheGreen.Game.Tiles
{
    /// <summary>
    /// Used by WorldGen to store tiles in the tilemap.
    /// Contains only the tile id, the tile texture atlas state, and it's liquid level.
    /// </summary>
    public struct Tile
    {
        public ushort ID;
        public byte State;
        public ushort WallID;
        public byte WallState;
        public byte Liquid;
    }
}
