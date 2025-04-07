using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Items;
using TheGreen.Game.UIComponents;

namespace TheGreen.Game.Inventory
{
    public class ItemSlot : UIComponent
    {
        public ItemSlot(Vector2 position, Texture2D image, Color color) : base(position, image, color) { }
        public void DrawItem(SpriteBatch spriteBatch, Item item)
        {
            if (item == null) return;

            spriteBatch.Draw(
                item.Image,
                Position + new Vector2((int)(Size.X - item.Image.Width) / 2, (int)(Size.Y - item.Image.Height) / 2), 
                Color.White
                );
            
            if (item.Stackable)
            {
                spriteBatch.DrawString(ContentLoader.GameFont, item.Quantity + "", Position + new Vector2(Size.X / 2, Size.Y - 10) + new Vector2(0.6f, 0.6f), Color.Black, 0.0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0.0f);
                spriteBatch.DrawString(ContentLoader.GameFont, item.Quantity + "", Position + new Vector2(Size.X / 2, Size.Y - 10), Color.White, 0.0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0.0f);
            }
        }
    }
}
