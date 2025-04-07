using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UIComponents;

namespace TheGreen.Game.Inventory
{
    public class Inventory : Grid
    {
        private Item[] _inventoryItems;
        private ItemSlot[] _inventoryItemSlots;
        private DragItem _dragItem;

        public Inventory(int cols, DragItem dragItem, Item[] inventoryItems, int margin = 5, Vector2 position = default, Vector2 size = default, Color itemSlotColor = default) : base(cols, margin, position, size)
        {
            _inventoryItems = inventoryItems;
            _inventoryItemSlots = new ItemSlot[_inventoryItems.Length];
            this._dragItem = dragItem;
            for (int i = 0; i < _inventoryItemSlots.Length; i++)
            {
                int index = i;
                _inventoryItemSlots[i] = new ItemSlot(Vector2.Zero, ContentLoader.ItemSlotTexture, itemSlotColor == default ? Color.ForestGreen : itemSlotColor);
                AddGridItem(_inventoryItemSlots[i]);
                _inventoryItemSlots[i].OnGuiInput += (@event) => OnItemSlotGuiInput(index, @event);
            }
        }

        private void OnItemSlotGuiInput(int index, InputEvent @event)
        {
            if (@event.InputButton == InputButton.LeftMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                PlaceItem(index);
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.RightMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                SplitItem(index);
                InputManager.MarkInputAsHandled(@event);
            }
        }
        public Item AddItem(Item item)
        {
            int emptyIndex = -1;
            int remainingQuantity = item.Quantity;
            for (int i = 0; i < _inventoryItems.Length; i++)
            {
                //get first empty slot starting from the back of the array
                if (emptyIndex == -1 && _inventoryItems[_inventoryItems.Length - 1 - i] == null)
                    emptyIndex = _inventoryItems.Length - 1 - i;
                else if (emptyIndex != -1 && !item.Stackable)
                    break;

                if (item.Stackable && item.ID == (_inventoryItems[i]?.ID ?? -1))
                {
                    if (_inventoryItems[i].Quantity >= 30) //MAXSTACK
                        continue;
                    int newQuantity = _inventoryItems[i].Quantity + remainingQuantity;
                    remainingQuantity = int.Clamp(newQuantity - 30, 0, 30);
                    newQuantity -= remainingQuantity;

                    SetItemQuantity(i, newQuantity);
                }
            }
            item.Quantity = remainingQuantity;
            if (emptyIndex != -1 && item.Quantity > 0)
            {
                SetItem(item, emptyIndex);
                return null;
            }
            else if (item.Quantity == 0)
                return null;
            item.Quantity = remainingQuantity;
            return item;
        }

        private void PlaceItem(int index)
        {
            if (_dragItem.Item == null && _inventoryItems[index] == null)
            {
                return;
            }
            if (_dragItem.Item == null)
            {
                _dragItem.Item = _inventoryItems[index];
                SetItem(null, index);
            }
            else if (_inventoryItems[index] == null)
            {
                SetItem(_dragItem.Item, index);
                _dragItem.Item = null;
            }
            else if (_inventoryItems[index].ID == _dragItem.Item.ID && _inventoryItems[index].Stackable)
            {
                SetItemQuantity(index, _inventoryItems[index].Quantity + _dragItem.Item.Quantity);
                _dragItem.Item = null;
            }
            else
            {
                Item temp = _inventoryItems[index];
                SetItem(_dragItem.Item, index);
                _dragItem.Item = temp;
            }
        }

        private void SplitItem(int index)
        {
            if (_inventoryItems[index] == null)
                return;
            if (_dragItem.Item == null)
            {
                if (_inventoryItems[index].Stackable)
                {
                    Item splitItem = ItemDatabase.InstantiateItemByID(_inventoryItems[index].ID);
                    splitItem.Quantity = (int)Math.Ceiling(_inventoryItems[index].Quantity / 2.0f);
                    _dragItem.Item = splitItem;
                    SetItemQuantity(index, _inventoryItems[index].Quantity - splitItem.Quantity);
                }
                else
                {
                    _dragItem.Item = _inventoryItems[index];
                    SetItem(null, index);

                }
            }
        }
        private void SetItem(Item item, int index)
        {
            _inventoryItems[index] = item;
        }
        private void SetItemQuantity(int index, int quantity)
        {
            _inventoryItems[index].Quantity = quantity;
            SetItem(_inventoryItems[index], index);
            if (quantity <= 0)
            {
                SetItem(null, index);
            }
        }
        public override void Draw(SpriteBatch spritebatch)
        {
            for (int i = 0; i < _inventoryItemSlots.Length; i++)
            {
                _inventoryItemSlots[i].Draw(spritebatch);
                _inventoryItemSlots[i].DrawItem(spritebatch, _inventoryItems[i]);
            }
        }
    }
}
