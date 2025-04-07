using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheGreen.Game.Input;
using TheGreen.Game.UIComponents;

namespace TheGreen.Game.Menus
{
    public class MainMenu
    {
        private Label _titleLabel;
        private Button _newGameButton;
        private Button _loadGameButton;
        private Button _backButton;
        private Button _createWorldButton;
        private UIComponentContainer _startMenu;
        private UIComponentContainer _createWorldMenu;
        private Stack<UIComponentContainer> _menus;
        private TheGreen _game;

        //new selector class that has a list of options and will instantiate button components for each selection and store a variable that keeps track of the selected.

        //TODO: export each menu to its own class so this doesn't become a nightmare file
        public MainMenu(TheGreen game, GraphicsDevice graphicsDevice)
        {
            _game = game;
            _menus = new Stack<UIComponentContainer>();


            _startMenu = new UIComponentContainer();
            _createWorldMenu = new UIComponentContainer();

            _titleLabel = new Label(Globals.ScreenCenter.ToVector2() - new Vector2(0, 200), "The Green", Vector2.Zero, textColor: Color.ForestGreen, drawCentered: true, scale: 5.0f);
            _startMenu.AddUIComponent(_titleLabel);

            _newGameButton = new Button(Globals.ScreenCenter.ToVector2(), "New Game", new Vector2(0, 5), borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, drawCentered: true);
            _newGameButton.OnButtonPress += () => AddSubMenu(_createWorldMenu);
            _startMenu.AddUIComponent(_newGameButton);

            _loadGameButton = new Button(Globals.ScreenCenter.ToVector2() + new Vector2(0, 20), "Load Game", new Vector2(0, 5), borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, drawCentered: true);
            _loadGameButton.OnButtonPress += LoadGame;
            _startMenu.AddUIComponent(_loadGameButton);

            Button reduceUIScaleButton = new Button(Globals.ScreenCenter.ToVector2() + new Vector2(0, 40), "Reduce UI Scale", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, drawCentered: true);
            reduceUIScaleButton.OnButtonPress += () => 
            {
                _game.SetUIScaleMatrix(Math.Max(0.1f, TheGreen.UIScaleMatrix.M11 - 0.1f));
            };
            _startMenu.AddUIComponent(reduceUIScaleButton);

            Button increaseUIScaleButton = new Button(Globals.ScreenCenter.ToVector2() + new Vector2(0, 60), "Increase UI Scale", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, drawCentered: true);
            increaseUIScaleButton.OnButtonPress += () =>
            { 
                _game.SetUIScaleMatrix(Math.Min(5f, TheGreen.UIScaleMatrix.M11 + 0.1f));
            };
            _startMenu.AddUIComponent(increaseUIScaleButton);

            _backButton = new Button(new Vector2(150, Globals.NativeResolution.Y - 100), "Back", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, drawCentered: true);
            _backButton.OnButtonPress += () => RemoveSubMenu();
            _createWorldMenu.AddUIComponent( _backButton );

            _createWorldButton = new Button(Globals.ScreenCenter.ToVector2(), "Create World", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, drawCentered: true);
            _createWorldButton.OnButtonPress += () => StartNewGame();
            _createWorldMenu.AddUIComponent( _createWorldButton );

            AddSubMenu(_startMenu);
        }
        private void StartNewGame()
        {
            int numMenus = _menus.Count;
            for (int i = 0; i < numMenus; i++)
            {
                Debug.WriteLine(_menus.Peek().ToString());
                _menus.Pop().Dereference();
            }
            _game.StartNewWorld(new Point(2100, 600));
        }
        private void LoadGame() {
            Debug.WriteLine("You probably want to implement this at some point... (for future reference)");
        }
        private void AddSubMenu(UIComponentContainer menu)
        {
            UIManager.RegisterContainer(menu);
            InputManager.RegisterHandler(menu);
            _menus.Push(menu);
        }
        private void RemoveSubMenu()
        {
            UIManager.UnregisterContainer(_menus.Peek());
            InputManager.UnregisterHandler(_menus.Peek());
            _menus.Pop();
        }
    }
}
