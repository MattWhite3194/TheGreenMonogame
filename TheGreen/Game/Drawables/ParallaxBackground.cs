using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheGreen.Game.Drawables
{
    public class ParallaxBackground
    {
        private Texture2D _backgroundImage;
        private Vector2 Offset;
        public Vector2 Speed;
        /// <summary>
        /// The height in the world that this parallax background should be draw in
        /// </summary>
        private int _maxDrawDepth;
        private int _minDrawDepth;
        public bool Active = true;
        private Vector2 _currentPosition;
        public ParallaxBackground(Texture2D backgroundImage, Vector2 speed, Vector2 initialPlayerPosition, int maxDrawDepth, int minDrawDepth) 
        { 
            this._backgroundImage = backgroundImage;
            this.Speed = speed;
            this._currentPosition = initialPlayerPosition;
            this._maxDrawDepth = maxDrawDepth;
            this._minDrawDepth = minDrawDepth;
            this.Offset = new Vector2(0, (maxDrawDepth - initialPlayerPosition.Y) * Speed.Y);
        }

        public void Update(double delta, Vector2 position)
        {
            //Only Activate this parallax if the player is above the maxdrawdepth
            Active = position.Y < _maxDrawDepth && position.Y > _minDrawDepth;
            //offset increases as the player is moving left, and decreases as the player is moving right
            Offset.X -= (position.X - _currentPosition.X) * Speed.X;
            Offset.Y = (_maxDrawDepth - position.Y) * Speed.Y;
            _currentPosition = position;
            if (Offset.X < 0)
            {
                Offset.X = _backgroundImage.Width + Offset.X;
            }
            if (Offset.X > _backgroundImage.Width)
            {
                Offset.X = Offset.X - _backgroundImage.Width;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            for (int i = 0; i <= (int)Math.Ceiling(Globals.NativeResolution.X / (float)_backgroundImage.Width); i++)
            {
                spriteBatch.Draw(
                    _backgroundImage,
                    new Vector2((int)(Offset.X + (i * _backgroundImage.Width) - _backgroundImage.Width), (int)(Globals.NativeResolution.Y - _backgroundImage.Height + Offset.Y)),
                    color
                    );
            }
        }
    }
}
