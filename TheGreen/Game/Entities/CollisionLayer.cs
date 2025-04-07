using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGreen.Game.Entities
{
    [Flags]
    public enum CollisionLayer
    {
        None = 1 << 0,
        Player = 1 << 1,
        Enemy = 1 << 2,
        FriendlyProjectile = 1 << 3,
        HostileProjectile = 1 << 4,
        /// <summary>
        /// Item drops in the world
        /// </summary>
        ItemDrop = 1 << 5,
        /// <summary>
        /// The players item hitbox
        /// </summary>
        ItemCollider = 1 << 6,
    }
}
