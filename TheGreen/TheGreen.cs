using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game;
using TheGreen.Game.Entities;
using TheGreen.Game.Input;
using TheGreen.Game.Inventory;
using TheGreen.Game.Menus;
using TheGreen.Game.UIComponents;
using TheGreen.Game.WorldGeneration;

namespace TheGreen
{

    public class TheGreen : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Main _gameManager;
        private RenderTarget2D _gameTarget;
        public static Matrix UIScaleMatrix;
        public static Rectangle RenderDestination;

        public TheGreen()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            
            //Screen settings
            SetWindowProperties(1920, 1080, false);
            _gameTarget = new RenderTarget2D(GraphicsDevice, Globals.NativeResolution.X * 2, Globals.NativeResolution.Y * 2, false, SurfaceFormat.Color, DepthFormat.None);
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnClientSizeChanged;
            //For unlimited fps:
            IsFixedTimeStep = false;
            base.Initialize();
            
        }
        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            UpdateRenderDestination(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            ContentLoader.Load(Content);

            base.LoadContent();
        }
        protected override void BeginRun()
        {
            MainMenu mainMenu = new MainMenu(this, GraphicsDevice);
        }
        protected override void Update(GameTime gameTime)
        {
            //get input
            InputManager.Update();

            //update ui
            UIManager.Update(gameTime.ElapsedGameTime.TotalSeconds);
            //update game
            _gameManager?.Update(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);

        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (_gameManager != null)
            {
                GraphicsDevice.SetRenderTarget(_gameTarget);
                _gameManager.Draw(_spriteBatch);
                GraphicsDevice.SetRenderTarget(null);

                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(_gameTarget, RenderDestination, Color.White);
                _spriteBatch.End();
            }
            

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: UIScaleMatrix);
            UIManager.Draw(_spriteBatch);
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }
        

        private void SetWindowProperties(int width, int height, bool fullScreen)
        {
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.IsFullScreen = fullScreen;
            //_graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();
            UpdateRenderDestination(width, height);
        }
        public void StartNewWorld(Point size)
        {
            InventoryManager inventory = new InventoryManager(5, 8);
            WorldGen.World.Map = new Texture2D(GraphicsDevice, size.X, size.Y);
            WorldGen.World.GenerateWorld(size.X, size.Y);
            Player player = new Player(ContentLoader.PlayerTexture, inventory, 100);
            _gameManager = new Main(player, GraphicsDevice);
        }
        private void UpdateRenderDestination(int width, int height)
        {
            float xScale = width / (float)Globals.NativeResolution.X;
            float yScale = height / (float)Globals.NativeResolution.Y;
            SetUIScaleMatrix(width / (float)Globals.NativeResolution.X);
            float scale = Math.Max(xScale, yScale);
            RenderDestination = new Rectangle(
                width / 2 - (int)(Globals.NativeResolution.X * scale) / 2,
                height / 2 - (int)(Globals.NativeResolution.Y * scale) / 2,
                (int)(Globals.NativeResolution.X * scale),
                (int)(Globals.NativeResolution.Y * scale)
                );
        }
        public void SetUIScaleMatrix(float scale)
        {
            UIScaleMatrix = Matrix.CreateScale(scale);
        }
    }
}
