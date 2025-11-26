using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Vestige.Game.Entities.NPCs;
using Vestige.Game.Entities.Projectiles;
using Vestige.Game.Items;
using Vestige.Game.Tiles;
using Vestige.Game.Tiles.TileData;

namespace Vestige.Game.Entities
{
    /// <summary>
    /// Handles collision detection between entities
    /// </summary>
    /// 
    //FIX: if entity penetration distance on a tile is greater than a threshold, consider it stuck and don't move it, will prevent jitter when itemdrop is stuck in a block
    public class EntityManager
    {
        //TODO: possibly split screen, leaving this here
        private Player[] _players = new Player[12];
        /// <summary>
        /// The Entity the mouse is currently on. Ensure this is accessed post collision updates.
        /// </summary>
        public Entity MouseEntity;
        private List<IRespawnable> _respawnList = new List<IRespawnable>();
        private ItemDrop[] _itemDrops = new ItemDrop[500];
        private NPC[] _npcs = new NPC[500];
        private Projectile[] _projectiles = new Projectile[1000];
        private EntitySpawner _entitySpawner = new EntitySpawner();

        public void Update(double delta)
        {
            _entitySpawner.Update(delta);
            MouseEntity = null;
            for (int i = _respawnList.Count - 1; i >= 0; i--)
            {
                IRespawnable respawn = _respawnList[i];
                respawn.RespawnTime -= (float)delta;
                if (respawn.RespawnTime <= 0.0f)
                {
                    respawn.Respawn();
                    _respawnList.Remove(respawn);
                }
            }

            //Update Entities
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] != null)
                {
                    if (!_players[i].Active)
                    {
                        _players[i] = null;
                        continue;
                    }
                    UpdateEntity(_players[i], delta);
                    UpdateEntity(_players[i].ItemCollider, delta);
                }
            }
            for (int i = 0; i < _itemDrops.Length; i++)
            {
                if (_itemDrops[i] == null) continue;
                if (!_itemDrops[i].Active)
                {
                    _itemDrops[i] = null;
                    continue;
                }
                UpdateEntity(_itemDrops[i], delta);
            }
            for (int i = 0; i < _npcs.Length; i++)
            {
                if (_npcs[i] == null) continue;
                if (!_npcs[i].Active)
                {
                    _npcs[i] = null;
                    continue;
                }
                UpdateEntity(_npcs[i], delta);
            }
            for (int i = 0; i < _projectiles.Length; i++)
            {
                if (_projectiles[i] == null) continue;
                if (!_projectiles[i].Active)
                {
                    _projectiles[i] = null;
                    continue;
                }
                UpdateEntity(_projectiles[i], delta);
            }
            //Collisions between Player and ItemDrops
            for (int i = 0; i < _itemDrops.Length; i++)
            {
                if (_itemDrops[i] == null) continue;
                for (int j = 0; j < _players.Length; j++)
                {
                    if (_players[j] == null) continue;
                    if (_itemDrops[i].GetBounds().Intersects(_players[j].GetBounds()))
                        _players[j].OnCollision(_itemDrops[i]);
                }
            }
            //Collisions between npcs and player/friendly npcs
            for (int i = 0; i < _npcs.Length; i++)
            {
                if (_npcs[i] == null) continue;
                //TODO: collide with other npcs
                if (_npcs[i].Friendly) continue;
                CollisionRectangle npcBounds = _npcs[i].GetBounds();
                for (int j = 0; j < _players.Length; j++)
                {
                    if (_players[j] == null) continue;
                    if (npcBounds.Intersects(_players[j].GetBounds()))
                    {
                        _players[j].OnCollision(_npcs[i]);
                    }
                    if (npcBounds.Intersects(_players[j].ItemCollider.GetBounds()))
                        _npcs[i].OnCollision(_players[j].ItemCollider);
                }
            }
            for (int i = 0; i < _projectiles.Length; i++)
            {
                if (_projectiles[i] == null) continue;
                if (_projectiles[i].Friendly)
                {
                    for (int j = 0; j < _npcs.Length; j++)
                    {
                        if (_npcs[j] == null) continue;
                        if (_npcs[j].Friendly) continue;
                        if (_npcs[j].GetBounds().Intersects(_projectiles[i].GetBounds()))
                        {
                            _npcs[j].OnCollision(_projectiles[i]);
                            _projectiles[i].OnCollision(_npcs[j]);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < _players.Length; j++)
                    {
                        if (_players[j] == null) continue;
                        if (_projectiles[i].GetBounds().Intersects(_players[j].GetBounds()))
                        {
                            _players[j].OnCollision(_projectiles[i]);
                            _projectiles[i].OnCollision(_players[j]);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Moves the entity, performs tile collisions, and updates the mouse entity
        /// </summary>
        /// <param name="entity"></param>
        public void UpdateEntity(Entity entity, double delta)
        {
            if (entity.GetBounds().Contains(Main.GetMouseWorldPosition()))
                MouseEntity = entity;
            //Update Entities
            entity.Update(delta);
            Vector2 minPos = Vector2.Zero;
            Vector2 maxPos = new((Main.World.WorldSize.X * Vestige.TILESIZE) - entity.Size.X, (Main.World.WorldSize.Y * Vestige.TILESIZE) - entity.Size.Y);
            //update enemies positions that don't collide with tiles
            if (!entity.CollidesWithTiles)
            {
                entity.Position += entity.Velocity * (float)delta;
                entity.Position = Vector2.Clamp(entity.Position, minPos, maxPos);
                return;
            }
            
            int tileWidth = entity.GetBounds().Width / Vestige.TILESIZE;
            int tileHeight = entity.GetBounds().Height / Vestige.TILESIZE;

            //An entity can move at max 16 pixels at a time, to ensure a tile collision isn't skipped if the entity was moving to fast
            //horizontal tile collisions
            float distanceX = entity.Velocity.X * (float)delta;
            int numTiles = (int)Math.Floor(Math.Abs(distanceX) / (Vestige.TILESIZE - 1));
            int horizontalCollisionDirection = 0;
            for (int i = 0; i < numTiles + 1; i++)
            {
                float distance = i == numTiles ? distanceX % (Vestige.TILESIZE - 1) : (Vestige.TILESIZE - 1);
                horizontalCollisionDirection = HorizontalCollisionPass(entity, distance, minPos, maxPos);
                if (horizontalCollisionDirection != 0)
                {
                    break;
                }
            }

            //vertical tile collisions
            float distanceY = entity.Velocity.Y * (float)delta;
            numTiles = (int)Math.Floor(Math.Abs(distanceY) / Vestige.TILESIZE);
            for (int i = 0; i < numTiles + 1; i++)
            {
                float distance = i == numTiles ? distanceY % Vestige.TILESIZE : Vestige.TILESIZE;
                if (VerticalCollisionPass(entity, distance, minPos, maxPos))
                {
                    break;
                }
            }

            //Determine if an entity who has horizontally collided with a tile can hop up
            if (horizontalCollisionDirection != 0)
            {
                if (entity.HopTiles && Math.Sign(entity.Velocity.X) == horizontalCollisionDirection && CanEntityHop(entity, (entity.GetBounds().Position / Vestige.TILESIZE).ToPoint(), tileWidth, tileHeight, horizontalCollisionDirection))
                {
                    entity.Position.Y -= Vestige.TILESIZE;
                    entity.Position.X += 2 * Math.Sign(entity.Velocity.X);
                }
                else
                {
                    entity.Velocity.X = 0.0f;
                }
            }

            //Resolve liquid collisions
            LiquidCollisionPass(entity);
        }

        //TODO: find a better solution than this, this is terrible
        //EDIT: It's good enough
        public bool TileOccupied(int x, int y)
        {
            CollisionRectangle tile = new CollisionRectangle(x * Vestige.TILESIZE, y * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
            for (int i = 0; i < _npcs.Length; i++)
            {
                if (_npcs[i] == null || !_npcs[i].CollidesWithTiles) continue;
                if (_npcs[i].GetBounds().Intersects(tile))
                    return true;
            }
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] != null && _players[i].GetBounds().Intersects(tile))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name=""></param>
        /// <returns>The direction of the horizontal collision, 0 if there is none</returns>
        private int HorizontalCollisionPass(Entity entity, float distance, Vector2 minPos, Vector2 maxPos)
        {
            entity.Position.X += distance;
            entity.Position.X = float.Clamp(entity.Position.X, minPos.X, maxPos.X);

            CollisionRectangle entityBounds = entity.GetBounds();
            int startX = (int)entityBounds.Left / Vestige.TILESIZE;
            int endX = (int)Math.Ceiling(entityBounds.Right / Vestige.TILESIZE);
            int startY = (int)entityBounds.Top / Vestige.TILESIZE;
            int endY = (int)Math.Ceiling(entityBounds.Bottom / Vestige.TILESIZE);
            int horizontalCollisionDirection = 0;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    CollisionRectangle tileCollider = new CollisionRectangle(x * Vestige.TILESIZE, y * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
                    //IMPORTANT: entity bounds will not intersect a tile or other collision if the position update is less than a pixels width, since bounds are calculated using integers. Players Velocity will get up to 30 before the player actually moves enough to detect a collision.
                    if (entityBounds.Intersects(tileCollider))
                    {
                        if (TileDatabase.GetTileData(Main.World.GetTileID(x, y)) is ICollideableTile collideableTile)
                            collideableTile.OnCollision(Main.World, x, y, entity);
                        if (TileDatabase.TileHasProperties(Main.World.GetTileID(x, y), TileProperty.Platform))
                        {
                            //TODO: stairs eventually
                            continue;
                        }
                        else if (!TileDatabase.TileHasProperties(Main.World.GetTileID(x, y), TileProperty.Solid))
                        {
                            continue;
                        }
                        float penetrationDistance = GetPenetrationX(entity.GetBounds(), tileCollider);
                        if (penetrationDistance < 0)
                        {
                            entity.Position.X = tileCollider.Left - entity.Origin.X - (entityBounds.Width / 2.0f);
                            horizontalCollisionDirection = 1;
                        }
                        else if (penetrationDistance > 0)
                        {
                            entity.Position.X = tileCollider.Right - entity.Origin.X + (entityBounds.Width / 2.0f);
                            horizontalCollisionDirection = -1;
                        }
                    }
                }
            }
            return horizontalCollisionDirection;
        }

        private bool VerticalCollisionPass(Entity entity, float distance, Vector2 minPos, Vector2 maxPos)
        {
            bool floorCollision = false;
            bool ceilingCollision = false;
            bool collisionDetected = false;
            Vector2 initialPosition = entity.Position;
            entity.Position.Y += distance;

            CollisionRectangle entityBounds = entity.GetBounds();
            int startX = (int)entityBounds.Left / Vestige.TILESIZE;
            int endX = (int)Math.Ceiling(entityBounds.Right / Vestige.TILESIZE);
            int startY = (int)entityBounds.Top / Vestige.TILESIZE;
            int endY = (int)Math.Ceiling(entityBounds.Bottom / Vestige.TILESIZE);
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    CollisionRectangle tileCollider = new CollisionRectangle(x * Vestige.TILESIZE, y * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
                    if (entityBounds.Intersects(tileCollider))
                    {
                        if (TileDatabase.GetTileData(Main.World.GetTileID(x, y)) is ICollideableTile collideableTile)
                            collideableTile.OnCollision(Main.World, x, y, entity);
                        if (TileDatabase.TileHasProperties(Main.World.GetTileID(x, y), TileProperty.Platform))
                        {
                            if (!entity.CollidesWithPlatforms)
                            {
                                continue;
                            }
                            bool validPlatformCollision = initialPosition.Y + entity.Size.Y <= tileCollider.Y && entity.Position.Y + entity.Size.Y > tileCollider.Y;
                            if (!validPlatformCollision)
                                continue;
                        }
                        else if (!TileDatabase.TileHasProperties(Main.World.GetTileID(x, y), TileProperty.Solid))
                        {
                            continue;
                        }
                        collisionDetected = true;
                        if (tileCollider.Y > entity.Position.Y)
                        {
                            floorCollision = true;
                        }
                        else if (tileCollider.Y < entity.Position.Y && entity.Velocity.Y < 0.0f)
                        {
                            ceilingCollision = true;
                        }
                        float penetrationDistance = GetPenetrationY(entity.GetBounds(), tileCollider);
                        entity.Position.Y += penetrationDistance;
                        if (penetrationDistance != 0)
                        {
                            entity.Velocity.Y = 0;
                        }
                    }
                }
            }
            entity.CollidesWithPlatforms = true;
            entity.IsOnFloor = floorCollision;
            entity.IsOnCeiling = ceilingCollision;
            return collisionDetected;
        }
        private void LiquidCollisionPass(Entity entity)
        {
            //TODO: to fix water level collisions (NPC should not detect water collision if water is in the tile it's colliding with but the water line is lower than the bottom of the entity)
            //Create a collision rectangle of the water (Size of the water sprite, and position of the water sprite) skip it if the entity and water are not colliding
            Vector2 initialPosition = entity.Position;

            CollisionRectangle entityBounds = entity.GetBounds();
            int startX = (int)entityBounds.Left / Vestige.TILESIZE;
            int endX = (int)Math.Ceiling(entityBounds.Right / Vestige.TILESIZE);
            int startY = (int)entityBounds.Top / Vestige.TILESIZE;
            int endY = (int)Math.Ceiling(entityBounds.Bottom / Vestige.TILESIZE);
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    CollisionRectangle tileCollider = new CollisionRectangle(x * Vestige.TILESIZE, y * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
                    if (entityBounds.Intersects(tileCollider))
                    {
                        if (Main.World.GetLiquid(x, y) != 0)
                        {
                            //No liquid IDs implemented yet
                            entity.OnLiquidCollision(x, y, 0);
                            break;
                        }
                    }
                }
            }
        }
        float GetPenetrationX(CollisionRectangle a, CollisionRectangle b)
        {
            float distanceX = a.Center.X - b.Center.X;
            float penetrationX = (a.Width / 2f) + (b.Width / 2f) - Math.Abs(distanceX);
            return penetrationX <= 0 ? 0.0f : distanceX < 0 ? -penetrationX : penetrationX;
        }
        float GetPenetrationY(CollisionRectangle a, CollisionRectangle b)
        {
            float distanceY = a.Center.Y - b.Center.Y;
            float penetrationY = (a.Height / 2f) + (b.Height / 2f) - Math.Abs(distanceY);
            return penetrationY <= 0 ? 0.0f : distanceY < 0 ? -penetrationY : penetrationY;
        }
        /// <summary>
        /// Adds a player to the game.
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(Player player)
        {
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] == null)
                {
                    _players[i] = player;
                    break;
                }
            }
        }
        /// <summary>
        /// Gets the next available player target
        /// </summary>
        /// <returns>A Player object if one is available, otherwise null</returns>
        public Player GetPlayerTarget()
        {
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] != null)
                {
                    return _players[i];
                }
            }
            return null;
        }
        public Player GetPlayerByName(string name)
        {
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] != null && _players[i].Name == name)
                {
                    return _players[i];
                }
            }
            return null;
        }
        public void CreateEnemy(int enemyID, Vector2 Position)
        {
            NPC enemy = NPC.InstantiateNPCByID(enemyID);
            enemy.Position = Position;
            for (int i = 0; i < _npcs.Length; i++)
            {
                if (_npcs[i] == null)
                {
                    _npcs[i] = enemy;
                    break;
                }
            }
        }
        public void CreateItemDrop(Item item, Vector2 position, Vector2 velocity = default, bool canBePickedUp = true)
        {
            ItemDrop itemDrop = new ItemDrop(item, default, canBePickedUp);
            itemDrop.Position = position - itemDrop.Origin;
            itemDrop.Velocity = velocity;
            for (int i = 0; i < _itemDrops.Length; i++)
            {
                if (_itemDrops[i] == null)
                {
                    _itemDrops[i] = itemDrop;
                    break;
                }
            }
        }
        /// <summary>
        /// Creates a new projectile in the world with the specified ID
        /// </summary>
        /// <param name="projectileID">The projectiles ID</param>
        /// <param name="position">The position the projectile spawns at</param>
        /// <param name="speed">The speed at which the projectile initially starts at</param>
        /// <param name="direction">The normalized direction of the projectile</param>
        public void CreateProjectile(int projectileID, Vector2 position, float speed, Vector2 direction)
        {
            Projectile projectile = Projectile.InstantiateProjectileByID(projectileID);
            projectile.Position = position;
            projectile.Velocity = direction * speed;
            for (int i = 0; i < _projectiles.Length; i++)
            {
                if (_projectiles[i] == null)
                {
                    _projectiles[i] = projectile;
                    break;
                }
            }
        }
        private bool CanEntityHop(Entity entity, Point tilePoint, int tileWidth, int tileHeight, int direction)
        {
            if (!entity.IsOnFloor || entity.Velocity.X == 0)
                return false;
            for (int x = 0; x <= tileWidth; x++)
            {
                if (TileDatabase.TileHasProperties(Main.World.GetTileID(tilePoint.X + x, tilePoint.Y - 1), TileProperty.Solid))
                    return false;
            }
            int tilesInFrontOffset = direction == -1 ? -1 : tileWidth + 1;
            for (int y = -1; y < tileHeight; y++)
            {
                if (TileDatabase.TileHasProperties(Main.World.GetTileID(tilePoint.X + tilesInFrontOffset, tilePoint.Y + y), TileProperty.Solid))
                    return false;
            }
            return true;
        }
        internal void QueueRespawn(IRespawnable respawnable)
        {
            _respawnList.Add(respawnable);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _npcs.Length; i++)
            {
                _npcs[i]?.Draw(spriteBatch);
            }
            for (int i = 0; i < _players.Length; i++)
            {
                _players[i]?.Draw(spriteBatch);
            }
            for (int i = 0; i < _itemDrops.Length; i++)
            {
                _itemDrops[i]?.Draw(spriteBatch);
            }
            for (int i = 0; i < _projectiles.Length; i++)
            {
                _projectiles[i]?.Draw(spriteBatch);
            }
        }
        public Player[] GetPlayers()
        {
            return _players;
        }
    }
}
