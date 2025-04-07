using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TheGreen.Game.Drawables;

namespace TheGreen.Game.Entities
{
    /// <summary>
    /// Defines a basic entity, something that moves and collides in the world
    /// </summary>
    public abstract class Entity : Sprite
    {
        public Vector2 Velocity;
        public bool IsOnFloor = false, IsOnCeiling = false;
        public bool DrawBehindTiles = false;
        /// <summary>
        /// The entity will stop when it collides with a tile
        /// </summary>
        public bool CollidesWithTiles;
        public bool Active = true;
        /// <summary>
        /// The layer other entities receive when they collide with this entity
        /// </summary>
        public CollisionLayer Layer;
        /// <summary>
        /// The collision layers this entity receives collision events from
        /// </summary>
        public CollisionLayer CollidesWith;

        protected Entity(Texture2D image, Vector2 position, Vector2 size = default, Vector2 origin = default, List<(int, int)> animationFrames = null) : base(image, position, size, origin: origin, animationFrames: animationFrames)
        {
        }

        public virtual void OnCollision(Entity entity)
        {

        }
        public virtual void OnTileCollision()
        {

        }
        public virtual Rectangle GetBounds()
        {
            return new Rectangle(Position.ToPoint(), Size.ToPoint());
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Point centerTilePosition = ((Position + Size / 2) / Globals.TILESIZE).ToPoint();
            spriteBatch.Draw(Image,
                new Vector2((int)Position.X, (int)Position.Y) + Origin,
                Animation?.AnimationRectangle ?? null,
                Main.LightEngine.GetLight(centerTilePosition.X, centerTilePosition.Y),
                Rotation,
                Origin,
                Scale,
                FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0.0f
            );
        }
    }
}
