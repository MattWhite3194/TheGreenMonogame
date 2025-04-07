using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Drawables;
using TheGreen.Game.Entities;
using TheGreen.Game.Lighting;
using TheGreen.Game.Renderer;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game
{
    /// <summary>
    /// The instance of the game itself.
    /// </summary>
    public class Main
    {
        private GraphicsDevice _graphicsDevice;
        private static Matrix _translation;
        private TileRenderer _tileRenderer;
        public static LightEngine LightEngine;
        public static readonly Random Random = new Random();
        public static EntityManager EntityManager = null;
        public static ParallaxManager ParallaxManager = null;

        //Debugging Only
        private static Texture2D _pixel;

        public Main(Player player, GraphicsDevice graphicsDevice)
        {

            Globals.StartGameClock(10, 30);
            WorldGen.World.InitializeGameUpdates();
            _graphicsDevice = graphicsDevice;
            EntityManager = new EntityManager();
            ParallaxManager = new ParallaxManager();
            EntityManager.SetPlayer(player);
            EntityManager.CreateEnemy(0, player.Position + new Vector2(500, -100));
            EntityManager.CreateEnemy(0, player.Position + new Vector2(-500, -100));
            _tileRenderer = new TileRenderer();
            LightEngine = new LightEngine(_graphicsDevice);
            ParallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.MountainsBackground, new Vector2(0.01f, 0.001f), EntityManager.GetPlayer().Position, 300 * Globals.TILESIZE, 50 * Globals.TILESIZE));
            ParallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFarthestBackground, new Vector2(0.1f, 0.06f), EntityManager.GetPlayer().Position, 250 * Globals.TILESIZE, 140 * Globals.TILESIZE));
            ParallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFartherBackground, new Vector2(0.2f, 0.08f), EntityManager.GetPlayer().Position, 250 * Globals.TILESIZE, 140 * Globals.TILESIZE));
            ParallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesBackground, new Vector2(0.3f, 0.1f), EntityManager.GetPlayer().Position, 250 * Globals.TILESIZE, 140 * Globals.TILESIZE));

            //Debugging only
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White }); // Fill it with white color
        }
        public void Update(double delta)
        {
            Globals.UpdateGameTime(delta);
            WorldGen.World.Update(delta);
            ParallaxManager.Update(delta, GetCameraPosition());
            EntityManager.Update(delta);
            CalculateTranslation();
        }
        public void Draw(SpriteBatch spriteBatch)
        {

            Point drawBoxMin = new Point(((int)-_translation.Translation.X / Globals.TILESIZE), ((int)-_translation.Translation.Y / Globals.TILESIZE));
            Point drawBoxMax = new Point(((int)-_translation.Translation.X / Globals.TILESIZE) + Globals.DrawDistance.X, ((int)-_translation.Translation.Y / Globals.TILESIZE) + Globals.DrawDistance.Y);
            _tileRenderer.SetDrawBox(drawBoxMin, drawBoxMax);
            LightEngine.SetDrawBox(drawBoxMin, drawBoxMax);
            LightEngine.CalculateLightMap();
            //Render entities scaled twice or thrice to a larger render target, then scale the target down

            float normalizedGlobalLight = (Globals.GlobalLight - 50) / 205.0f;
            _graphicsDevice.Clear(new Color((int)(100 * normalizedGlobalLight), (int)(149 * normalizedGlobalLight), (int)(237 * normalizedGlobalLight)));
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(2.0f));
            ParallaxManager.Draw(spriteBatch, new Color(Globals.GlobalLight, Globals.GlobalLight, Globals.GlobalLight));
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, transformMatrix: _translation * Matrix.CreateScale(2.0f));
            
            _tileRenderer.DrawWalls(spriteBatch);
            //TODO: get rid of this function, sort tiles by depth defined in tile database
            _tileRenderer.DrawBackgroundTiles(spriteBatch);
            _tileRenderer.DrawTiles(spriteBatch);
            _tileRenderer.DrawLiquids(spriteBatch);
            //_tileRenderer.DrawDebug(spriteBatch);
            //TODO: entities also sorted by depth
            EntityManager.Draw(spriteBatch);
            spriteBatch.End();

            
        }
        
        public static void DrawDebugRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height, rect.Width, 1), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width, rect.Y, 1, rect.Height), color);
        }
        public static Vector2 GetCameraPosition()
        {
            return new Vector2(Math.Abs(_translation.Translation.X), Math.Abs(_translation.Translation.Y));
        }
        private void CalculateTranslation()
        {
            Player player = EntityManager.GetPlayer();
            int dx = (int)(Globals.NativeResolution.X / 2 - player.Position.X);
            dx = MathHelper.Clamp(dx, -WorldGen.World.WorldSize.X * Globals.TILESIZE + Globals.NativeResolution.X, 0);
            int dy = (int)(Globals.NativeResolution.Y / 2 - player.Position.Y);
            dy = MathHelper.Clamp(dy, -WorldGen.World.WorldSize.Y * Globals.TILESIZE + Globals.NativeResolution.Y, 0);
            _translation = Matrix.CreateTranslation(dx, dy, 0f);
        }
    }
}
