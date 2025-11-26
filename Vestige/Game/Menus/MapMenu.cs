using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Entities;
using Vestige.Game.Input;
using Vestige.Game.Time;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;
using Vestige.Game.WorldMap;

namespace Vestige.Game.Menus
{
    internal class MapMenu : UIContainer
    {
        private Button _zoomInButton;
        private Button _zoomOutButton;
        private float _defaultZoom;
        private float _userZoom = 1.0f;
        private float _maxZoom;
        private Vector2 _offset = Vector2.Zero;
        private Vector2 _grabPosition;
        private bool _mouseDown = false;
        private Vector2 _mapPosition;
        private Vector2 _mapPositionOnGrab;
        private Vector2 _mapOrigin;
        private Map _map;
        public MapMenu(Map map)
        {
            _map = map;
            _defaultZoom = Vestige.NativeResolution.X / (float)map.MapRenderTarget.Width;
            _maxZoom = 2.0f / _defaultZoom;
            _maxZoom = (float)Math.Round(_maxZoom, 1);
            _zoomInButton = new Button(Vector2.Zero, "Zoom In", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 60);
            _zoomOutButton = new Button(Vector2.Zero, "Zoom Out", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 60);
            _offset = Vector2.Zero;
            _mapPosition = Vector2.Zero;
            _mapOrigin = new Vector2(map.MapRenderTarget.Width / 2, map.MapRenderTarget.Height / 2);
        }
        public override void HandleInput(InputEvent @event)
        {
            if (@event.InputButton == InputButton.LeftMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                _mouseDown = true;
                _grabPosition = InputManager.GetMouseWindowPosition();
                _mapPositionOnGrab = _mapPosition;
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.LeftMouse && @event.EventType == InputEventType.MouseButtonUp)
            {
                _mouseDown = false;
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.MiddleMouse)
            {
                _userZoom = @event.EventType == InputEventType.MouseButtonDown
                    ? Math.Min(_userZoom + (0.05f * _userZoom), _maxZoom)
                    : Math.Max(_userZoom - (0.05f * _userZoom), 0.5f);
            }
            base.HandleInput(@event);
        }
        public override void Update(double delta)
        {
            if (_mouseDown)
            {
                _offset = InputManager.GetMouseWindowPosition() - _grabPosition;
                _mapPosition = _mapPositionOnGrab + (_offset / (_userZoom * _defaultZoom));
            }
            float mapWidth = _map.MapRenderTarget.Width * (_userZoom * _defaultZoom) * (parentSize.X / Vestige.NativeResolution.X);
            Vector2 clampedPosition = _mapPosition;
            clampedPosition = mapWidth > parentSize.X
                ? Vector2.Clamp(clampedPosition, new Vector2(-(mapWidth - parentSize.X) / 2.0f / (_userZoom * _defaultZoom), clampedPosition.Y), new Vector2((mapWidth - parentSize.X) / 2.0f / (_userZoom * _defaultZoom), clampedPosition.Y))
                : Vector2.Clamp(clampedPosition, new Vector2((-parentSize.X + mapWidth) / 2.0f / (_userZoom * _defaultZoom), clampedPosition.Y), new Vector2((parentSize.X - mapWidth) / 2.0f / (_userZoom * _defaultZoom), clampedPosition.Y));
            float mapHeight = _map.MapRenderTarget.Height * (_userZoom * _defaultZoom) * (parentSize.X / Vestige.NativeResolution.X);
            clampedPosition = mapHeight > parentSize.Y
                ? Vector2.Clamp(clampedPosition, new Vector2(clampedPosition.X, -(mapHeight - parentSize.Y) / 2.0f / (_userZoom * _defaultZoom)), new Vector2(clampedPosition.X, (mapHeight - parentSize.Y) / 2.0f / (_userZoom * _defaultZoom)))
                : Vector2.Clamp(clampedPosition, new Vector2(clampedPosition.X, (-parentSize.Y + mapHeight) / 2.0f / (_userZoom * _defaultZoom)), new Vector2(clampedPosition.X, (parentSize.Y - mapHeight) / 2.0f / (_userZoom * _defaultZoom)));
            if (_mouseDown)
            {
                _mapPositionOnGrab -= _mapPosition - clampedPosition;
            }
            _mapPosition = clampedPosition;
        }
        public override void Draw(SpriteBatch spriteBatch, RasterizerState rasterizerState = null)
        {
            //Draw this without an anchor matrix
            spriteBatch.Begin();
            int globalLight = Main.GameClock.GlobalLight;
            spriteBatch.Draw(Main.DayTimeSkyGradient, new Rectangle(Point.Zero, new Point((int)parentSize.X, (int)parentSize.Y)), new Color(globalLight, globalLight, globalLight));
            spriteBatch.End();
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(parentSize.X / Vestige.NativeResolution.X) * Matrix.CreateTranslation(new Vector3((parentSize / 2.0f) + (_mapPosition * (_userZoom * _defaultZoom)), 0)));
            spriteBatch.Draw(_map.MapRenderTarget, Vector2.Zero, null, Color.White, 0.0f, _mapOrigin, _userZoom * _defaultZoom, SpriteEffects.None, 0.0f);
            //Draw player names
            //TODO: draw head icon
            foreach (Player player in Main.EntityManager.GetPlayers())
            {
                if (player == null || player.Dead) continue;
                Vector2 stringSize = ContentLoader.GameFont.MeasureString(player.Name);
                Vector2 centeredPlayerPosition = player.Position / Vestige.TILESIZE - Main.World.WorldSize.ToVector2() / 2;
                spriteBatch.DrawString(ContentLoader.GameFont, player.Name, centeredPlayerPosition * (_userZoom * _defaultZoom) - stringSize / 2, Color.White);
            }
            spriteBatch.End();
        }
    }
}
