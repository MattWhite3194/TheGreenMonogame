using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Items;

namespace TheGreen.Game.Entities
{
    /// <summary>
    /// An entity that contains an item.
    /// </summary>
    public class ItemDrop : Entity
    {
        private Item _item;
        private int _maxFallSpeed = 700;
        public static Vector2 ColliderSize = new Vector2(10, 10);
        public ItemDrop(Item item, Vector2 position) : base(item.Image, position)
        {
            _item = item;
            CollidesWithTiles = true;
            this.Size = ColliderSize;
            this.Layer = CollisionLayer.ItemDrop;
        }
        public Item GetItem() { return _item; }
        public override void Update(double delta)
        {
            base.Update(delta);
            Vector2 newVelocity = Velocity;
            newVelocity.Y += Globals.GRAVITY / 2 * (float)delta;
            if (newVelocity.Y > _maxFallSpeed)
                newVelocity.Y = _maxFallSpeed;
            Velocity = newVelocity;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Point centerTilePosition = ((Position + Size / 2) / Globals.TILESIZE).ToPoint();
            spriteBatch.Draw(Image,
                new Vector2((int)Position.X, (int)Position.Y) + Origin + new Vector2(-_item.Image.Width / 2 + 5, -_item.Image.Height + 10),
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
