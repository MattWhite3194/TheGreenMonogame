using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace TheGreen.Game.UIComponents
{
    public class Label : UIComponent
    {
        protected string _text;
        private int _borderRadius;
        private int _borderThickness;
        private Vector2 _padding;
        private Color _borderColor;
        protected Color _textColor;
        protected Vector2 _stringPosition;
        private int _maxWidth;
        public Label(Vector2 position, string text, Vector2 padding, int borderRadius = 0, Color color = default(Color), Color textColor = default(Color),
            GraphicsDevice graphicsDevice = null, Texture2D image = null, bool drawCentered = false, int maxWidth = 0, float rotation = 0.0f, float scale = 1.0f) : base(position, image, color, graphicsDevice, drawCentered, rotation, scale)
        {
            this.color = color;
            this._textColor = textColor == default(Color) ? Color.White : textColor;
            this._padding = padding;
            this._borderRadius = borderRadius;
            this._maxWidth = maxWidth;

            SetText(text);
            this.OnPositionChanged += () => UpdateStringPosition();
        }

        private void UpdateStringPosition()
        {
            Vector2 stringSize = ContentLoader.GameFont.MeasureString(_text);
            _stringPosition.X = _drawPosition.X + Size.X / 2 - stringSize.X / 2;
            _stringPosition.Y = _drawPosition.Y + Size.Y / 2 - stringSize.Y / 2;
        }

        private Vector2 WrapText(int maxWidth)
        {
            Vector2 stringSize = ContentLoader.GameFont.MeasureString(_text);
            if (maxWidth == 0)
                return stringSize;
            if (stringSize.X < maxWidth)
                return new Vector2(maxWidth, stringSize.Y);
            float characterWidth = stringSize.X / _text.Length;
            int charsPerLine = (int)(maxWidth / characterWidth);
            string newText = "";
            int textIndex = 0;
            while (textIndex < _text.Length)
            {
                newText += _text[textIndex];
                if ((textIndex + 1) % charsPerLine == 0)
                    newText += "\n";
                textIndex++;
            }
            _text = newText;
            return new Vector2(maxWidth, ContentLoader.GameFont.MeasureString(newText).Y);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (image != null)
            {
                spriteBatch.Draw(image, _drawPosition, null, color, _rotation, Vector2.Zero, _scale, SpriteEffects.None, 0.0f);
            }
            spriteBatch.DrawString(ContentLoader.GameFont, _text, _stringPosition + _origin + new Vector2(1, 1) * _scale, Color.Black, _rotation, _origin, _scale, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(ContentLoader.GameFont, _text, _stringPosition + _origin, _textColor, _rotation, _origin, _scale, SpriteEffects.None, 0.0f);
            
        }

        public void SetText(string text)
        {
            _text = text;
            if (image != null)
            {
                Size = new Vector2(image.Width, image.Height);
                WrapText(image.Width - (int)_padding.X * 2);
            }
            else if (_graphicsDevice != null)
            {
                Vector2 imageSize = Vector2.Add(WrapText(_maxWidth), 2 * _padding);
                this.image = new Texture2D(_graphicsDevice, (int)imageSize.X, (int)imageSize.Y);
                var data = Enumerable.Repeat(Color.White, (int)(imageSize.X * imageSize.Y)).ToArray();
                this.image.SetData<Color>(data);
                Size = imageSize;
            }
            else
            {
                Size = ContentLoader.GameFont.MeasureString(_text);
            }
            UpdateStringPosition();
        }
    }
}
