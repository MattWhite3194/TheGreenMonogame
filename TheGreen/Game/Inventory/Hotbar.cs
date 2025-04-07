using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UIComponents;

namespace TheGreen.Game.Inventory
{
    public class Hotbar : Grid
    {
        private Item[] _inventoryItems;
        private ItemSlot[] _hotbarItemSlots;
        private int selected;
        public Hotbar(int cols, Item[] inventoryItems, int margin = 5, Vector2 position = default, Vector2 size = default) : base(cols, margin, position, size)
        {
            _inventoryItems = inventoryItems;
            _hotbarItemSlots = new ItemSlot[cols];
            for (int i = 0; i < cols; i++)
            {
                int index = i;
                _hotbarItemSlots[i] = new ItemSlot(Vector2.Zero, ContentLoader.ItemSlotTexture, new Color(34, 139, 34, 200));
                AddGridItem(_hotbarItemSlots[i]);
                _hotbarItemSlots[i].OnGuiInput += (@event) => OnItemSlotGuiInput(index, @event);
            }
            SetSelected(0);
        }
        private void OnItemSlotGuiInput(int index, InputEvent @event)
        {
            if (@event.InputButton == InputButton.LeftMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                SetSelected(index);
                _hotbarItemSlots[selected].SetColor(Color.Yellow);
                InputManager.MarkInputAsHandled(@event);
            }
        }
        public Item GetSelected()
        {
            return _inventoryItems[selected];
        }
        public void SetSelected(int index)
        {
            _hotbarItemSlots[selected].SetColor(new Color(34, 139, 34, 200));
            selected = index;
            _hotbarItemSlots[selected].SetColor(Color.Yellow);
        }
        public void SetSelectedQuantity(int quantity)
        {
            if (quantity <= 0)  
                _inventoryItems[selected] = null;
            else
                _inventoryItems[selected].Quantity = quantity;
        }
        public override void HandleInput(InputEvent @event)
        {
            base.HandleInput(@event);
            if (@event.InputButton == InputButton.MiddleMouse)
            {
                if (@event.EventType == InputEventType.MouseButtonUp)
                {
                    SetSelected((selected + 1) % _hotbarItemSlots.Length);
                }
                else
                {
                    SetSelected((selected + _hotbarItemSlots.Length - 1) % _hotbarItemSlots.Length);
                }
                InputManager.MarkInputAsHandled(@event);
            }
        }
        public override void Draw(SpriteBatch spritebatch)
        {
            for (int i = 0; i < _hotbarItemSlots.Length; i++)
            {
                _hotbarItemSlots[i].Draw(spritebatch);
                _hotbarItemSlots[i].DrawItem(spritebatch, _inventoryItems[i]);
            }
        }
    }
}
