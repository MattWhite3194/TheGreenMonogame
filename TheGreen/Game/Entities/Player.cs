using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Timers;
using TheGreen.Game.Entities.NPCs;
using TheGreen.Game.Input;
using TheGreen.Game.Inventory;
using TheGreen.Game.Tiles;
using TheGreen.Game.UIComponents;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Entities
{
    public class Player : Entity, IInputHandler
    {
        private float _acceleration = 500;
        public Vector2 Direction = Vector2.Zero;
        private int _maxSpeed = 200;
        private int _maxFallSpeed = 900;
        private int _jumpVelocity = -600;
        public InventoryManager Inventory;
        private int _health;
        private float _fallDistance = 0;
        private bool _queueJump = false;
        public ItemCollider ItemCollider;
        private Timer _invincibilityTimer;
        private Timer _respawnTimer;
        private bool invincible = false;
        private bool _dead = false;
        private HashSet<InputButton> _activeInputs = new HashSet<InputButton>();
        public bool Dead { get { return _dead; } }
        public Player(Texture2D image, InventoryManager inventory, int health) : base(image, default, size: new Vector2(20, 42), animationFrames: new List<(int, int)> { (0, 0), (1, 8), (9, 9), (10, 10) })
        {
            this.Inventory = inventory;
            CollidesWithTiles = true;
            _health = health;
            this.Layer = CollisionLayer.Player;
            this.CollidesWith = CollisionLayer.Enemy | CollisionLayer.ItemDrop | CollisionLayer.HostileProjectile;
        }

        /// <summary>
        /// Called once when the player is initially added to the world
        /// </summary>
        public void InitializeGameUpdates()
        {
            ItemCollider = new ItemCollider(Inventory);
            _invincibilityTimer = new Timer(1000);
            _invincibilityTimer.Elapsed += OnInvincibleTimeout;
            _respawnTimer = new Timer(5000);
            _respawnTimer.Elapsed += OnRespawnTimeout;
            Position = WorldGen.World.SpawnTile.ToVector2() * Globals.TILESIZE - new Vector2(0, Size.Y - 1);
            Main.EntityManager.AddEntity(this);
            Main.EntityManager.AddEntity(ItemCollider);
            InputManager.RegisterHandler(this);
            UIManager.RegisterContainer(Inventory);
        }

        public void HandleInput(InputEvent @event)
        {
            if (!Active) return;
            //TODO:
            //Handle input for equipment
            Inventory.HandleInput(@event);
            if (InputManager.IsEventHandled(@event))
                return;
            //if input is handled return

            if (@event.InputButton == InputButton.Left)
            {
                if (@event.EventType == InputEventType.KeyDown)
                    _activeInputs.Add(InputButton.Left);
                else
                    _activeInputs.Remove(InputButton.Left);
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.Right)
            {
                if (@event.EventType == InputEventType.KeyDown)
                    _activeInputs.Add(InputButton.Right);
                else
                    _activeInputs.Remove(InputButton.Right);
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.Jump)
            {
                _queueJump = @event.EventType == InputEventType.KeyDown;
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.RightMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(Globals.TILESIZE);
                if (TileDatabase.GetTileData(WorldGen.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y)) is IInteractableTile interactableTile)
                {
                    interactableTile.OnRightClick(mouseTilePosition.X, mouseTilePosition.Y);
                }
            }
            else
            {
                ItemCollider.HandleInput(@event);
            }
        }

        public override void Update(double delta)
        {
            base.Update(delta);
            Vector2 newVelocity = Velocity;
            Direction.X = 0;
            if (_activeInputs.Contains(InputButton.Left)) Direction.X -= 1;
            if (_activeInputs.Contains(InputButton.Right)) Direction.X += 1;
            //Slow down player if they stopped moving
            if (Direction.X == 0.0f)
            {
                if (MathF.Abs(newVelocity.X) < 20)
                {
                    newVelocity.X = 0;
                    Animation.SetCurrentAnimation(0);
                }
                else
                    newVelocity.X -= MathF.Sign(newVelocity.X) * (_acceleration * 2.0f) * (float)delta;             
            }
            else
            {
                if ((int)Direction.X != MathF.Sign(Velocity.X))
                    newVelocity.X += Direction.X * (_acceleration * 2.0f) * (float)delta;
                else
                    newVelocity.X += Direction.X * _acceleration * (float)delta;
                if (MathF.Abs(newVelocity.X) > _maxSpeed)
                    newVelocity.X = Math.Sign(newVelocity.X) * _maxSpeed;
                Animation.SetCurrentAnimation(1);
                Animation.SetAnimationSpeed(Math.Abs(Velocity.X / 10));
            }

            //add gravity
            newVelocity.Y += Globals.GRAVITY * (float)delta;
            if (newVelocity.Y > _maxFallSpeed)
                newVelocity.Y = _maxFallSpeed;

            //calculate fall damage and jump
            if (IsOnFloor)
            {
                Direction.Y = 0;
                //if the player falls for 10 tiles or more, take damage
                if (_fallDistance / Globals.TILESIZE > 10)
                {
                    ApplyDamage((((int)(_fallDistance / Globals.TILESIZE) - 10) * 2));
                    
                }
                _fallDistance = 0;
                if (_queueJump)
                {
                    newVelocity.Y = _jumpVelocity;
                    _queueJump = false;
                }
            }
            else
            {
                if (Velocity.Y > 0)
                {
                    Direction.Y = 1;
                    Animation.SetCurrentAnimation(3);
                    _fallDistance += Velocity.Y * (float)delta;
                }
                else
                {
                    Direction.Y = -1;
                    Animation.SetCurrentAnimation(2);
                }
            }

            if (Direction.X < 0)
                FlipSprite = true;
            else if (Direction.X > 0)
                FlipSprite = false;
            
            Velocity = newVelocity;
        }

        public override void OnCollision(Entity entity)
        {
            switch (entity.Layer)
            {
                case CollisionLayer.ItemDrop:
                    ItemDrop itemDrop = (ItemDrop)entity;
                    if (Inventory.AddItemToPlayerInventory(itemDrop.GetItem()) == null)
                    {
                        Main.EntityManager.RemoveEntity(itemDrop);
                    }
                    break;
                case CollisionLayer.Enemy:
                    if (invincible) return;
                    NPC enemy = (NPC)entity;
                    ApplyDamage(enemy.Damage);
                    ApplyKnockback(enemy.Position + enemy.Origin);
                    break;
            }
        }
        public void ApplyDamage(int damage)
        {
            this._health -= damage;
            if (this._health <= 0)
            {
                Active = false;
                ItemCollider.Active = false;
                _dead = true;
                _respawnTimer.Start();
                return;
            }
            invincible = true;
            if (Active)
            {
                _invincibilityTimer.Start();
            }
        }
        private void ApplyKnockback(Vector2 knockbackSource)
        {
            this.Velocity.Y = Math.Min(-300, Velocity.Y);
            this.Velocity.X = Math.Sign((Position.X + Origin.X) - knockbackSource.X) * _maxSpeed;
        }
        private void OnInvincibleTimeout(object sender, ElapsedEventArgs e)
        {
            invincible = false;
            _invincibilityTimer.Stop();
        }
        private void OnRespawnTimeout(object sender, ElapsedEventArgs e)
        {
            Active = true;
            ItemCollider.Active = true;
            _dead = false;
            Position = WorldGen.World.SpawnTile.ToVector2() * Globals.TILESIZE - new Vector2(0, Size.Y - 1);
            Velocity = Vector2.Zero;
            _activeInputs.Clear();
            ItemCollider.ItemActive = false;
            _queueJump = false;
            Main.EntityManager.AddEntity(this);
            Main.EntityManager.AddEntity(ItemCollider);
            _health = 100;
            _respawnTimer.Stop();
        }
    }
}
