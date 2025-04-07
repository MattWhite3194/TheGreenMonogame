using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace TheGreen.Game.Entities.NPCs.Behaviors
{
    /// <summary>
    /// Defined movement pattern for a mutant cricket
    /// </summary>
    public class MutantCricketBehavior : INPCBehavior
    {
        private int _directionX = 0;
        private double _elapsedTime = 0.0f;
        private double _nextJumpTime = 2.0;
        private float _maxSpeed = 100;
        private float _acceleration = 1000;
        private Player _player;
        public void AI(double delta, NPC enemy)
        {
            _player = Main.EntityManager.GetPlayer();
            Vector2 newVelocity = enemy.Velocity;
            newVelocity.Y += Globals.GRAVITY * (float)delta;
            _elapsedTime += delta;
            _directionX = -MathF.Sign((enemy.Position.X + enemy.Origin.X) - (_player.Position.X + _player.Origin.X));

            if (enemy.IsOnFloor)
            {
                enemy.Animation.SetCurrentAnimation(0);
                enemy.Animation.SetAnimationSpeed(Math.Abs(newVelocity.X) / 10);
                if (_directionX != MathF.Sign(enemy.Velocity.X))
                    newVelocity.X += _directionX * (_acceleration * 2.0f) * (float)delta;
                else
                    newVelocity.X += _acceleration * _directionX * (float)delta;
                if (MathF.Abs(enemy.Velocity.X) > _maxSpeed)
                    newVelocity.X = _maxSpeed * Math.Sign(newVelocity.X);
                if (_elapsedTime >= _nextJumpTime)
                {
                    _nextJumpTime = Main.Random.NextDouble() * 5.0 + 2.0;
                    _elapsedTime = 0.0;
                    newVelocity.Y = _maxSpeed * Main.Random.Next(-4, -3);
                    newVelocity.X = _maxSpeed * _directionX * Main.Random.Next(2, 5);
                    enemy.Animation.SetCurrentAnimation(1);
                }
            }

            if (newVelocity.X > 0)
                enemy.FlipSprite = true;
            if (newVelocity.X < 0)
                enemy.FlipSprite = false;

            enemy.Velocity = newVelocity;
        }

        public INPCBehavior Clone()
        {
            return new MutantCricketBehavior();
        }
    }
}
