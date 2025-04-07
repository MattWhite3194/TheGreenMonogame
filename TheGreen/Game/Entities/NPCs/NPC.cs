using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using TheGreen.Game.Entities.NPCs.Behaviors;
using TheGreen.Game.Items;

namespace TheGreen.Game.Entities.NPCs
{
    public class NPC : Entity
    {
        public int ID;
        public string Name;
        private int _health;
        public readonly int Damage;
        private readonly bool _friendly;
        private INPCBehavior _behavior;
        private Timer _invincibilityTimer;
        private bool _invincible = false;
        private List<(int, int)> _animationFrames;
        public NPC(int id, 
            string name, 
            Texture2D image, 
            Vector2 size, 
            int health, 
            int damage, 
            bool collidesWithTiles, 
            INPCBehavior behavior,
            bool drawBehindTiles = false,
            bool friendly = false,
            List<(int, int)> animationFrames = null, 
            CollisionLayer layer = default, 
            CollisionLayer collidedWith = default) 
            : base(image, default, size: size, animationFrames: animationFrames)
        {
            ID = id;
            Name = name;
            Damage = damage;
            this._health = health;
            CollidesWithTiles = collidesWithTiles;
            _friendly = friendly;
            _behavior = behavior;
            _animationFrames = animationFrames;
            _invincibilityTimer = new Timer(500);
            _invincibilityTimer.Elapsed += OnInvincibleTimeout;
            this.Layer = layer;
            this.CollidesWith = collidedWith;
            if (Layer == default)
            {
                this.Layer = friendly ? CollisionLayer.Player : CollisionLayer.Enemy;
            }
            if (CollidesWith == default)
            {
                this.CollidesWith = friendly ? CollisionLayer.Enemy | CollisionLayer.HostileProjectile : CollisionLayer.Player | CollisionLayer.ItemCollider | CollisionLayer.FriendlyProjectile;
            }
        }
        public override void Update(double delta)
        {
            _behavior?.AI(delta, this);
            base.Update(delta);
        }
        public override void OnCollision(Entity entity)
        {
            switch (entity.Layer)
            {
                case CollisionLayer.ItemCollider:
                    if (_invincible) return;
                    ItemCollider itemCollider = (ItemCollider)entity;
                    ApplyDamage(((WeaponItem)itemCollider.Item).Damage);
                    ApplyKnockback(((WeaponItem)itemCollider.Item).Knockback, Main.EntityManager.GetPlayer().Position + Main.EntityManager.GetPlayer().Origin);
                    break;
            }
        }
        private void ApplyDamage(int damage)
        {
            this._health -= damage;
            _invincible = true;
            if (this._health <= 0)
                Active = false;
            if (Active)
            {
                _invincibilityTimer.Start();
            } 
        }
        private void ApplyKnockback(int knockback, Vector2 knockbackSource)
        {
            this.Velocity.Y = -(knockback * 100);
            this.Velocity.X = Math.Sign((Position.X + Origin.X) - knockbackSource.X) * (knockback * 100);
        }
        private void OnInvincibleTimeout(object sender, ElapsedEventArgs e)
        {
            _invincible = false;
            _invincibilityTimer.Stop();
        }
        public static NPC CloneNPC(NPC npc)
        {
            return new NPC(npc.ID, npc.Name, npc.Image, npc.Size, npc._health, npc.Damage, npc.CollidesWithTiles, npc._behavior.Clone(), npc.DrawBehindTiles, npc._friendly, npc._animationFrames, npc.Layer, npc.CollidesWith);
        }
    }
}
