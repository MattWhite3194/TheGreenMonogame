using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TheGreen.Game.Drawables
{
    public class Sprite
    {
        public Texture2D Image;
        public Color Color;
        public Vector2 Position;
        public AnimationComponent Animation;
        public bool FlipSprite = false;
        public float Rotation = 0.0f;
        public float Scale = 1.0f;
        private Vector2 _size;
        public Vector2 Size
        {
            get
            {
                if (_size == default)
                    return Image == null ? Vector2.Zero : new Vector2(Image.Width, Image.Height);
                return _size;
            }
            set
            {
                _size = value;
            }
        }
        private Vector2 _origin;
        /// <summary>
        /// The origin at which the sprite is scaled from
        /// </summary>
        public Vector2 Origin
        {
            get
            {
                if (_origin == default)
                    return new Vector2(Size.X / 2, Size.Y / 2);
                return _origin;
            }
            set
            {
                _origin = value;
            }
        }

        public Sprite(Texture2D image, Vector2 position, Vector2 size = default, Vector2 origin = default, Color color = default, List<(int, int)> animationFrames = null)
        {
            this.Image = image;
            this.Position = position;
            if (size != default)
            {
                this.Size = size;
            }
            if (origin != default)
            {
                this.Origin = origin;
            }
            if (animationFrames != null)
            {
                this.Animation = new AnimationComponent(size, animationFrames);
            }
            Color = color == default ? Color.White : color;
        }

        public virtual void Update(double delta)
        {
            Animation?.Update(delta);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image,
                new Vector2((int)Position.X, (int)Position.Y) + Origin,
                Animation?.AnimationRectangle ?? new Rectangle(Point.Zero, Size.ToPoint()),
                Color,
                Rotation,
                Origin,
                Scale,
                FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0.0f
            );
        }
    }
}
