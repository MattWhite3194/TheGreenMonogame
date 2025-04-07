

using TheGreen.Game.Entities;

namespace TheGreen.Game.Tiles
{
    internal interface ICollideableTile
    {
        void OnCollision(int x, int y, Entity entity);
    }
}
