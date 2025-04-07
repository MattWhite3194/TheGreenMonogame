using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using TheGreen.Game.Input;
using TheGreen.Game.Inventory;
using TheGreen.Game.Items;

namespace TheGreen.Game.Entities
{
    public class ItemCollider : Entity
    {
        private InventoryManager _inventory;
        public Item Item;
        public bool ItemActive = false;
        private double _holdTime;
        private bool _leftReleased = false;
        private bool _canUseItem = true;
        public ItemCollider(InventoryManager inventory) : base(null, default, default)
        {
            this._inventory = inventory;
            this.Layer = CollisionLayer.ItemCollider;
        }
        public void HandleInput(InputEvent @event)
        {
            if (@event is MouseInputEvent mouseInputEvent)
            {
                if (mouseInputEvent.InputButton == InputButton.LeftMouse)
                {
                    if (mouseInputEvent.EventType == InputEventType.MouseButtonDown)
                    {
                        if (ItemActive) return;
                        _holdTime = 0.0f;
                        _canUseItem = true;
                        _leftReleased = false;
                        ItemActive = true;
                    }
                    else if (mouseInputEvent.EventType == InputEventType.MouseButtonUp)
                    {
                        _leftReleased = true;
                    }
                    InputManager.MarkInputAsHandled(@event);
                }
            }
        }

        public override void Update(double delta)
        {
            if (!ItemActive)
                return;
            Item = _inventory.GetSelected();
            if (Item == null) {
                ItemActive = false;
                return;
            }

            FlipSprite = Main.EntityManager.GetPlayer().FlipSprite;
            Position = Main.EntityManager.GetPlayer().Position + new Vector2(0, 20) + (FlipSprite ? new Vector2(12, 0) : new Vector2(8, 0));
            Origin = FlipSprite ? new Vector2(Item.Image.Width + 8, Item.Image.Height) : new Vector2(-8, Item.Image.Height);
            if (_canUseItem && _inventory.UseSelected())
            {
                _canUseItem = false;
            }
            switch (Item.UseStyle)
            {
                case UseStyle.Point:
                    if (_holdTime == 0.0f)
                    {
                        Vector2 playerPosition = Main.EntityManager.GetPlayer().Position;
                        Point mousePosition = InputManager.GetMouseWorldPosition();
                        Rotation = (float)Math.Atan((playerPosition.Y - mousePosition.Y) / Math.Abs(playerPosition.X - mousePosition.X));
                        Rotation = FlipSprite ? -Rotation : Rotation;
                    }
                    break;
                case UseStyle.Swing:
                    Rotation = (float)(_holdTime / Item.UseSpeed * MathHelper.PiOver2);
                    Rotation = FlipSprite ? -Rotation : Rotation;
                    break;
                default:
                    Rotation = 0.0f;
                    break;
            }
            _holdTime += delta;
            if (_holdTime >= Item.UseSpeed)
            {
                _holdTime = 0.0f;
                _canUseItem = true;
                if (!Item.AutoUse || _leftReleased || _inventory.GetSelected() == null)
                {
                    ItemActive = false;
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!ItemActive)
                return;
            Point centerTilePosition = ((Position + Size / 2) / Globals.TILESIZE).ToPoint();
            spriteBatch.Draw(Item.Image,
                        Position,
                        null,
                        Main.LightEngine.GetLight(centerTilePosition.X, centerTilePosition.Y),
                        Rotation,
                        Origin,
                        Item.Scale,
                        FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                        0f
                    );
        }

        public override Rectangle GetBounds()
        {
            if (ItemActive && Item is WeaponItem weaponItem && weaponItem.SpriteDoesDamage)
            {
                Vector2[] corners = {
                //top left
                Position + RotateVector2(new Vector2(FlipSprite ? Item.Image.Width : 0, 0) - Origin),
                //top right
                Position + RotateVector2(new Vector2(FlipSprite ? 0 : Item.Image.Width, 0) - Origin),
                //bottom left
                Position + RotateVector2(new Vector2(FlipSprite ? Item.Image.Width : 0, Item.Image.Height) - Origin),
                //bottom right
                Position + RotateVector2(new Vector2(FlipSprite ? 0 : Item.Image.Width, Item.Image.Height) - Origin)
                };
                float minX = corners.Min(c => c.X);
                float maxX = corners.Max(c => c.X);
                float minY = corners.Min(c => c.Y);
                float maxY = corners.Max(c => c.Y);
                return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
            }
            
            return default;
        }
        private Vector2 RotateVector2(Vector2 vector)
        {
            return new Vector2(
                (float)(vector.X * Math.Cos(Rotation) - vector.Y * Math.Sin(Rotation)), 
                (float)(vector.X * Math.Sin(Rotation) + vector.Y * Math.Cos(Rotation)));
        }
    }
}
