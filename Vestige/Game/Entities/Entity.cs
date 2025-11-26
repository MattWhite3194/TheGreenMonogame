using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Vestige.Game.Drawables;

namespace Vestige.Game.Entities
{
    /// <summary>
    /// Defines a basic entity, something that moves and collides in the world
    /// </summary>
    public abstract class Entity : Sprite
    {
        /// <summary>
        /// The entity's velocity in pixels/second
        /// </summary>
        public Vector2 Velocity;
        public bool IsOnFloor = false, IsOnCeiling = false;
        public bool DrawBehindTiles = false;
        /// <summary>
        /// The entity will stop when it collides with a tile
        /// </summary>
        public bool CollidesWithTiles;
        public bool CollidesWithPlatforms;
        /// <summary>
        /// If true, the entity will not stop when colliding with a single-tile tall barrier. Instead it will move over it.
        /// </summary>
        public bool HopTiles = true;
        /// <summary>
        /// When set to false, the entity will be removed in the next update
        /// </summary>
        public bool Active = true;
        public readonly string Name;
        /// <summary>
        /// the size of the entities hit box, will be calculated relative to the entities origin
        /// </summary>
        private Point _hitboxSize;
        private Vector2 _hitboxCenter;
        protected Entity(Texture2D image, Vector2 position, Vector2 size = default, Vector2 origin = default, Point hitboxSize = default, List<(int, int)> animationFrames = null, string name = null) : base(image, position, size, origin: origin, animationFrames: animationFrames)
        {
            Name = name != null ? name : "";
            _hitboxSize = hitboxSize != default ? hitboxSize : Size.ToPoint();
            _hitboxCenter = Origin - (_hitboxSize.ToVector2() / 2.0f);
        }
        public virtual void OnCollision(Entity entity) { }
        public virtual void OnLiquidCollision(int x, int y, ushort liquidID) { }
        public virtual CollisionRectangle GetBounds()
        {
            return new CollisionRectangle(Position + _hitboxCenter, _hitboxSize);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Point centerTilePosition = ((Position + Origin) / Vestige.TILESIZE).ToPoint();
            spriteBatch.Draw(Image,
                Vector2.Round(Position + Origin),
                Animation?.AnimationRectangle ?? null,
                Main.LightEngine.GetLight(centerTilePosition.X, centerTilePosition.Y),
                Rotation,
                Origin,
                Scale,
                FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0.0f
            );
        }
        public virtual string GetTooltipDisplay()
        {
            return Name;
        }
    }
}
