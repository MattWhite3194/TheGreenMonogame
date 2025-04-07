using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TheGreen.Game.Entities.Projectiles
{
    public class Projectile : Entity
    {
        private int _timeLeft;
        public Projectile(Texture2D image, Vector2 position, Vector2 size, bool hostile, bool friendly, bool collidesWithTiles, List<(int, int)> animationFrames = null) : base(image, position, size, animationFrames: animationFrames)
        {
            this.Layer = 0;
            this.CollidesWith = 0;
            this.CollidesWithTiles = collidesWithTiles;
            if (friendly)
            {
                this.Layer |= CollisionLayer.FriendlyProjectile;
                this.CollidesWith |= CollisionLayer.Enemy;
            }
            if (hostile)
            {
                this.Layer |= CollisionLayer.HostileProjectile;
                this.CollidesWith |= CollisionLayer.FriendlyProjectile;
            }

        }
    }
}
