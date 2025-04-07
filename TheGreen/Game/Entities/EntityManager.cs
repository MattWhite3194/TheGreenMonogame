using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TheGreen.Game.Entities.NPCs;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Entities
{
    /// <summary>
    /// Handles collision detection between entities
    /// </summary>
    public class EntityManager
    {
        private Player _player;

        private List<Entity> _entities = new List<Entity>();
        private List<Rectangle> _intersections;

        public void Update(double delta)
        {
            //Handle tile collisions
            for (int i = 0; i < _entities.Count; i++)
            {
                Entity entity = _entities[i];
                if (!entity.Active)
                {
                    _entities.Remove(entity);
                    continue;
                }
                //Update Entities
                entity.Update(delta);
                //update enemies positions that don't collide with tiles
                if (!entity.CollidesWithTiles)
                {
                    entity.Position += entity.Velocity * (float)delta;
                    continue;
                }

                int distanceFactor = (int)Math.Min(entity.Size.X, Globals.TILESIZE);

                Vector2 minPos = Vector2.Zero;
                Vector2 maxPos = new(WorldGen.World.WorldSize.X * Globals.TILESIZE - entity.Size.X, WorldGen.World.WorldSize.Y * Globals.TILESIZE - entity.Size.Y);
                int tileWidth = (int)entity.Size.X / Globals.TILESIZE;
                int tileHeight = (int)entity.Size.Y / Globals.TILESIZE;
                float distanceX = entity.Velocity.X * (float)delta;
                int horizontalCollisionDirection = 0;
                List<float> horizontalDistances = new List<float>();
                for (int _ = 0; _ < Math.Floor(distanceX / (distanceFactor - 1)); _++)
                {
                    horizontalDistances.Add((distanceFactor - 1));
                }
                horizontalDistances.Add(distanceX % (distanceFactor - 1));
                foreach (float distance in horizontalDistances)
                {
                    entity.Position.X += distance;
                    entity.Position.X = float.Clamp(entity.Position.X, minPos.X, maxPos.X);
                    _intersections = GetHorizontalCollisions(entity);
                    List<Rectangle> horizontalCollisions = new();


                    foreach (var rect in _intersections)
                    {
                        //temporary check for smaller map size, bigger map and bounds will resolve this
                        if (rect.X >= 0 && rect.X < WorldGen.World.WorldSize.X && rect.Y >= 0 && rect.Y < WorldGen.World.WorldSize.Y)
                        {
                            if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(rect.X, rect.Y), TileProperty.Solid))
                            {
                                Rectangle collision = new Rectangle(
                                    rect.X * Globals.TILESIZE,
                                    rect.Y * Globals.TILESIZE,
                                    Globals.TILESIZE,
                                    Globals.TILESIZE
                                );
                                if (rect.X * Globals.TILESIZE < entity.Position.X && entity.Velocity.X < 0.0f)
                                {
                                    horizontalCollisionDirection = -1;
                                    entity.Position.X = collision.Right;
                                }
                                else if (rect.X * Globals.TILESIZE > entity.Position.X && entity.Velocity.X > 0.0f)
                                {
                                    horizontalCollisionDirection = 1;
                                    entity.Position.X = collision.Left - entity.Size.X;
                                }
                                break;
                            }
                        }
                    }
                    if (horizontalCollisionDirection != 0)
                        break;
                }

                distanceFactor = (int)Math.Min(entity.Size.Y, Globals.TILESIZE);
                float distanceY = entity.Velocity.Y * (float)delta;
                List<float> verticalDistances = new List<float>();
                for (int _ = 0; _ < Math.Floor(distanceY / (distanceFactor - 1)); _++)
                {
                    verticalDistances.Add(distanceFactor - 1);
                }
                verticalDistances.Add(distanceY % (distanceFactor - 1));
                foreach (float distance in verticalDistances)
                {
                    entity.Position.Y += distance;
                    entity.Position.Y = float.Clamp(entity.Position.Y, minPos.Y, maxPos.Y);
                    _intersections = GetVerticalCollisions(entity);
                    bool floorCollision = false;
                    bool ceilingCollision = false;


                    foreach (var rect in _intersections)
                    {
                        //temporary check for smaller map size, bigger map and bounds will resolve this
                        if (rect.X >= 0 && rect.X < WorldGen.World.WorldSize.X && rect.Y >= 0 && rect.Y < WorldGen.World.WorldSize.Y)
                        {
                            if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(rect.X, rect.Y), TileProperty.Solid))
                            {
                                Rectangle collision = new Rectangle(
                                    rect.X * Globals.TILESIZE,
                                    rect.Y * Globals.TILESIZE,
                                    Globals.TILESIZE,
                                    Globals.TILESIZE
                                );

                                if (rect.Y * Globals.TILESIZE > entity.Position.Y && entity.Velocity.Y > 0.0f)
                                {
                                    entity.Position.Y = collision.Top - entity.Size.Y;
                                    floorCollision = true;
                                    entity.Velocity.Y = 0.0f;
                                }
                                else if (rect.Y * Globals.TILESIZE < entity.Position.Y && entity.Velocity.Y < 0.0f)
                                {
                                    entity.Position.Y = collision.Bottom;
                                    ceilingCollision = true;
                                    entity.Velocity.Y = 0.0f;
                                }
                                break;
                            }
                        }
                    }
                    entity.IsOnCeiling = ceilingCollision;
                    entity.IsOnFloor = floorCollision;

                    if (ceilingCollision || floorCollision)
                    {
                        break;
                    }
                }

                if (horizontalCollisionDirection != 0) 
                {
                    if (Math.Sign(entity.Velocity.X) == horizontalCollisionDirection && CanEntityHop(entity, (entity.Position / Globals.TILESIZE).ToPoint(), tileWidth, tileHeight, horizontalCollisionDirection))
                    {
                        entity.Position.Y -= Globals.TILESIZE;
                        entity.Position.X += 2 * Math.Sign(entity.Velocity.X);
                    }
                    else
                    {
                        entity.Velocity.X = 0.0f;
                    }
                }
            }
            for (int i = 0; i < _entities.Count; i++)
            {
                for (int j = i + 1; j < _entities.Count; j++)
                {
                    if ((_entities[i].CollidesWith & _entities[j].Layer) == 0 && (_entities[j].CollidesWith & _entities[i].Layer) == 0)
                        continue;
                    if (!_entities[i].GetBounds().Intersects(_entities[j].GetBounds()))
                        continue;
                    if ((_entities[j].CollidesWith & _entities[i].Layer) != 0)
                        _entities[j].OnCollision(_entities[i]);
                    if ((_entities[i].CollidesWith & _entities[j].Layer) != 0)
                        _entities[i].OnCollision(_entities[j]);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //TODO: add drawing order using SpriteSortMode based on collision layer
            foreach (Entity entity in _entities)
            {
                entity.Draw(spriteBatch);
            }
        }

        private List<Rectangle> GetHorizontalCollisions(Entity entity)
        {
            List<Rectangle> intersections = new();
            int tileWidth = (int)(entity.Size.X) / Globals.TILESIZE;
            int tileHeight = (int)(entity.Size.Y) / Globals.TILESIZE;
            for (int y = 0; y <= tileHeight; y++)
            {
                intersections.Add(new Rectangle(
                    (int)(entity.Position.X / Globals.TILESIZE),
                    (int)((entity.Position.Y + y * (Globals.TILESIZE - 1)) / Globals.TILESIZE),
                    Globals.TILESIZE,
                    Globals.TILESIZE
                ));

                intersections.Add(new Rectangle(
                    (int)((entity.Position.X + entity.Size.X) / Globals.TILESIZE),
                    (int)((entity.Position.Y + y * (Globals.TILESIZE - 1)) / Globals.TILESIZE),
                    Globals.TILESIZE,
                    Globals.TILESIZE
                ));
            }
            intersections.Add(new Rectangle(
                (int)(entity.Position.X / Globals.TILESIZE),
                (int)((entity.Position.Y + entity.Size.Y - 1) / Globals.TILESIZE),
                Globals.TILESIZE,
                Globals.TILESIZE
            ));
            intersections.Add(new Rectangle(
                (int)((entity.Position.X + entity.Size.X) / Globals.TILESIZE),
                (int)((entity.Position.Y + entity.Size.Y - 1) / Globals.TILESIZE),
                Globals.TILESIZE,
                Globals.TILESIZE
            ));

            return intersections;
        }

        private List<Rectangle> GetVerticalCollisions(Entity entity)
        {
            List<Rectangle> intersections = new();
            int tileWidth = (int)(entity.Size.X) / Globals.TILESIZE;
            int tileHeight = (int)(entity.Size.Y) / Globals.TILESIZE;

            for (int x = 0; x <= tileWidth; x++)
            {
                intersections.Add(new Rectangle(
                    (int)((entity.Position.X + x * (Globals.TILESIZE - 1)) / Globals.TILESIZE),
                    (int)(entity.Position.Y / Globals.TILESIZE),
                    Globals.TILESIZE,
                    Globals.TILESIZE
                ));

                intersections.Add(new Rectangle(
                    (int)((entity.Position.X + x * (Globals.TILESIZE - 1)) / Globals.TILESIZE),
                    (int)((entity.Position.Y + entity.Size.Y) / Globals.TILESIZE),
                    Globals.TILESIZE,
                    Globals.TILESIZE
                ));
            }
            intersections.Add(new Rectangle(
                (int)((entity.Position.X + entity.Size.X - 1) / Globals.TILESIZE),
                (int)(entity.Position.Y / Globals.TILESIZE),
                Globals.TILESIZE,
                Globals.TILESIZE
            ));
            intersections.Add(new Rectangle(
                (int)((entity.Position.X + entity.Size.X - 1) / Globals.TILESIZE),
                (int)((entity.Position.Y + entity.Size.Y) / Globals.TILESIZE),
                Globals.TILESIZE,
                Globals.TILESIZE
            ));

            return intersections;
        }

        /// <summary>
        /// Sets the player for the game world, called only once
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer(Player player)
        {
            _player = player;
            _player.InitializeGameUpdates();
        }



        public Player GetPlayer()
        {
            return _player;
        }

        //Change this to spawn by enemy ID
        public void CreateEnemy(int enemyID, Vector2 Position)
        {
            NPC enemy = NPCDatabase.InstantiateNPCByID(enemyID);
            enemy.Position = Position;
            _entities.Add(enemy);
        }

        public void AddItemDrop(Item item, Vector2 position, Vector2 velocity = default)
        {

            ItemDrop itemDrop = new ItemDrop(item, position + new Vector2(Globals.TILESIZE / 2 - ItemDrop.ColliderSize.X / 2, 0));
            itemDrop.Velocity = velocity == default ? Vector2.Zero : velocity;
            _entities.Add(itemDrop);
        }
        public void AddEntity(Entity entity)
        {
            _entities.Add(entity);
        }
        public void RemoveEntity(Entity entity)
        {
            _entities.Remove(entity);
        }
        public List<Entity> GetEntities()
        {
            return _entities;
        }
        private bool CanEntityHop(Entity entity, Point tilePoint, int tileWidth, int tileHeight, int direction)
        {
            if (!entity.IsOnFloor || entity.Velocity.X == 0)
                return false;
            for (int x = 0; x <= tileWidth; x++)
            {
                if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(tilePoint.X + x, tilePoint.Y - 1), TileProperty.Solid))
                    return false;
            }
            int tilesInFrontOffset = direction == -1 ? -1 : tileWidth + 1;
            for (int y = -1; y < tileHeight; y++)
            {
                if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(tilePoint.X + tilesInFrontOffset, tilePoint.Y + y), TileProperty.Solid))
                    return false;
            }
            return true;
        }
    }
}
