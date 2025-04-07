using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UIComponents;

namespace TheGreen.Game.Inventory
{
    public class DragItem : UIComponent
    {
        private Item _item;
        public Item Item
        {
            get { return _item; }
            set
            {
                _item = value;
            }
        }

        public DragItem(Vector2 position) : base(position)
        {
            this.Position = position;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_item == null) return;
            spriteBatch.Draw(_item.Image, Position, Color.White);
            if (_item.Stackable)
            {
                spriteBatch.DrawString(ContentLoader.GameFont, _item.Quantity + "", Position + new Vector2(Size.X / 2, Size.Y - 10) + new Vector2(0.6f, 0.6f), Color.Black, 0.0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0.0f);
                spriteBatch.DrawString(ContentLoader.GameFont, _item.Quantity + "", Position + new Vector2(Size.X / 2, Size.Y - 10), Color.White, 0.0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0.0f);
            }
        }

        public override void Update(double delta)
        {
            Position = InputManager.GetMouseWindowPosition();
        }
    }
}
